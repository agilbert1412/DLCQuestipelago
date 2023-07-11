using System.Diagnostics;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using File = System.IO.File;

namespace DLCQuestipelagoInstaller
{
    public class Program
    {
        private const string BEPINEX_EXECUTABLE = "BepInEx.NET.Framework.Launcher.exe";
        private const string DLC_EXECUTABLE = "DLC.exe";
        private const string CONTENT_FOLDER = "Content";
        private const string BEPINEX_MODLOADER_FOLDER = "BepInEx ModLoader";
        private const string DLCQUESTIPELAGO_PLUGIN_FOLDER = "DLCQuestipelago Plugin";
        private const string DEFAULT_MODDED_FOLDER_NAME = "DLCQuestipelago";
        private const string AP_CONNECTION_FILE = "ArchipelagoConnectionInfo.json";

        public static void Main()
        {
            try
            {
                PerformSetup();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The installer has encountered a critical error.\nMessage: '{ex.Message}'\nStack Trace: {ex.StackTrace}");
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static void PerformSetup()
        {
            Console.WriteLine("Welcome to the DLCQuestipelago Installer Wizard");
            Console.WriteLine();
            Console.WriteLine("This setup will install both the BepInEx Modloader and the DLCQuestipelago mod for DLCQuest");
            Console.WriteLine();

            var currentFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var moddedFolder = ChooseDefaultInstallFolder(currentFolder);
            moddedFolder = AskForCustomInstallFolder(moddedFolder);

            Console.WriteLine();

            UnblockEveryFile();

            Console.WriteLine();

            SetupModdedFolder(moddedFolder);
            SetupGame(moddedFolder);
            SetupModLoader(currentFolder, moddedFolder);
            SetupModAndConnectionFile(moddedFolder, currentFolder);

            Console.WriteLine();

            Console.WriteLine($"Your modded DLCQuest is now available at {moddedFolder}. To Run it, Launch \"{BEPINEX_EXECUTABLE}\".");

            SetupDesktopShortcut(moddedFolder);

            Console.WriteLine("Setup Complete. Press any key to exit.");
        }

        private static string ChooseDefaultInstallFolder(string? currentFolder)
        {
            var installFolder = Path.Combine(currentFolder, "..");
            var moddedFolder = Path.Combine(installFolder, DEFAULT_MODDED_FOLDER_NAME);
            moddedFolder = Path.GetFullPath(moddedFolder);
            return moddedFolder;
        }

        private static string AskForCustomInstallFolder(string moddedFolder)
        {
            Console.WriteLine($"Current Install Location is \"{moddedFolder}\"");
            Console.WriteLine(
                "If this is okay, simply press Enter. Otherwise, please enter the path to your preferred directory:");
            var preferredFolder = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(preferredFolder))
            {
                moddedFolder = preferredFolder.Trim('"');
            }

            return moddedFolder;
        }

        private static void UnblockEveryFile()
        {
            Console.WriteLine("Unblocking every file...");
            if (!RunPowerShell("unblock-win.ps1", out var error))
            {
                Console.WriteLine(
                    $"Could not unblock the mod files. Windows may have flagged them as malicious. You will need to unblock them manually by going to each relevant file -> properties -> unblock.");
                Console.WriteLine($"The installer will proceed anyway.");
                Console.WriteLine($"Error Message: {error}");
            }
        }

        private static void SetupModdedFolder(string moddedFolder)
        {
            if (Directory.Exists(moddedFolder))
            {
                return;
            }

            Console.WriteLine("Creating Install Directory...");
            Directory.CreateDirectory(moddedFolder);
        }

        private static void SetupGame(string moddedFolder)
        {
            Console.WriteLine("Copying DLC Quest Game...");
            var dlcQuestFolder = FindDLCQuest();
            CopyFolderContent(dlcQuestFolder, moddedFolder);
        }

        private static void SetupModLoader(string currentFolder, string moddedFolder)
        {
            Console.WriteLine("Creating BepInEx ModLoader Files...");
            var bepInExFolder = Path.Combine(currentFolder, BEPINEX_MODLOADER_FOLDER);
            CopyFolderContent(bepInExFolder, moddedFolder);
        }

        private static void SetupModAndConnectionFile(string moddedFolder, string currentFolder)
        {
            var modFolder = Path.Combine(currentFolder, DLCQUESTIPELAGO_PLUGIN_FOLDER);
            var pluginFolder = Path.Combine(moddedFolder, "BepInEx", "plugins", "DLCQuestipelago");
            SetupMod(modFolder, pluginFolder);
            SetupConnectionFile(pluginFolder, moddedFolder);
        }

        private static void SetupMod(string modFolder, string pluginFolder)
        {
            Console.WriteLine("Installing DLCQuestipelago Mod...");
            CopyFolderContent(modFolder, pluginFolder);
        }

        private static void SetupConnectionFile(string pluginFolder, string moddedFolder)
        {
            Console.Write("Creating Connection File...");
            var apFileOriginalLocation = Path.Combine(pluginFolder, AP_CONNECTION_FILE);
            File.Copy(apFileOriginalLocation, Path.Combine(moddedFolder, AP_CONNECTION_FILE), true);
            File.Delete(apFileOriginalLocation);
        }

        private static void SetupDesktopShortcut(string moddedFolder)
        {
            Console.WriteLine("Do you wish to Create a Desktop Shortcut? [yes/no]");
            var createShortcutAnswer = Console.ReadLine() ?? "";
            if (createShortcutAnswer.Length > 0 && createShortcutAnswer.ToLower().First() == 'y')
            {
                CreateDesktopShortcut(moddedFolder);
            }
        }

        private static string FindDLCQuest()
        {
            var steamInstallRegistryNode = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null);
            if (steamInstallRegistryNode == null)
            {
                steamInstallRegistryNode = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null);
                if (steamInstallRegistryNode == null)
                {
                    return GetManualPathToDLCQuest();
                }
            }

            var steamInstallPath = (string)steamInstallRegistryNode;
            var dlcQuestInstall = Path.Combine(steamInstallPath, "steamapps", "common", "DLC Quest");
            return IsDLCQuestInstall(dlcQuestInstall) ? dlcQuestInstall : GetManualPathToDLCQuest();
        }

        private static string GetManualPathToDLCQuest()
        {
            var dlcQuestFolder = "";
            while (!IsDLCQuestInstall(dlcQuestFolder))
            {
                Console.WriteLine("Could not Find DLC Quest install location. Please enter the full path to it:");
                var manualPath = Console.ReadLine();
                manualPath = manualPath.Trim('"');
                dlcQuestFolder = manualPath;
            }

            return dlcQuestFolder;
        }

        private static bool IsDLCQuestInstall(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (!Directory.Exists(path))
            {
                return false;
            }

            var executablePath = Path.Combine(path, DLC_EXECUTABLE);
            if (!File.Exists(executablePath))
            {
                return false;
            }

            var contentPath = Path.Combine(path, CONTENT_FOLDER);
            if (!Directory.Exists(contentPath))
            {
                return false;
            }

            return true;
        }

        private static void CopyFolderContent(string originFolder, string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            var originEndsWithSeparator = originFolder.EndsWith(Path.DirectorySeparatorChar);
            var directoryEndsWithSeparator = destinationFolder.EndsWith(Path.DirectorySeparatorChar);
            if (originEndsWithSeparator != directoryEndsWithSeparator)
            {
                if (originEndsWithSeparator)
                {
                    originFolder = originFolder.Substring(0, originFolder.Length - 1);
                }
                if (directoryEndsWithSeparator)
                {
                    destinationFolder = destinationFolder.Substring(0, destinationFolder.Length - 1);
                }
            }

            var allFiles = Directory.EnumerateFiles(originFolder, "*", SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var destinationFile = file.Replace(originFolder, destinationFolder);
                var destinationFileFolder = new FileInfo(destinationFile).DirectoryName;
                if (!Directory.Exists(destinationFileFolder))
                {
                    Directory.CreateDirectory(destinationFileFolder);
                }

                File.Copy(file, destinationFile, true);
            }
        }

        private static void CreateDesktopShortcut(string moddedFolder)
        {
            var bepInExPath = Path.Combine(moddedFolder, BEPINEX_EXECUTABLE);
            var dlcExecutablePath = Path.Combine(moddedFolder, DLC_EXECUTABLE);
            try
            {
                Console.WriteLine("Creating Desktop Shortcut...");
                var shDesktop = (object)"Desktop";
                var shell = new WshShell();
                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\DLC Quest - Modded.lnk";
                var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = "Shortcut to Modded DLC Quest";
                shortcut.IconLocation = dlcExecutablePath;
                shortcut.WorkingDirectory = moddedFolder;
                // shortcut.Hotkey = "Ctrl+Shift+N";
                shortcut.TargetPath = bepInExPath;
                shortcut.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create desktop shortcut. Error: {ex.Message}");
                Console.WriteLine($"You can still use your modded DLC Quest by launching the executable at \"{bepInExPath}\"");
            }
        }

        private static bool RunPowerShell(string scriptPath, out string errorMessage)
        {
            try
            {
                var ps1File = scriptPath;

                var startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{ps1File}\"",
                    UseShellExecute = false
                };
                var process = Process.Start(startInfo);
                process?.WaitForExit();
                process?.Close();

                errorMessage = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
