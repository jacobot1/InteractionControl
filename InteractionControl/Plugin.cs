using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using InteractionControl.Patches;
using BepInEx.Configuration;

namespace InteractionControl
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    public class InteractionControlMod : BaseUnityPlugin
    {
        // Mod metadata
        public const string modGUID = "com.jacobot5.InteractionControl";
        public const string modName = "InteractionControl";
        public const string modVersion = "1.0.0";

        // Initalize Harmony
        private readonly Harmony harmony = new Harmony(modGUID);

        // Create static instance
        private static InteractionControlMod Instance;

        // Configuration
        public static ConfigEntry<bool> configGrabWithJetpack;

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
            configGrabWithJetpack = Config.Bind("General.Toggles",
                                                "GrabWithJetpack",
                                                false,
                                                "Whether to enable grabbing objects while in jetpack control mode");
            // Do the patching
            harmony.PatchAll(typeof(InteractionControlMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
        }
    }
}
