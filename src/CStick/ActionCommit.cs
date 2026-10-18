using System;
using System.Reflection;
using InControl;

namespace CStick
{
    static class ActionCommit
    {
        static readonly FieldInfo PendingTickField = typeof(OneAxisInputControl).GetField("pendingTick", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo NextStateField = typeof(OneAxisInputControl).GetField("nextState", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo ThisStateField = typeof(OneAxisInputControl).GetField("thisState", BindingFlags.Instance | BindingFlags.NonPublic);

        static bool warned;

        internal static void CommitState(PlayerAction action, bool state)
        {
            if (action == null)
            {
                return;
            }

            // UpdateWithState ORs into nextState and refuses a tick older than pendingTick.
            // Reset nextState so CommitWithState can force the control off, on a tick InControl will accept.
            ResetNextState(action);
            ulong tick = action.UpdateTick;
            ulong pending = ReadPendingTick(action);
            if (pending > tick)
            {
                tick = pending;
            }

            ulong current = InputManager.CurrentTick;
            if (current > tick)
            {
                tick = current;
            }

            try
            {
                action.CommitWithState(state, tick, 0f);
            }
            catch (InvalidOperationException ex)
            {
                Warn(ex.Message);
            }

            if (action.State != state)
            {
                WriteState(action, state);
            }
        }

        static ulong ReadPendingTick(PlayerAction action)
        {
            if (PendingTickField == null)
            {
                return 0;
            }

            object value = PendingTickField.GetValue(action);
            return value is ulong tick ? tick : 0;
        }

        static void ResetNextState(PlayerAction action)
        {
            if (NextStateField == null)
            {
                return;
            }

            object boxed = NextStateField.GetValue(action);
            if (boxed == null)
            {
                return;
            }

            InputControlState next = (InputControlState)boxed;
            next.Reset();
            NextStateField.SetValue(action, next);
        }

        static void WriteState(PlayerAction action, bool state)
        {
            if (ThisStateField == null)
            {
                Warn("Could not override input action state.");
                return;
            }

            InputControlState value = default(InputControlState);
            value.Set(state);
            ThisStateField.SetValue(action, value);
            if (NextStateField != null)
            {
                NextStateField.SetValue(action, value);
            }
        }

        static void Warn(string message)
        {
            if (warned)
            {
                return;
            }

            warned = true;
            UnityEngine.Debug.LogWarning("[C-Stick] " + message);
        }
    }
}
