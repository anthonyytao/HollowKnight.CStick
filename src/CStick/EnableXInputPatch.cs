using HarmonyLib;
using InControl;

namespace CStick
{
    // Unity 6 enables InControl's native module, which skips XInput. Steam Input
    // feeds an XInput pad, and the native module never sees those buttons.
    [HarmonyPatch(typeof(InControlManager), "OnEnable")]
    static class EnableXInputPatch
    {
        static void Prefix(InControlManager __instance)
        {
            __instance.enableNativeInput = false;
            __instance.enableXInput = true;
        }
    }
}
