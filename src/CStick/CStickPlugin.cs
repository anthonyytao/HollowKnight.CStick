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
        internal static FlickDirection QueuedFlick;
        internal static int QueuedSteps;

        // HeroController.ATTACK_QUEUE_STEPS. FixedUpdate increments it, so five steps is about a tenth of a second.
        internal const int FlickQueueSteps = 5;

        private void Awake()
        {
            _ = HeroAttack.CanAttack;
            new HarmonyLib.Harmony(PluginId).PatchAll(typeof(CStickPlugin).Assembly);
            Logger.LogInfo("C-Stick loaded.");
        }

        internal static bool InGameplay(HeroController hero)
        {
            return PlaySessionOpen(hero) && hero.hero_state != ActorStates.no_input;
        }

        internal static bool CanTrackStick(HeroController hero)
        {
            if (!PlaySessionOpen(hero))
            {
                return false;
            }

            if (hero.hero_state != ActorStates.no_input)
            {
                return true;
            }

            return hero.cState != null && (hero.cState.recoiling || hero.cState.recoilFrozen);
        }

        static bool PlaySessionOpen(HeroController hero)
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

            if (manager.gameState != GameState.PLAYING || manager.isPaused || !manager.IsGameplayScene())
            {
                return false;
            }

            if (!hero.acceptingInput || hero.controlReqlinquished)
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
