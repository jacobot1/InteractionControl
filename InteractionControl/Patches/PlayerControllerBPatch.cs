using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine.InputSystem;
using UnityEngine;

namespace InteractionControl.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("Interact_performed")]
        [HarmonyPrefix]
        static bool GrabPatch(PlayerControllerB __instance, ref InputAction.CallbackContext context)
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
                if (!__instance.isGrabbingObjectAnimation && !__instance.isTypingChat && !__instance.inTerminalMenu && !__instance.throwingObject && !__instance.IsInspectingItem && !(__instance.inAnimationWithEnemy != null) && (!__instance.jetpackControls || InteractionControlMod.configGrabWithJetpack.Value) && (!__instance.disablingJetpackControls || InteractionControlMod.configGrabWithJetpack.Value) && !StartOfRound.Instance.suckingPlayersOutOfShip)
                {
                    InteractionControlMod.mls.LogInfo("ALLOWED interaction");
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
            // if (__instance.jetpackControls || __instance.disablingJetpackControls)
            // {
            //     __instance.DisableJetpackControlsLocally();
            // }
            return false;
        }
        [HarmonyPatch("InteractTriggerUseConditionsMet")]
        [HarmonyPrefix]
        static bool TriggerPatch(PlayerControllerB __instance, ref bool __result)
        {
            if (__instance.sinkingValue > 0.73f)
            {
                __result = false;
                return false;
            }
            if ((__instance.jetpackControls && !InteractionControlMod.configTriggerWithJetpack.Value) && (__instance.hoveringOverTrigger.specialCharacterAnimation || __instance.hoveringOverTrigger.isLadder))
            {
                InteractionControlMod.mls.LogInfo("Blocked interaction while in jetpack mode.");
                __result = false;
                return false;
            }
            if (__instance.isClimbingLadder)
            {
                if (__instance.hoveringOverTrigger.isLadder)
                {
                    if (!__instance.hoveringOverTrigger.usingLadder)
                    {
                        __result = false;
                        return false;
                    }
                }
                else if (__instance.hoveringOverTrigger.specialCharacterAnimation)
                {
                    __result = false;
                    return false;
                }
            }
            else if (__instance.inSpecialInteractAnimation && !__instance.hoveringOverTrigger.allowUseWhileInAnimation)
            {
                __result = false;
                return false;
            }
            if (__instance.disableInteract)
            {
                __result = false;
                return false;
            }
            if (__instance.hoveringOverTrigger.isPlayingSpecialAnimation)
            {
                __result = false;
                return false;
            }
            __result = true;
            return false;
        }
        [HarmonyPatch("ScrollMouse_performed")]
        [HarmonyPrefix]
        static bool ScrollMousePatch(PlayerControllerB __instance, ref InputAction.CallbackContext context)
        {
            if (__instance.inTerminalMenu)
            {
                float num = context.ReadValue<float>();
                __instance.terminalScrollVertical.value += num / 3f;
            }
            else if (((__instance.IsOwner && __instance.isPlayerControlled && (!__instance.IsServer || __instance.isHostPlayerObject)) || __instance.isTestingPlayer) && !(__instance.timeSinceSwitchingSlots < 0.3f) && !__instance.isGrabbingObjectAnimation && !__instance.quickMenuManager.isMenuOpen && !__instance.inSpecialInteractAnimation && !__instance.throwingObject && !__instance.isTypingChat && !__instance.twoHanded && !__instance.activatingItem && (!__instance.jetpackControls || InteractionControlMod.configSwitchSlotsWithJetpack.Value) && (!__instance.disablingJetpackControls || InteractionControlMod.configSwitchSlotsWithJetpack.Value))
            {
                ShipBuildModeManager.Instance.CancelBuildMode();
                __instance.playerBodyAnimator.SetBool("GrabValidated", value: false);
                if (context.ReadValue<float>() > 0f)
                {
                    __instance.SwitchToItemSlot(__instance.NextItemSlot(forward: true));
                    __instance.SwitchItemSlotsServerRpc(forward: true);
                }
                else
                {
                    __instance.SwitchToItemSlot(__instance.NextItemSlot(forward: false));
                    __instance.SwitchItemSlotsServerRpc(forward: false);
                }
                if (__instance.currentlyHeldObjectServer != null)
                {
                    __instance.currentlyHeldObjectServer.gameObject.GetComponent<AudioSource>().PlayOneShot(__instance.currentlyHeldObjectServer.itemProperties.grabSFX, 0.6f);
                }
                __instance.timeSinceSwitchingSlots = 0f;
            }
            return false;
        }
    }
}
