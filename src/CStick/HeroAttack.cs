using System;
using System.Reflection;

namespace CStick
{
    static class HeroAttack
    {
        internal static readonly Action<HeroController> DoAttack;
        internal static readonly Func<HeroController, bool> CanAttack;

        static HeroAttack()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            MethodInfo doAttack = typeof(HeroController).GetMethod("DoAttack", flags, null, Type.EmptyTypes, null);
            MethodInfo canAttack = typeof(HeroController).GetMethod("CanAttack", flags, null, Type.EmptyTypes, null);
            if (doAttack == null || canAttack == null)
            {
                throw new MissingMethodException("HeroController.DoAttack or HeroController.CanAttack was not found.");
            }

            DoAttack = (Action<HeroController>)Delegate.CreateDelegate(typeof(Action<HeroController>), doAttack);
            CanAttack = (Func<HeroController, bool>)Delegate.CreateDelegate(typeof(Func<HeroController, bool>), canAttack);
        }
    }
}
