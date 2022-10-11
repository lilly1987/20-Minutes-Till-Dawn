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
        #region GUI
        public static ManualLogSource logger;

        static Harmony harmony;

        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> isGUIOnKey;
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> isOpenKey;

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
        #endregion

        #region 변수
        // =========================================================

        private static ConfigEntry<bool> hpChg;
        private static ConfigEntry<bool> useAmmo;
        private static ConfigEntry<bool> onlyWin;
        private static ConfigEntry<bool> CanReroll;
        private static ConfigEntry<bool> powerup;
        private static ConfigEntry<bool> maxHPNotZero;
        private static ConfigEntry<bool> startTime;
        private static ConfigEntry<bool> autoPowerup;
        private static ConfigEntry<float> movementSpeed;
        private static ConfigEntry<float> xpMulti;
        private static ConfigEntry<float> pickupRangeAdd;
        private static ConfigEntry<float> visionRangeAdd;
        private static ConfigEntry<float> enemySpeedMultiplier;
        private static ConfigEntry<int> swawmMulti;
        private static ConfigEntry<int> hpMulti;
        private static ConfigEntry<int> numTimeRepeatable;

        // =========================================================
        #endregion

        public void Awake()
        {
            #region GUI
            logger = Logger;
            Logger.LogMessage("Awake");

            isGUIOnKey = Config.Bind("GUI", "isGUIOnKey", new KeyboardShortcut(KeyCode.Keypad0));// 이건 단축키
            isOpenKey = Config.Bind("GUI", "isOpenKey", new KeyboardShortcut(KeyCode.KeypadPeriod));// 이건 단축키

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
            #endregion

            #region 변수
            // =========================================================
            powerup = Config.Bind("game", "powerup", true);
            onlyWin = Config.Bind("game", "onlyWin", true);
            CanReroll = Config.Bind("game", "CanReroll", true);
            maxHPNotZero = Config.Bind("game", "maxHPNotZero", true);
            startTime = Config.Bind("game", "startTime", true);
            hpChg = Config.Bind("game", "hpChg", false);
            useAmmo = Config.Bind("game", "useAmmo", false);
            autoPowerup = Config.Bind("game", "autoPowerup", true);
            movementSpeed = Config.Bind("game", "movementSpeed", 8f);
            xpMulti = Config.Bind("game", "xpMulti", 2f);
            pickupRangeAdd = Config.Bind("game", "pickupRangeAdd", 9f);
            visionRangeAdd = Config.Bind("game", "visionRangeAdd", 9f);
            enemySpeedMultiplier = Config.Bind("game", "enemySpeedMultiplier", 1f);
            swawmMulti = Config.Bind("game", "swawmMulti", 2);
            hpMulti = Config.Bind("game", "hpMulti", 2);
            numTimeRepeatable = Config.Bind("game", "numTimeRepeatable", 6);
            // =========================================================
            #endregion

            PrereqsMet = AccessTools.Method(typeof(PowerupGenerator), "PrereqsMet");
        }

        #region GUI
        public void IsOpen_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"IsOpen_SettingChanged {isOpen.Value} , {isGUIOn.Value},{windowRect.x} ");
            if (isOpen.Value)
            {
                title = isGUIOnKey.Value.ToString() + "," + isOpenKey.Value.ToString();
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
        #endregion

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
            #region GUI
            if (isGUIOnKey.Value.IsUp())// 단축키가 일치할때
            {
                isGUIOn.Value = !isGUIOn.Value;
            }
            if (isOpenKey.Value.IsUp())// 단축키가 일치할때
            {
                if (isGUIOn.Value)
                {
                    isOpen.Value = !isOpen.Value;
                }
                else
                {
                    isGUIOn.Value = true;
                    isOpen.Value = true;
                }
            }
            #endregion
        }

        #region GUI
        public void OnGUI()
        {
            if (!isGUIOn.Value)
                return;

            // 창 나가는거 방지
            windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + 4, Screen.width - 4);
            windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + 4, Screen.height - 4);
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunction, windowName, w, h);
        }
        #endregion

        public virtual void WindowFunction(int id)
        {
            #region GUI
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
                #endregion

                // 여기에 항목 작성
                // =========================================================

                if (GUILayout.Button($"add point 100000 : {PointsTracker.pts}"))
                {
                    PointsTracker.pts += 100000;
                    if (SaveSystem.data != null)
                    {
                        SaveSystem.data.points = PointsTracker.pts;
                        SaveSystem.Save();
                    }
                }

                if (GUILayout.Button($"CanReroll {CanReroll.Value}")) { CanReroll.Value = !CanReroll.Value; }

                if (GUILayout.Button($"onlyWin {onlyWin.Value}")) { onlyWin.Value = !onlyWin.Value; }

                if (GameTimer.SharedInstance != null)
                {
                    GUILayout.Label($"timer : {GameTimer.SharedInstance.timer}");
                    GUILayout.Label($"timeLimit : {GameTimer.SharedInstance.timeLimit}");
                }
                else
                {
                    GUILayout.Label("GameTimer null");
                }

                if (GUILayout.Button($"hpChg {hpChg.Value}")) { hpChg.Value = !hpChg.Value; }
                
                if (GUILayout.Button($"maxHPNotZero {maxHPNotZero.Value}")) { maxHPNotZero.Value = !maxHPNotZero.Value; }

                if (GUILayout.Button($"useAmmo {useAmmo.Value}")) { useAmmo.Value = !useAmmo.Value; }

                GUILayout.BeginHorizontal();
                GUILayout.Label($"xp Multiplier {xpMulti.Value}");
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { xpMulti.Value += 1; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { xpMulti.Value -= 1; }
                GUILayout.EndHorizontal();

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

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Pickupper AddMultiplierBonus : {PlayerController.Instance.stats[StatType.PickupRange].Modify(1f)}");
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

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"VisionRange AddMultiplierBonus : {PlayerController.Instance.stats[StatType.VisionRange].Modify(1f)}");
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

                    #region powerup

                    GUILayout.Label($"--- powerup --- {takenPowerups.Count}");
                    if (GUILayout.Button($"autoPowerup {autoPowerup.Value}")) { autoPowerup.Value = !autoPowerup.Value; }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"numTimeRepeatable : {numTimeRepeatable.Value}");
                    if (GUILayout.Button("3", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        numTimeRepeatable.Value = 3;
                    }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        numTimeRepeatable.Value += 1;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        numTimeRepeatable.Value -= 1;
                    }
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button($"powerup menu {powerup.Value}")) { powerup.Value = !powerup.Value; }
                    if (powerup.Value)
                    {
                        if (GUILayout.Button($"All Add One"))
                        {
                            List<Powerup> l = new List<Powerup>();
                            foreach (PowerupPoolItem item in powerupPool)
                            {
                                PowerupSet(l, item.powerup);
                            }
                        }
                        foreach (PowerupPoolItem item in powerupPool)
                        {
                            //GUILayout.Label($"{powerupPoolsNm[item]} {powerupPools[item]} / {item.numTimeRepeatable}");
                            if (GUILayout.Button($"{powerupPoolsNm[item]} {powerupPools[item]} / {item.numTimeRepeatable}"))
                            {
                                PlayerController.Instance.playerPerks.Equip(item.powerup);
                                PowerupGenerator.Instance.RemoveFromPool(item.powerup);
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    GUILayout.Label("PlayerController null");
                }

                #region enemy
                GUILayout.Label("--- enemy ---");

                if (GUILayout.Button($"startTime 1/2 {startTime.Value}")) { startTime.Value = !startTime.Value; }
                GUILayout.BeginHorizontal();
                GUILayout.Label($"swawmMulti : {swawmMulti.Value}");
                //GUILayout.Label($"baseValue * (1f + multiplierBonus) * multiplierReduction + flatBonus");
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    swawmMulti.Value += 1;
                    SpawnSessionsSet();
                }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    swawmMulti.Value -= 1;
                    if (swawmMulti.Value < 1)
                    {
                        swawmMulti.Value = 1;
                    }
                    SpawnSessionsSet();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"hpMulti : {hpMulti.Value}");
                //GUILayout.Label($"baseValue * (1f + multiplierBonus) * multiplierReduction + flatBonus");
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    hpMulti.Value += 1;
                    SpawnSessionsHPSet();
                }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    hpMulti.Value -= 1;
                    if (hpMulti.Value < 1)
                    {
                        hpMulti.Value = 1;
                    }
                    SpawnSessionsHPSet();
                }
                GUILayout.EndHorizontal();

                if (hordeSpawner != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"speedMultiplier : {enemySpeedMultiplier.Value} , {hordeSpawner.speedMultiplier}");
                    //GUILayout.Label($"baseValue * (1f + multiplierBonus) * multiplierReduction + flatBonus");
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        enemySpeedMultiplier.Value += 1;
                        hordeSpawner.speedMultiplier += 1;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        enemySpeedMultiplier.Value -= 1;
                        hordeSpawner.speedMultiplier -= 1;
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label($"speedMultiplier null");
                }
                #endregion

                // =========================================================

                #region GUI
                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
            #endregion
        }

        #region PowerupSet

        private static void PowerupSet(List<Powerup> list, Powerup item)
        {
            if (item.powerupTreeUIData != null)
            {
                PowerupSet2(list, item.powerupTreeUIData.startingPowerup);
                PowerupSet2(list, item.powerupTreeUIData.rightPowerup);
                PowerupSet2(list, item.powerupTreeUIData.leftPowerup);
            }
            PowerupSet2(list, item);
        }

        private static void PowerupSet2(List<Powerup> list, Powerup item)
        {
            if (item == null)
            {
                return;
            }
            if (!list.Contains(item))
            {
                try
                {
                    PlayerController.Instance.playerPerks.Equip(item);
                }
                catch (Exception e)
                {
                    logger.LogError("NewMethod1 : " + e);
                }
                PowerupGenerator.Instance.RemoveFromPool(item);
                list.Add(item);
            }

        }

        #endregion

        public void OnDisable()
        {
            logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();
        }

        // ====================== 하모니 패치 샘플 ===================================

        #region HP

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

        
        [HarmonyPatch(typeof(Health), "maxHP", MethodType.Setter)]
        [HarmonyPrefix]
        public static void baseMaxHP(Health __instance, ref int value)
        {
            if (!maxHPNotZero.Value || PlayerController.Instance.playerHealth != __instance)
            {
                return;
            }
            logger.LogWarning($"HPChange {__instance.maxHP} , {value}");
            if (value<1)
            {
                value = 1;
            }
        }

        #endregion

        #region UseAmmo
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
        #endregion

        #region GainXP

        [HarmonyPatch(typeof(PlayerXP), "GainXP")]
        [HarmonyPrefix]
        public static void GainXP(ref float amount, float ___xp)//PlayerXP __instance,
        {
            //logger.LogWarning($"GainXP {___xp} , {amount}");
            amount *= xpMulti.Value;
        }
        #endregion

        #region stats

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

            logger.LogWarning($"activeSpawners {spawnSessions.Count}");
        }

        #endregion

        /*
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

        [HarmonyPatch(typeof(CombatState), "OnTimerReached")]
        [HarmonyPostfix]
        public static void OnTimerReached(CombatState __instance)
        {
            logger.LogWarning($"CombatState.OnTimerReached");
            // this.owner.ChangeState<KillEnemiesState>();
        }
        */

        #region onlyWin
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
        #endregion

        #region spawn

        static List<SpawnSession> spawnSessionsBak;
        static List<SpawnSession> spawnSessions;
        static List<SpawnSession> endlessSpawnSessionsBak;
        static List<SpawnSession> endlessSpawnSessions;

        [HarmonyPatch(typeof(MapInitializer), "Start")]
        [HarmonyPostfix]
        public static void Start(MapInitializer __instance)
        {
            logger.LogWarning($"MapInitializer.Start {SelectedMap.MapData.endless} ,  {SelectedMap.MapData.numPowerupsRepeat} ");

            spawnSessionsBak = (spawnSessions = SelectedMap.MapData.spawnSessions).Copy();
            endlessSpawnSessionsBak = (endlessSpawnSessions = SelectedMap.MapData.endlessSpawnSessions).Copy();


            /*
            [Message: Lilly] BrainMonster; 24; 20; 4; 3; 0; 60; False; 0;
            [Message: Lilly] BrainMonster; 24; 50; 10; 4; 60; 60; False; 0;
            [Message: Lilly] Boomer; 30; 2; 1; 4; 60; 60; False; 0;
            [Message: Lilly] BrainMonster; 30; 200; 7; 2; 120; 240; False; 0;
            [Message: Lilly] Boomer; 30; 10; 2; 5; 120; 240; False; 0;
            [Message: Lilly] ElderBrain; 1000; 1; 1; 10; 180; 5; True; 0;
            [Message: Lilly] BrainMonster; 80; 400; 12; 2; 360; 120; False; 0;
            [Message: Lilly] EyeMonster; 400; 2; 2; 10; 360; 120; False; 0;
            [Message: Lilly] Boomer; 60; 1; 1; 1; 420; 120; False; 0;
            [Message: Lilly] BrainMonster; 100; 600; 16; 1; 480; 120; False; 0;
            [Message: Lilly] Lamprey; 300; 30; 3; 1; 605; 55; False; 0;
            [Message: Lilly] EyeMonster; 300; 12; 2; 1; 660; 120; False; 0;
            [Message: Lilly] Lamprey; 350; 100; 5; 1; 660; 120; False; 0;
            [Message: Lilly] Boomer; 240; 4; 2; 1; 660; 120; False; 0;
            [Message: Lilly] SpawnerMonster; 10000; 1; 1; 10; 680; 5; True; 0;
            [Message: Lilly] WingedMonster; 18000; 1; 1; 10; 960; 5; True; 0;
            [Message: Lilly] EyeMonster; 350; 16; 2; 1; 900; 60; False; 0;
            [Message: Lilly] Boomer; 400; 20; 2; 1; 900; 60; False; 0;
            [Message: Lilly] Lamprey; 500; 300; 14; 1; 780; 115; False; 0;
            [Message: Lilly] BrainMonster; 250; 600; 26; 1; 960; 120; False; 0;
            [Message: Lilly] Lamprey; 600; 300; 20; 1; 1080; 119; False; 0;
            [Message: Lilly] EyeMonster; 500; 10; 1; 1; 1080; 119; False; 0;
            */
            for (int i = 0; i < spawnSessions.Count; i++)
            {
                spawnSessions[i].HP = spawnSessionsBak[i].HP * hpMulti.Value;
                spawnSessions[i].maximum = spawnSessionsBak[i].maximum * swawmMulti.Value;
                spawnSessions[i].numPerSpawn = spawnSessionsBak[i].numPerSpawn * swawmMulti.Value;
            }
            for (int i = 0; i < endlessSpawnSessions.Count; i++)
            {
                endlessSpawnSessions[i].HP = endlessSpawnSessionsBak[i].HP * hpMulti.Value;
                endlessSpawnSessions[i].maximum = endlessSpawnSessionsBak[i].maximum * swawmMulti.Value;
                endlessSpawnSessions[i].numPerSpawn = endlessSpawnSessionsBak[i].numPerSpawn * swawmMulti.Value;
            }
            if (startTime.Value)
            {
                for (int i = 0; i < spawnSessions.Count; i++)
                {
                    spawnSessions[i].startTime = spawnSessionsBak[i].startTime / 2f;
                }
                for (int i = 0; i < endlessSpawnSessions.Count; i++)
                {
                    endlessSpawnSessions[i].startTime = endlessSpawnSessionsBak[i].startTime / 2f;
                }
            }

            foreach (var item in spawnSessions)
            {
                logger.LogMessage($"{item.monsterPrefab.name} ; {item.HP} ; {item.maximum} ; {item.numPerSpawn} ; {item.spawnCooldown} ; {item.startTime} ; {item.duration} ; {item.isElite} ; {item.timer} ;");
            }
        }

        private static void SpawnSessionsSet()
        {
            if (spawnSessions == null)
            {
                return;
            }
            logger.LogMessage("--- SpawnSessionsSet ST ---");
            for (int i = 0; i < spawnSessions.Count; i++)
            {
                spawnSessions[i].maximum = spawnSessionsBak[i].maximum * swawmMulti.Value;
                spawnSessions[i].numPerSpawn = spawnSessionsBak[i].numPerSpawn * swawmMulti.Value;
            }
            for (int i = 0; i < endlessSpawnSessions.Count; i++)
            {
                endlessSpawnSessions[i].maximum = endlessSpawnSessionsBak[i].maximum * swawmMulti.Value;
                endlessSpawnSessions[i].numPerSpawn = endlessSpawnSessionsBak[i].numPerSpawn * swawmMulti.Value;
            }
            logger.LogMessage("--- SpawnSessionsSet ED ---");
        }

        private static void SpawnSessionsHPSet()
        {
            if (spawnSessions == null)
            {
                return;
            }
            logger.LogMessage("--- SpawnSessionsHPSet ST ---");
            for (int i = 0; i < spawnSessions.Count; i++)
            {
                spawnSessions[i].HP = spawnSessionsBak[i].HP * hpMulti.Value;
            }
            for (int i = 0; i < endlessSpawnSessions.Count; i++)
            {
                endlessSpawnSessions[i].HP = endlessSpawnSessionsBak[i].HP * hpMulti.Value;
            }
            logger.LogMessage("--- SpawnSessionsHPSet ED ---");
        }

        static HordeSpawner hordeSpawner;

        [HarmonyPatch(typeof(HordeSpawner), "Awake")]
        [HarmonyPostfix]
        public static void Awake(HordeSpawner __instance, List<SpawnSession> ___activeSpawners)
        {
            logger.LogWarning($"HordeSpawner.Awake {___activeSpawners.Count} {__instance.speedMultiplier}");
            // this.owner.ChangeState<KillEnemiesState>();
            spawnSessions = ___activeSpawners;
            hordeSpawner = __instance;

            hordeSpawner.speedMultiplier += enemySpeedMultiplier.Value;
        }

        /// <summary>
        /// 게임이 종료될때 출력됨
        /// </summary>
        /// <param name="__instance"></param>
        //        [HarmonyPatch(typeof(SpawnSession), MethodType.Constructor)]
        //        [HarmonyPostfix]
        //        public static void SpawnSessionCont(SpawnSession __instance)
        //        {
        //            logger.LogWarning($"SpawnSession ; {__instance.HP} ; {__instance.maximum} ;  {__instance.numPerSpawn} ; {__instance.spawnCooldown} ; {__instance.startTime} ; {__instance.duration} ; {__instance.isElite} ; {__instance.timer} ; ");
        //
        //        }

        #endregion

        #region Powerup

        static List<Powerup> takenPowerups = new List<Powerup>();
        static List<PowerupPoolItem> powerupPool = new List<PowerupPoolItem>();
        static List<PowerupPoolItem> powerupPoolOri = new List<PowerupPoolItem>();
        static Dictionary<PowerupPoolItem, int> powerupPools = new Dictionary<PowerupPoolItem, int>();
        static Dictionary<PowerupPoolItem, string> powerupPoolsNm = new Dictionary<PowerupPoolItem, string>();

        [HarmonyPatch(typeof(PowerupGenerator), "InitPowerupPool")]
        [HarmonyPostfix]
        public static void InitPowerupPool(ref int numTimesRepeatable, List<PowerupPoolItem> ___powerupPool, List<Powerup> ___takenPowerups)//PowerupGenerator __instance, 
        {
            logger.LogWarning($"InitPowerupPool {numTimesRepeatable} , {___powerupPool.Count} , {___takenPowerups.Count} ");
            takenPowerups = ___takenPowerups;
            powerupPool = ___powerupPool.ToList();
            powerupPoolOri = ___powerupPool;
            foreach (PowerupPoolItem item in powerupPool)
            {
                logger.LogWarning($"{item.powerup.nameString} {item.numTimeRepeatable}");
                powerupPools[item] = 0;
                powerupPoolsNm[item] = item.powerup.nameString;
                item.numTimeRepeatable = numTimeRepeatable.Value;
            }
        }

        [HarmonyPatch(typeof(PowerupGenerator), "RemoveFromPool")]
        [HarmonyPostfix]
        public static void RemoveFromPool(Powerup powerup)//PowerupGenerator __instance,
        {
            logger.LogWarning($"RemoveFromPool {powerup.nameString} ");
            PowerupPoolItem powerupPoolItem = powerupPool.Find((PowerupPoolItem x) => x.powerup == powerup);
            if (powerupPoolItem == null)
            {
                return;
            }
            powerupPools[powerupPoolItem]++;
        }

        static System.Reflection.MethodInfo PrereqsMet;

        [HarmonyPatch(typeof(PowerupMenuState), "Enter")]
        [HarmonyPrefix]
        public static bool Enter(PowerupMenuState __instance)//PowerupGenerator __instance,
        {
            logger.LogWarning($"PowerupMenuState.Enter");
            
            if (autoPowerup.Value && powerupPoolOri.Count>0)
            {
                PowerupPoolItem powerupPoolItem = null;
                while (true)
                {
                    powerupPoolItem = powerupPoolOri[UnityEngine.Random.Range(0, powerupPoolOri.Count)];
                    if ((bool)PrereqsMet.Invoke(powerupGenerator, new object[] { powerupPoolItem.powerup }))
                    {
                        AccessTools.Method(typeof(PowerupMenuState), "OnConfirm").Invoke(__instance, new object[] { null, powerupPoolItem.powerup });
                        return false;
                    }
                }
                
                /*
                PlayerController.Instance.playerPerks.Equip(powerupPoolItem.powerup);
                PowerupGenerator.Instance.RemoveFromPool(powerupPoolItem.powerup);
                */
                //return false;
            }
            return true;
        }


        #endregion

        #region CanReroll

        static PowerupGenerator powerupGenerator;

        [HarmonyPatch(typeof(PowerupGenerator), "Awake")]
        [HarmonyPostfix]
        public static void PowerupGeneratorAwake(PowerupGenerator __instance)//
        {
            powerupGenerator = __instance;
            logger.LogWarning($"PowerupGenerator.Awake {PowerupGenerator.CanReroll} ");
            if (CanReroll.Value)
            {
                PowerupGenerator.CanReroll = true;
            }

        }

        [HarmonyPatch(typeof(PowerupMenuState), "OnReroll")]
        [HarmonyPrefix]
        public static bool OnReroll(PowerupMenuState __instance, GameController ___owner)
        {
            logger.LogWarning($"PowerupMenuState.OnReroll");
            if (CanReroll.Value)
            {
                AccessTools.Method(typeof(PowerupMenuState), "GeneratePowerups").Invoke(__instance, null);
                //__instance.GeneratePowerups();            
                //___owner.powerupRerollButton.gameObject.SetActive(false);
                ___owner.powerupMenuPanel.SelectDefault();
                return false;
            }
            return true;
        }

        #endregion

        // =========================================================
    }

}

