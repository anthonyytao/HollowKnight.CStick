using BepInEx;
using GlobalEnums;
using UnityEngine;

namespace CStick
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    public class CStickPlugin : BaseUnityPlugin
    {
        public const string PluginId = "com.anthonyytao.hollowknight.cstick";
        public const string PluginName = "C-Stick";
        public const string PluginVersion = "1.0.0";

        internal const float AxisThreshold = 0.5f;

        internal static bool ProtectReverse;
        internal static bool SuppressCancelOnce;
        internal static FlickDirection PreviousFlick;

        private void Awake()
        {
            _ = HeroAttack.CanAttack;
            new HarmonyLib.Harmony(PluginId).PatchAll(typeof(CStickPlugin).Assembly);
            Logger.LogInfo("C-Stick loaded.");
        }

        internal static bool InGameplay(HeroController hero)
        {
            if (hero == null)
            {
                return false;
            }

            GameManager manager = GameManager.instance;
            if (manager == null || HeroController.instance == null)
            {
                return false;
            }

            if (manager.gameState != GameState.PLAYING || manager.isPaused)
            {
                return false;
            }

            if (!hero.acceptingInput || hero.controlReqlinquished)
            {
                return false;
            }

            if (hero.hero_state == ActorStates.no_input)
            {
                return false;
            }

            if (!manager.IsGameplayScene())
            {
                return false;
            }

            return !MapCanPan(manager);
        }

        static bool MapCanPan(GameManager manager)
        {
            GameObject mapObject = manager.gameMap;
            if (mapObject == null)
            {
                return false;
            }

            GameMap map = mapObject.GetComponent<GameMap>();
            return map != null && map.canPan;
        }
    }
}
