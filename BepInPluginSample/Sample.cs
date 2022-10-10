using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using flanne;
using flanne.Core;
using flanne.Player;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BepInPluginSample
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.0")]
    public class Sample : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        static Harmony harmony;

        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter;
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter2;

        private ConfigEntry<bool> isGUIOn;
        private ConfigEntry<bool> isOpen;
        private ConfigEntry<float> uiW;
        private ConfigEntry<float> uiH;

        public int windowId = 542;
        public Rect windowRect;

        public string title = "";
        public string windowName = ""; // 변수용 
        public string FullName = "Plugin"; // 창 펼쳤을때
        public string ShortName = "P"; // 접었을때

        GUILayoutOption h;
        GUILayoutOption w;
        public Vector2 scrollPosition;

        // =========================================================

        private static ConfigEntry<bool> hpChg;
        private static ConfigEntry<bool> useAmmo;
        private static ConfigEntry<bool> onlyWin;
        private static ConfigEntry<float> movementSpeed;
        private static ConfigEntry<float> xpMulti;
        private static ConfigEntry<float> pickupRangeAdd;
        private static ConfigEntry<float> visionRangeAdd;

        public void Awake()
        {
            logger = Logger;
            Logger.LogMessage("Awake");

            ShowCounter = Config.Bind("GUI", "isGUIOnKey", new KeyboardShortcut(KeyCode.Keypad0));// 이건 단축키
            ShowCounter2 = Config.Bind("GUI", "isOpenKey", new KeyboardShortcut(KeyCode.KeypadPeriod));// 이건 단축키

            isGUIOn = Config.Bind("GUI", "isGUIOn", true);
            isOpen = Config.Bind("GUI", "isOpen", true);
            isOpen.SettingChanged += IsOpen_SettingChanged;
            uiW = Config.Bind("GUI", "uiW", 300f);
            uiH = Config.Bind("GUI", "uiH", 600f);

            if (isOpen.Value)
                windowRect = new Rect(Screen.width - 65, 0, uiW.Value, 800);
            else
                windowRect = new Rect(Screen.width - uiW.Value, 0, uiW.Value, 800);

            IsOpen_SettingChanged(null, null);

            // =========================================================

            onlyWin = Config.Bind("game", "onlyWin", true);
            hpChg = Config.Bind("game", "hpChg", false);
            useAmmo = Config.Bind("game", "useAmmo", false);
            movementSpeed = Config.Bind("game", "movementSpeed", 8f);
            xpMulti = Config.Bind("game", "xpMulti", 2f);
            pickupRangeAdd = Config.Bind("game", "pickupRangeAdd", 9f);
            visionRangeAdd = Config.Bind("game", "visionRangeAdd", 9f);
        }

        public void IsOpen_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"IsOpen_SettingChanged {isOpen.Value} , {isGUIOn.Value},{windowRect.x} ");
            if (isOpen.Value)
            {
                title = ShowCounter.Value.ToString() + "," + ShowCounter2.Value.ToString();
                h = GUILayout.Height(uiH.Value);
                w = GUILayout.Width(uiW.Value);
                windowName = FullName;
                windowRect.x -= (uiW.Value - 64);
            }
            else
            {
                title = "";
                h = GUILayout.Height(40);
                w = GUILayout.Width(60);
                windowName = ShortName;
                windowRect.x += (uiW.Value - 64);
            }
        }


        public void OnEnable()
        {
            Logger.LogWarning("OnEnable");
            // 하모니 패치
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony = Harmony.CreateAndPatchAll(typeof(Sample));
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public void Update()
        {
            if (ShowCounter.Value.IsUp())// 단축키가 일치할때
            {
                isGUIOn.Value = !isGUIOn.Value;
            }
            if (ShowCounter2.Value.IsUp())// 단축키가 일치할때
            {
                isOpen.Value = !isOpen.Value;
            }
        }


        public void OnGUI()
        {
            if (!isGUIOn.Value)
                return;

            // 창 나가는거 방지
            windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + 4, Screen.width - 4);
            windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + 4, Screen.height - 4);
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunction, windowName, w, h);
        }

        public virtual void WindowFunction(int id)
        {
            GUI.enabled = true; // 기능 클릭 가능

            GUILayout.BeginHorizontal();// 가로 정렬
                                        // 라벨 추가
                                        //GUILayout.Label(windowName, GUILayout.Height(20));
                                        // 안쓰는 공간이 생기더라도 다른 기능으로 꽉 채우지 않고 빈공간 만들기
            if (isOpen.Value) GUILayout.Label(title);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { isOpen.Value = !isOpen.Value; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { isGUIOn.Value = false; }
            GUI.changed = false;

            GUILayout.EndHorizontal();// 가로 정렬 끝

            if (!isOpen.Value) // 닫혔을때
            {
            }
            else // 열렸을때
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

                // 여기에 항목 작성
                // =========================================================

                if (GUILayout.Button($"add point 100000 : {PointsTracker.pts}")) {
                    PointsTracker.pts += 100000;
                    if (SaveSystem.data != null)
                    {
                        SaveSystem.data.points = PointsTracker.pts;
                        SaveSystem.Save();
                    }
                }


                if (GUILayout.Button($"onlyWin {onlyWin.Value}")) { onlyWin.Value = !onlyWin.Value; }
                if (GUILayout.Button($"hpChg {hpChg.Value}")) { hpChg.Value = !hpChg.Value; }

                if (GUILayout.Button($"useAmmo {useAmmo.Value}")) { useAmmo.Value = !useAmmo.Value; }

                GUILayout.BeginHorizontal();
                GUILayout.Label($"xp Multiplier {xpMulti.Value}");
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { xpMulti.Value += 1; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { xpMulti.Value -= 1; }
                GUILayout.EndHorizontal();

                if (GameTimer.SharedInstance != null)
                {
                    GUILayout.Label($"timer : {GameTimer.SharedInstance.timer}");
                    GUILayout.Label($"timeLimit : {GameTimer.SharedInstance.timeLimit}");
                }
                else
                {
                    GUILayout.Label("GameTimer null");
                }
                if (PlayerController.Instance != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"move Speed : {PlayerController.Instance.movementSpeed}");
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        movementSpeed.Value += 1;
                        PlayerController.Instance.movementSpeed += 1;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        movementSpeed.Value -= 1;
                        PlayerController.Instance.movementSpeed -= 1;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label($"move Speed Modify : {PlayerController.Instance.stats[StatType.MoveSpeed].Modify(PlayerController.Instance.movementSpeed)}");

                    GUILayout.Label($"move Speed Multiplier : {PlayerController.Instance.moveSpeedMultiplier}");

                    //GameObject.FindGameObjectWithTag("Pickupper").transform.localScale = Vector3.one * PlayerController.Instance.stats[StatType.PickupRange].Modify(1f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Pickupper AddMultiplierBonus : {PlayerController.Instance.stats[StatType.PickupRange].Modify(1f)}");
                    //GUILayout.Label($"baseValue * (1f + multiplierBonus) * multiplierReduction + flatBonus");
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        pickupRangeAdd.Value += 1;
                        PlayerController.Instance.stats[StatType.PickupRange].AddMultiplierBonus(1);
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        pickupRangeAdd.Value -= 1;
                        PlayerController.Instance.stats[StatType.PickupRange].AddMultiplierBonus(-1);
                    }
                    GUILayout.EndHorizontal();

                    //GameObject.FindGameObjectWithTag("PlayerVision").transform.localScale = Vector3.one * PlayerController.Instance.stats[StatType.VisionRange].Modify(1f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"VisionRange AddMultiplierBonus : {PlayerController.Instance.stats[StatType.VisionRange].Modify(1f)}");
                    //GUILayout.Label($"baseValue * (1f + multiplierBonus) * multiplierReduction + flatBonus");
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        visionRangeAdd.Value += 1;
                        PlayerController.Instance.stats[StatType.VisionRange].AddMultiplierBonus(1);
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        visionRangeAdd.Value -= 1;
                        PlayerController.Instance.stats[StatType.VisionRange].AddMultiplierBonus(-1);
                    }
                    GUILayout.EndHorizontal();

                }
                else
                {
                    GUILayout.Label("PlayerController null");
                }


                // =========================================================

                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
        }

        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();
        }

        // ====================== 하모니 패치 샘플 ===================================

        [HarmonyPatch(typeof(Health), "HPChange")]
        [HarmonyPrefix]
        public static void HPChange(Health __instance, ref int change)
        {
            if (hpChg.Value || PlayerController.Instance.playerHealth != __instance)
            {
                return;
            }
            //logger.LogWarning($"HPChange {__instance.maxHP} , {change}");
            change = __instance.maxHP / 2;
        }

        [HarmonyPatch(typeof(Ammo), "UseAmmo")]
        [HarmonyPrefix]
        public static void UseAmmo(Ammo __instance, ref int a)
        {
            if (useAmmo.Value)
            {
                return;
            }
            //logger.LogWarning($"UseAmmo {__instance.amount} , {a}");
            a = 0;
        }

        [HarmonyPatch(typeof(PlayerXP), "GainXP")]
        [HarmonyPrefix]
        public static void GainXP(ref float amount, float ___xp)//PlayerXP __instance,
        {
            //logger.LogWarning($"GainXP {___xp} , {amount}");
            amount *= xpMulti.Value;
        }
        /*
        [HarmonyPatch(typeof(StatMod), "Modify")]
        [HarmonyPrefix]
        public static void Modify(ref float baseValue)
        {
            
        }
        */

        private static void NewMethod<T>(string name, StatMod __instance, ref T value)
        {
            if (PlayerController.Instance.stats[StatType.VisionRange] == __instance)
            {
                logger.LogWarning($"{name} VisionRange {value}");
            }
            else if (PlayerController.Instance.stats[StatType.PickupRange] == __instance)
            {
                logger.LogWarning($"{name} PickupRange {value}");
            }
            else if (PlayerController.Instance.stats[StatType.MoveSpeed] == __instance)
            {
                logger.LogWarning($"{name} MoveSpeed {value}");
            }
            else if (PlayerController.Instance.stats[StatType.WalkSpeed] == __instance)
            {
                logger.LogWarning($"{name} WalkSpeed {value}");
            }
            else if (PlayerController.Instance.stats[StatType.SummonDamage] == __instance)
            {
                logger.LogWarning($"{name} SummonDamage {value}");
            }
            else if (PlayerController.Instance.stats[StatType.SummonAttackSpeed] == __instance)
            {
                logger.LogWarning($"{name} SummonAttackSpeed {value}");
            }
            else if (PlayerController.Instance.stats[StatType.BulletDamage] == __instance)
            {
                logger.LogWarning($"{name} BulletDamage {value}");
            }
            else
            {
                logger.LogWarning($"{name} {value}");
            }
        }
        /*
        [HarmonyPatch(typeof(StatMod), "AddFlatBonus")]
        [HarmonyPrefix]
        public static void AddFlatBonus(StatMod __instance, ref int value)
        {
            NewMethod("AddFlatBonus",__instance, ref value);
        }

        [HarmonyPatch(typeof(StatMod), "AddMultiplierBonus")]
        [HarmonyPrefix]
        public static void AddMultiplierBonus(StatMod __instance, ref float value)
        {
            NewMethod("AddMultiplierBonus",__instance, ref value);
        }

        [HarmonyPatch(typeof(StatMod), "AddMultiplierReduction")]
        [HarmonyPrefix]
        public static void AddMultiplierReduction(StatMod __instance, ref float value)
        {
            NewMethod("AddMultiplierReduction",__instance, ref value);
        }
        */

        /// <summary>
        /// 부하가 걸림
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="s"></param>
        // [HarmonyPatch(typeof(StatsHolder), "Item", MethodType.Getter)]
        // [HarmonyPrefix]
        // public static void GetItem(StatsHolder __instance, StatType s)
        // {
        //     logger.LogWarning($"GetItem {s}");
        // }

        [HarmonyPatch(typeof(PlayerController), "Awake")]
        [HarmonyPrefix]
        public static void Awake(PlayerController __instance)
        {
            logger.LogWarning($"PlayerController.Awake");
        }

        [HarmonyPatch(typeof(PlayerController), "Awake")]
        [HarmonyPrefix]
        public static void AwakePost(PlayerController __instance)
        {
            logger.LogWarning($"PlayerController.AwakePost");
        }

        [HarmonyPatch(typeof(StatsHolder), "Awake")]
        [HarmonyPrefix]
        public static void Awake(StatsHolder __instance)
        {
            logger.LogWarning($"StatsHolder.Awake");
        }

        [HarmonyPatch(typeof(StatsHolder), "Awake")]
        [HarmonyPostfix]
        public static void AwakePost(StatsHolder __instance)
        {
            logger.LogWarning($"StatsHolder.AwakePost");

        }

        [HarmonyPatch(typeof(PlayerBuffs), "Start")]
        [HarmonyPrefix]
        public static void Start(PlayerBuffs __instance)
        {
            logger.LogWarning($"PlayerBuffs.Start");
        }

        [HarmonyPatch(typeof(PlayerBuffs), "Start")]
        [HarmonyPostfix]
        public static void StartPost(PlayerBuffs __instance)
        {
            logger.LogWarning($"PlayerBuffs.StartPost");

            PlayerController.Instance.movementSpeed += movementSpeed.Value;

            PlayerController.Instance.stats[StatType.PickupRange].AddMultiplierBonus(pickupRangeAdd.Value);
            GameObject.FindGameObjectWithTag("Pickupper").transform.localScale = Vector3.one * PlayerController.Instance.stats[StatType.PickupRange].Modify(1f);

            PlayerController.Instance.stats[StatType.VisionRange].AddMultiplierBonus(visionRangeAdd.Value);
            GameObject.FindGameObjectWithTag("PlayerVision").transform.localScale = Vector3.one * PlayerController.Instance.stats[StatType.VisionRange].Modify(1f);
        }

        [HarmonyPatch(typeof(BuffPlayerStats), "Start")]
        [HarmonyPrefix]
        public static void Start(BuffPlayerStats __instance)
        {
            logger.LogWarning($"BuffPlayerStats.Start");
        }

        [HarmonyPatch(typeof(BuffPlayerStats), "Start")]
        [HarmonyPostfix]
        public static void StartPost(BuffPlayerStats __instance)
        {
            logger.LogWarning($"BuffPlayerStats.StartPost");
        }

        [HarmonyPatch(typeof(CombatState), "OnDeath")]
        [HarmonyPrefix]
        public static bool OnDeath(CombatState __instance, GameController ___owner)
        {
            logger.LogWarning($"CombatState.OnDeath");
            // this.owner.ChangeState<PlayerDeadState>();
            if (onlyWin.Value)
            {
                ___owner.ChangeState<KillEnemiesState>();
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CombatState), "OnTimerReached")]
        [HarmonyPostfix]
        public static void OnTimerReached(CombatState __instance)
        {
            logger.LogWarning($"CombatState.OnTimerReached");
            // this.owner.ChangeState<KillEnemiesState>();
        }
    }

    // =========================================================
}

