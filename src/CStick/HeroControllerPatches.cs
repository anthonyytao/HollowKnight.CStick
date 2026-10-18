using System.Diagnostics;
using System.Reflection;
using GlobalEnums;
using HarmonyLib;
using InControl;

namespace CStick
{
    [HarmonyPatch(typeof(HeroController), "Update")]
    static class UpdatePatch
    {
        static void Prefix(HeroController __instance)
        {
            CStickPlugin.SuppressCancelOnce = false;
            if (__instance.cState != null && !__instance.cState.attacking)
            {
                CStickPlugin.ProtectReverse = false;
            }

            if (!CStickPlugin.InGameplay(__instance))
            {
                return;
            }

            InputDevice device = InputManager.ActiveDevice;
            InputHandler handler = InputHandler.Instance;
            if (device == null || handler == null || handler.inputActions == null)
            {
                return;
            }

            HeroActions actions = handler.inputActions;
            float dpadUp = device.GetControl(InputControlType.DPadUp).Value;
            float dpadDown = device.GetControl(InputControlType.DPadDown).Value;
            float stickUp = device.GetControl(InputControlType.LeftStickUp).Value;
            float stickDown = device.GetControl(InputControlType.LeftStickDown).Value;

            ActionCommit.CommitState(actions.rs_up, false);
            ActionCommit.CommitState(actions.rs_down, false);

            if (dpadUp > CStickPlugin.AxisThreshold)
            {
                ActionCommit.CommitState(actions.rs_up, true);
                if (!(stickUp > CStickPlugin.AxisThreshold))
                {
                    ActionCommit.CommitState(actions.up, false);
                }
            }

            if (dpadDown > CStickPlugin.AxisThreshold)
            {
                ActionCommit.CommitState(actions.rs_down, true);
                if (!(stickDown > CStickPlugin.AxisThreshold))
                {
                    ActionCommit.CommitState(actions.down, false);
                }
            }
        }
    }

    [HarmonyPatch(typeof(HeroController), "LookForInput")]
    static class LookForInputPatch
    {
        static void Postfix(HeroController __instance)
        {
            if (!CStickPlugin.InGameplay(__instance))
            {
                return;
            }

            InputHandler handler = InputHandler.Instance;
            if (handler == null || handler.inputActions == null)
            {
                return;
            }

            HeroActions actions = handler.inputActions;
            float vertical = actions.up.Value - actions.down.Value;
            if (vertical > CStickPlugin.AxisThreshold)
            {
                vertical = 1f;
            }
            else if (vertical < -CStickPlugin.AxisThreshold)
            {
                vertical = -1f;
            }
            else
            {
                vertical = 0f;
            }

            __instance.vertical_input = vertical;
        }
    }

    [HarmonyPatch(typeof(HeroController), "LookForQueueInput")]
    static class LookForQueueInputPatch
    {
        static void Postfix(HeroController __instance)
        {
            FlickDirection current = ReadRightStick();
            bool edge = StickFlick.IsNewFlick(CStickPlugin.PreviousFlick, current);
            if (!CStickPlugin.CanTrackStick(__instance) || current == FlickDirection.None || (CStickPlugin.QueuedFlick != FlickDirection.None && current != CStickPlugin.QueuedFlick))
            {
                ClearQueue();
            }
            else if (edge && CanBuffer(__instance))
            {
                CStickPlugin.QueuedFlick = current;
                CStickPlugin.QueuedSteps = 0;
            }
            else if (edge && CStickPlugin.InGameplay(__instance))
            {
                TrySwing(__instance, current);
            }

            if (CStickPlugin.QueuedFlick != FlickDirection.None)
            {
                if (CStickPlugin.QueuedSteps > CStickPlugin.FlickQueueSteps)
                {
                    ClearQueue();
                }
                else if (TrySwing(__instance, CStickPlugin.QueuedFlick))
                {
                    ClearQueue();
                }
            }

            CStickPlugin.PreviousFlick = current;
        }

        static void ClearQueue()
        {
            CStickPlugin.QueuedFlick = FlickDirection.None;
            CStickPlugin.QueuedSteps = 0;
        }

        static bool StickBlocked(HeroController hero)
        {
            if (hero.cState == null)
            {
                return true;
            }

            InputHandler handler = InputHandler.Instance;
            if (handler == null || handler.inputActions == null)
            {
                return true;
            }

            return hero.cState.wallSliding || hero.cState.nailCharging || handler.inputActions.attack.IsPressed;
        }

        static bool CanBuffer(HeroController hero)
        {
            if (StickBlocked(hero))
            {
                return false;
            }

            if (hero.cState.dashing || hero.cState.recoiling || hero.cState.recoilFrozen)
            {
                return true;
            }

            return hero.hero_state == ActorStates.hard_landing || hero.hero_state == ActorStates.dash_landing;
        }

        static FlickDirection ReadRightStick()
        {
            InputDevice device = InputManager.ActiveDevice;
            if (device == null)
            {
                return FlickDirection.None;
            }

            return StickFlick.FromMagnitudes(
                device.GetControl(InputControlType.RightStickUp).Value,
                device.GetControl(InputControlType.RightStickDown).Value,
                device.GetControl(InputControlType.RightStickLeft).Value,
                device.GetControl(InputControlType.RightStickRight).Value);
        }

        static bool TrySwing(HeroController hero, FlickDirection direction)
        {
            if (direction == FlickDirection.None || StickBlocked(hero))
            {
                return false;
            }

            if (!HeroAttack.CanAttack(hero))
            {
                return false;
            }

            float previousVertical = hero.vertical_input;
            try
            {
                switch (direction)
                {
                    case FlickDirection.Up:
                        hero.vertical_input = 1f;
                        HeroAttack.DoAttack(hero);
                        break;
                    case FlickDirection.Down:
                        hero.vertical_input = -1f;
                        HeroAttack.DoAttack(hero);
                        break;
                    case FlickDirection.Left:
                        hero.FaceLeft();
                        hero.vertical_input = 0f;
                        HeroAttack.DoAttack(hero);
                        CStickPlugin.ProtectReverse = true;
                        break;
                    case FlickDirection.Right:
                        hero.FaceRight();
                        hero.vertical_input = 0f;
                        HeroAttack.DoAttack(hero);
                        CStickPlugin.ProtectReverse = true;
                        break;
                }
            }
            finally
            {
                hero.vertical_input = previousVertical;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(HeroController), "FixedUpdate")]
    static class FixedUpdatePatch
    {
        static void Prefix()
        {
            if (CStickPlugin.QueuedFlick != FlickDirection.None)
            {
                CStickPlugin.QueuedSteps++;
            }
        }
    }

    [HarmonyPatch(typeof(HeroController), "FlipSprite")]
    static class FlipSpritePatch
    {
        static bool Prefix(HeroController __instance)
        {
            if (!CStickPlugin.ProtectReverse || __instance.cState == null || !__instance.cState.attacking)
            {
                return true;
            }

            // FixedUpdate turns the knight back toward move_input with FlipSprite()
            // then CancelAttack(). That pair is the only flip that should swallow a
            // cancel. Wall slides also call FlipSprite from FixedUpdate, then
            // CancelWallsliding(), and Update does the same. Arming the guard there
            // would drop the next real cancel, including a hit.
            if (!__instance.cState.wallSliding && !__instance.wallLocked && CalledFromFixedUpdate("FlipSprite"))
            {
                CStickPlugin.SuppressCancelOnce = true;
            }

            return false;
        }

        internal static bool CalledFromFixedUpdate(string patchedMethod)
        {
            StackFrame[] frames = new StackTrace(false).GetFrames();
            if (frames == null)
            {
                return false;
            }

            bool skippedPatch = false;
            for (int i = 0; i < frames.Length; i++)
            {
                MethodBase method = frames[i].GetMethod();
                if (method == null)
                {
                    continue;
                }

                System.Type type = method.DeclaringType;
                if (type == null)
                {
                    continue;
                }

                string space = type.Namespace ?? "";
                if (space == "CStick" || space.StartsWith("HarmonyLib") || space.StartsWith("MonoMod") || type.Name.Contains("Harmony"))
                {
                    continue;
                }

                if (type != typeof(HeroController))
                {
                    return false;
                }

                string name = method.Name;
                if (!skippedPatch && (name == patchedMethod || name.StartsWith(patchedMethod + "_")))
                {
                    skippedPatch = true;
                    continue;
                }

                return name == "FixedUpdate";
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(HeroController), "CancelAttack")]
    static class CancelAttackPatch
    {
        static bool Prefix()
        {
            if (!CStickPlugin.SuppressCancelOnce)
            {
                return true;
            }

            CStickPlugin.SuppressCancelOnce = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(HeroController), "ResetAttacks")]
    static class ResetAttacksPatch
    {
        static void Postfix()
        {
            CStickPlugin.ProtectReverse = false;
        }
    }
}
