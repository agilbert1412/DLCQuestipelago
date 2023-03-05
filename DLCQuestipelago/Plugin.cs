using BepInEx;
using BepInEx.NetLauncher;
using BepInEx.NetLauncher.Common;
using HarmonyLib;

namespace DLCQuestipelago
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        private Harmony _harmony;

        public override void Load()
        {
            // Plugin startup logic
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            _harmony = new Harmony(PluginInfo.PLUGIN_NAME);
            _harmony.PatchAll();

            DLCPackPatch.Initialize(Log);
        }
    }
}
