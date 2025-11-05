using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using InteractionControl.Patches;
using BepInEx.Configuration;

namespace InteractionControl
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class InteractionControlMod : BaseUnityPlugin
    {
        // Mod metadata
        public const string modGUID = "com.jacobot5.InteractionControl";
        public const string modName = "InteractionControl";
        public const string modVersion = "1.2.0";

        // Initalize Harmony
        private readonly Harmony harmony = new Harmony(modGUID);

        // Create static instance
        private static InteractionControlMod Instance;

        // Configuration
        public static ConfigEntry<bool> configGrabWithJetpack;
        public static ConfigEntry<bool> configTriggerWithJetpack;
        public static ConfigEntry<bool> configSwitchSlotsWithJetpack;

        // Initialize logging
        public static ManualLogSource mls;

        private void Awake()
        {
            // Ensure static instance
            if (Instance == null)
            {
                Instance = this;
            }

            // Send alive message
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("InteractionControl has awoken.");

            // Bind configuration
            configGrabWithJetpack = Config.Bind("Jetpack",
                                                "GrabWithJetpack",
                                                false,
                                                "Whether to enable grabbing objects while in jetpack control mode");
            configSwitchSlotsWithJetpack = Config.Bind("Jetpack",
                                                "SwitchSlotsWithJetpack",
                                                false,
                                                "Whether to enable switching item slots while in jetpack control mode (RECOMMENDED to enable if using GrabWithJetpack)");
            configTriggerWithJetpack = Config.Bind("Jetpack",
                                                "TriggerWithJetpack",
                                                false,
                                                "Whether to enable triggering things while in jetpack control mode (IGNORE unless you have issues)");
            // Do the patching
            harmony.PatchAll(typeof(InteractionControlMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(KickIfModNotInstalled));
        }
    }
}
