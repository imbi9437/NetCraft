// using UnityEngine;
// using _Project.Script.Generic;
// using _Project.Script.Data;
// using _Project.Script.EventStruct;
//
// namespace _Project.Script.UI
// {
//     /// <summary>
//     /// UI 이벤트 테스트 클래스
//     /// 데이터 변경 시 UI가 자동으로 업데이트되는지 테스트
//     /// </summary>
//     public class UIEventTest : MonoBehaviour
//     {
//         [Header("테스트 데이터")]
//         [SerializeField] private PlayerData testPlayerData;
//         [SerializeField] private GameTimeData testGameData;
//
//         [Header("테스트 설정")]
//         [SerializeField] private bool enableAutoTest = false;
//         [SerializeField] private float testInterval = 2f;
//         [SerializeField] private float healthChangeAmount = -5f;
//         [SerializeField] private float hungerChangeAmount = -3f;
//         [SerializeField] private float thirstChangeAmount = -2f;
//
//         private float lastTestTime;
//
//         private void Start()
//         {
//             InitializeTestData();
//         }
//
//         private void Update()
//         {
//             if (enableAutoTest && Time.time - lastTestTime >= testInterval)
//             {
//                 RunAutoTest();
//                 lastTestTime = Time.time;
//             }
//         }
//
//         /// <summary>
//         /// 테스트 데이터 초기화
//         /// </summary>
//         private void InitializeTestData()
//         {
//             // 플레이어 데이터 초기화
//             testPlayerData = new PlayerData();
//             testPlayerData.maxHp = 100f;
//             testPlayerData.maxHunger = 100f;
//             testPlayerData.maxThirst = 100f;
//             testPlayerData.hp = 100f;
//             testPlayerData.hunger = 100f;
//             testPlayerData.thirst = 100f;
//
//             // 게임 데이터 초기화
//             testGameData = new GameTimeData();
//             testGameData.gameTime = 12.5f;
//             testGameData.dayNumber = 15;
//             testGameData.currentSeason = Season.Spring;
//             testGameData.currentWeather = WeatherType.Clear;
//
//             Debug.Log("[UIEventTest] 테스트 데이터 초기화 완료");
//         }
//
//         /// <summary>
//         /// 자동 테스트 실행
//         /// </summary>
//         private void RunAutoTest()
//         {
//             // 플레이어 상태 변경 테스트
//             testPlayerData.hp += healthChangeAmount;
//             testPlayerData.hunger += hungerChangeAmount;
//             testPlayerData.thirst += thirstChangeAmount;
//
//             // 게임 시간 진행
//             testGameData.gameTime += 0.5f;
//             if (testGameData.gameTime >= 24f)
//             {
//                 testGameData.gameTime = 0f;
//                 testGameData.dayNumber++;
//             }
//
//             Debug.Log($"[UIEventTest] 자동 테스트 실행 - HP: {testPlayerData.hp:F1}, Hunger: {testPlayerData.hunger:F1}, Thirst: {testPlayerData.thirst:F1}");
//         }
//
//         /// <summary>
//         /// 수동 테스트 버튼들
//         /// </summary>
//         [ContextMenu("체력 감소 테스트")]
//         public void TestHealthDecrease()
//         {
//             testPlayerData.TakeDamage(10f);
//             Debug.Log($"[UIEventTest] 체력 감소 테스트 - 현재 HP: {testPlayerData.hp:F1}");
//         }
//
//         [ContextMenu("체력 회복 테스트")]
//         public void TestHealthHeal()
//         {
//             testPlayerData.Heal(15f);
//             Debug.Log($"[UIEventTest] 체력 회복 테스트 - 현재 HP: {testPlayerData.hp:F1}");
//         }
//
//         [ContextMenu("배고픔 감소 테스트")]
//         public void TestHungerDecrease()
//         {
//             testPlayerData.Starve(20f);
//             Debug.Log($"[UIEventTest] 배고픔 감소 테스트 - 현재 Hunger: {testPlayerData.hunger:F1}");
//         }
//
//         [ContextMenu("배고픔 증가 테스트")]
//         public void TestHungerIncrease()
//         {
//             testPlayerData.Eat(25f);
//             Debug.Log($"[UIEventTest] 배고픔 증가 테스트 - 현재 Hunger: {testPlayerData.hunger:F1}");
//         }
//
//         [ContextMenu("수분 감소 테스트")]
//         public void TestThirstDecrease()
//         {
//             testPlayerData.Dehydrate(15f);
//             Debug.Log($"[UIEventTest] 수분 감소 테스트 - 현재 Thirst: {testPlayerData.thirst:F1}");
//         }
//
//         [ContextMenu("수분 증가 테스트")]
//         public void TestThirstIncrease()
//         {
//             testPlayerData.Drink(20f);
//             Debug.Log($"[UIEventTest] 수분 증가 테스트 - 현재 Thirst: {testPlayerData.thirst:F1}");
//         }
//
//         [ContextMenu("게임 시간 진행 테스트")]
//         public void TestGameTimeProgress()
//         {
//             testGameData.gameTime += 2f;
//             Debug.Log($"[UIEventTest] 게임 시간 진행 테스트 - 현재 시간: {testGameData.gameTime:F1}");
//         }
//
//         [ContextMenu("계절 변경 테스트")]
//         public void TestSeasonChange()
//         {
//             int currentSeason = (int)testGameData.currentSeason;
//             testGameData.currentSeason = (Season)((currentSeason + 1) % 4);
//             Debug.Log($"[UIEventTest] 계절 변경 테스트 - 현재 계절: {testGameData.currentSeason}");
//         }
//
//         [ContextMenu("날씨 변경 테스트")]
//         public void TestWeatherChange()
//         {
//             int currentWeather = (int)testGameData.currentWeather;
//             testGameData.currentWeather = (WeatherType)((currentWeather + 1) % 6);
//             Debug.Log($"[UIEventTest] 날씨 변경 테스트 - 현재 날씨: {testGameData.currentWeather}");
//         }
//
//         [ContextMenu("UI 새로고침 테스트")]
//         public void TestUIRefresh()
//         {
//             UIEventDispatcher.DispatchUIRefresh("All");
//             Debug.Log("[UIEventTest] UI 새로고침 테스트 실행");
//         }
//
//         [ContextMenu("배고픔 자동 감소 테스트")]
//         public void TestHungerAutoDecrease()
//         {
//             // 배고픔 자동 감소 이벤트 시뮬레이션
//             var hungerEvent = new _Project.Script.EventStruct.NetworkEvents.OnHungerChangeEvent
//             {
//                 hungerLossPerMinute = 5f, // 분당 5 감소
//                 reason = "시간 경과"
//             };
//
//             if (_Project.Script.Manager.EventHub.Instance != null)
//             {
//                 _Project.Script.Manager.EventHub.Instance.RaiseEvent(hungerEvent);
//             }
//
//             Debug.Log("[UIEventTest] 배고픔 자동 감소 테스트 실행 - 분당 5 감소");
//         }
//
//         [ContextMenu("수분 자동 감소 테스트")]
//         public void TestThirstAutoDecrease()
//         {
//             // 수분 자동 감소 이벤트 시뮬레이션
//             var thirstEvent = new _Project.Script.EventStruct.NetworkEvents.OnThirstChangeEvent
//             {
//                 thirstLossPerMinute = 3f, // 분당 3 감소
//                 reason = "시간 경과"
//             };
//
//             if (_Project.Script.Manager.EventHub.Instance != null)
//             {
//                 _Project.Script.Manager.EventHub.Instance.RaiseEvent(thirstEvent);
//             }
//
//             Debug.Log("[UIEventTest] 수분 자동 감소 테스트 실행 - 분당 3 감소");
//         }
//
//         [ContextMenu("플레이어 상태 전체 업데이트 테스트")]
//         public void TestPlayerStatusUpdate()
//         {
//             UIEventDispatcher.DispatchPlayerStatusUpdate(
//                 testPlayerData.hp,
//                 80f, // sanity
//                 testPlayerData.hunger,
//                 testPlayerData.thirst,
//                 10f, // cold
//                 Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber
//             );
//             Debug.Log("[UIEventTest] 플레이어 상태 전체 업데이트 테스트 실행");
//         }
//
//         [ContextMenu("게임 정보 전체 업데이트 테스트")]
//         public void TestGameInfoUpdate()
//         {
//             testGameData.DispatchGameInfoUpdate();
//             Debug.Log("[UIEventTest] 게임 정보 전체 업데이트 테스트 실행");
//         }
//
//         /// <summary>
//         /// 키보드 입력 테스트
//         /// </summary>
//         private void OnGUI()
//         {
//             if (!enableAutoTest)
//             {
//                 // 화면 중앙 위에 버튼 배치
//                 float panelWidth = 300f;
//                 float panelHeight = 500f;
//                 float x = (Screen.width - panelWidth) / 2f;
//                 float y = (Screen.height - panelHeight) / 3f; // 중앙 위쪽으로 이동
//
//                 GUILayout.BeginArea(new Rect(x, y, panelWidth, panelHeight));
//
//                 // 배경 박스
//                 GUI.Box(new Rect(0, 0, panelWidth, panelHeight), "");
//
//                 GUILayout.Space(10);
//                 GUILayout.Label("UI 이벤트 테스트", GUI.skin.box);
//                 GUILayout.Space(10);
//
//                 if (GUILayout.Button("체력 감소 (-10)"))
//                     TestHealthDecrease();
//
//                 if (GUILayout.Button("체력 회복 (+15)"))
//                     TestHealthHeal();
//
//                 if (GUILayout.Button("배고픔 감소 (-20)"))
//                     TestHungerDecrease();
//
//                 if (GUILayout.Button("배고픔 증가 (+25)"))
//                     TestHungerIncrease();
//
//                 if (GUILayout.Button("수분 감소 (-15)"))
//                     TestThirstDecrease();
//
//                 if (GUILayout.Button("수분 증가 (+20)"))
//                     TestThirstIncrease();
//
//                 if (GUILayout.Button("게임 시간 진행 (+2)"))
//                     TestGameTimeProgress();
//
//                 if (GUILayout.Button("계절 변경"))
//                     TestSeasonChange();
//
//                 if (GUILayout.Button("날씨 변경"))
//                     TestWeatherChange();
//
//                 if (GUILayout.Button("UI 새로고침"))
//                     TestUIRefresh();
//
//                 if (GUILayout.Button("배고픔 자동 감소"))
//                     TestHungerAutoDecrease();
//
//                 if (GUILayout.Button("수분 자동 감소"))
//                     TestThirstAutoDecrease();
//
//                 if (GUILayout.Button("플레이어 상태 전체 업데이트"))
//                     TestPlayerStatusUpdate();
//
//                 if (GUILayout.Button("게임 정보 전체 업데이트"))
//                     TestGameInfoUpdate();
//
//                 GUILayout.Space(10);
//
//                 // 현재 상태 표시
//                 GUILayout.Label($"현재 HP: {testPlayerData.hp:F1}", GUI.skin.box);
//                 GUILayout.Label($"현재 Hunger: {testPlayerData.hunger:F1}", GUI.skin.box);
//                 GUILayout.Label($"현재 Thirst: {testPlayerData.thirst:F1}", GUI.skin.box);
//                 GUILayout.Label($"현재 시간: {testGameData.gameTime:F1}", GUI.skin.box);
//                 GUILayout.Label($"현재 계절: {testGameData.currentSeason}", GUI.skin.box);
//
//                 GUILayout.EndArea();
//             }
//         }
//     }
// }
