using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using NamedSaves.Utilities;

namespace NamedSaves
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Options Options { get; } = OptionsPanelHandler.RegisterModOptions<Options>();

        public static ManualLogSource? Log;

        internal const string GUID = "com.visolar6.namedsaves";

        internal const string Name = "Named Saves";

        internal const string Version = "1.0.0";

        private readonly Harmony _harmony = new(GUID);

        /// <summary>
        /// Awakes the plugin (on game start).
        /// </summary>
        public void Awake()
        {
            Log = Logger;
        }

        public void Start()
        {
            Log?.LogInfo($"Patching hooks...");
            _harmony.PatchAll();

            Log?.LogInfo($"Patching localization...");
            LanguagesHandler.GlobalPatch();

            Log?.LogInfo($"Initialized!");
        }
    }
}
