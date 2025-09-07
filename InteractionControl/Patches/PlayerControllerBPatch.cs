using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine.InputSystem;

namespace InteractionControl.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("Interact_performed")]
        [HarmonyPrefix]
        static bool GrabWithJetpackPatch(PlayerControllerB __instance, ref InputAction.CallbackContext context)
        {
            // Knock back player
            if (InteractionControlMod.configGrabWithJetpack.Value)
            {
                if (__instance.IsOwner && __instance.isPlayerDead && (!__instance.IsServer || __instance.isHostPlayerObject))
                {
                    if (!StartOfRound.Instance.overrideSpectateCamera && __instance.spectatedPlayerScript != null && !__instance.spectatedPlayerScript.isPlayerDead)
                    {
                        __instance.SpectateNextPlayer();
                    }
                }
                else
                {
                    if (((!__instance.IsOwner || !__instance.isPlayerControlled || (__instance.IsServer && !__instance.isHostPlayerObject)) && !__instance.isTestingPlayer) || !context.performed || __instance.timeSinceSwitchingSlots < 0.2f || __instance.inSpecialMenu)
                    {
                        return false;
                    }
                    ShipBuildModeManager.Instance.CancelBuildMode();
                    if (!__instance.isGrabbingObjectAnimation && !__instance.isTypingChat && !__instance.inTerminalMenu && !__instance.throwingObject && !__instance.IsInspectingItem && !(__instance.inAnimationWithEnemy != null) && (__instance.jetpackControls || InteractionControlMod.configGrabWithJetpack.Value) && (__instance.disablingJetpackControls || InteractionControlMod.configGrabWithJetpack.Value) && !StartOfRound.Instance.suckingPlayersOutOfShip)
                    {
                        if (!__instance.activatingItem && !__instance.waitingToDropItem)
                        {
                            __instance.BeginGrabObject();
                        }
                        if (!(__instance.hoveringOverTrigger == null) && !__instance.hoveringOverTrigger.holdInteraction && (!__instance.isHoldingObject || __instance.hoveringOverTrigger.oneHandedItemAllowed) && (!__instance.twoHanded || (__instance.hoveringOverTrigger.twoHandedItemAllowed && !__instance.hoveringOverTrigger.specialCharacterAnimation)) && __instance.InteractTriggerUseConditionsMet())
                        {
                            __instance.hoveringOverTrigger.Interact(__instance.thisPlayerBody);
                        }
                    }
                }
            }
            return false;
        }
    }
}
