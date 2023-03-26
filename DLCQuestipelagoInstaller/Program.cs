using IWshRuntimeLibrary;
using Microsoft.Win32;
using File = System.IO.File;

namespace DLCQuestipelagoInstaller
{
    public class Program
    {
        private const string BEPINEX_EXECUTABLE = "BepInEx.NetLauncher.exe";
        private const string DLC_EXECUTABLE = "DLC.exe";
        private const string CONTENT_FOLDER = "Content";
        private const string BEPINEX_MODLOADER_FOLDER = "BepInEx ModLoader";
        private const string DLCQUESTIPELAGO_PLUGIN_FOLDER = "DLCQuestipelago Plugin";
        private const string DEFAULT_MODDED_FOLDER_NAME = "DLCQuestipelago";

        public static void Main()
        {
            Console.WriteLine("Welcome to the DLCQuestipelago Installer Wizard");
            Console.WriteLine();
            Console.WriteLine("This setup will install both the BepInEx Modloader and the DLCQuestipelago mod for DLCQuest");
            Console.WriteLine();
            var currentFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var installFolder = Path.Combine(currentFolder, "..");
            var moddedFolder = Path.Combine(installFolder, DEFAULT_MODDED_FOLDER_NAME);
            moddedFolder = Path.GetFullPath(moddedFolder);

            Console.WriteLine($"Current Install Location is \"{moddedFolder}\"");
            Console.WriteLine("If this is okay, simply press Enter. Otherwise, please enter the path to your preferred directory:");
            var preferredFolder = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(preferredFolder))
            {
                moddedFolder = preferredFolder;
            }
            Console.WriteLine();

            if (!Directory.Exists(moddedFolder))
            {
                Console.WriteLine("Creating Install Directory...");
                Directory.CreateDirectory(moddedFolder);
            }

            var bepInExFolder = Path.Combine(currentFolder, BEPINEX_MODLOADER_FOLDER);
            var modFolder = Path.Combine(currentFolder, DLCQUESTIPELAGO_PLUGIN_FOLDER);
            var dlcQuestFolder = FindDLCQuest();

            Console.WriteLine("Copying DLC Quest Game...");
            CopyFolderContent(dlcQuestFolder, moddedFolder);
            Console.WriteLine("Creating BepInEx ModLoader Files...");
            CopyFolderContent(bepInExFolder, moddedFolder);
            var pluginFolder = Path.Combine(moddedFolder, "BepInEx", "plugins", "DLCQuestipelago");
            Console.WriteLine("Installing DLCQuestipelago Mod...");
            CopyFolderContent(modFolder, pluginFolder);

            Console.WriteLine();

            Console.WriteLine($"Your modded DLCQuest is now available at {moddedFolder}. To Run it, Launch \"{BEPINEX_EXECUTABLE}\".");
            Console.WriteLine("Do you wish to Create a Desktop Shortcut? [yes/no]");
            var createShortcutAnswer = Console.ReadLine() ?? "";
            if (createShortcutAnswer.Length > 0 && createShortcutAnswer.ToLower().First() == 'y')
            {
                CreateDesktopShortcut(moddedFolder);
            }

            Console.WriteLine("Setup Complete. Press any key to exit.");
            Console.ReadKey();
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

            var allFiles = Directory.EnumerateFiles(originFolder, "*", SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var destinationFile = file.Replace(originFolder, destinationFolder);
                var destinationFileFolder = new FileInfo(destinationFile).DirectoryName;
                if (!Directory.Exists(destinationFileFolder))
                {
                    Directory.CreateDirectory(destinationFileFolder);
                }

                File.Copy(file, destinationFile);
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
    }
}
