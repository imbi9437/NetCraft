# 🎯 네트워크 게임 시작 플로우 (간단 버전)

## 📋 협업자를 위한 핵심 순서

---

## 🎬 **1단계: 게임 시작**
```
NetworkManager → PhotonNetwork.ConnectUsingSettings() → OnConnectedToMaster()
```

## 🏠 **2단계: 방 생성/참가**
```
UI 버튼 클릭 → RoomManager.CreateRoom() → PhotonNetwork.CreateRoom() → OnCreatedRoom()
```

## 🎮 **3단계: 게임 시작**
```
Ready 버튼 → 모든 플레이어 Ready → SceneTransition → GameScene 로드
```

## 🔄 **신 전환 시 초기화 순서**
```
PhotonNetwork.LoadLevel("GameScene") 호출
    ↓
새 씬 로드 완료
    ↓
[DefaultExecutionOrder(-100)] EventHub.Awake()
    ↓
[DefaultExecutionOrder(-99)] SceneController.Awake()
    ↓
[DefaultExecutionOrder(-90)] NetworkManager.Awake() + DataManager.Awake()
    ↓
[DefaultExecutionOrder(-80)] MainMenuPanel.Awake() + NetworkItemManager.Awake()
    ↓
[DefaultExecutionOrder(-60)] NetworkWorldManager.Awake()
    ↓
[DefaultExecutionOrder(-50)] InputManager.Awake() + GameManager.Awake() + NetworkPlayerInteraction.Awake()
    ↓
[DefaultExecutionOrder(-45)] NetworkPvESystem.Awake()
    ↓
[DefaultExecutionOrder(-40)] NetworkGameEventManager.Awake()
    ↓
[DefaultExecutionOrder(-100)] PlayerStateMachine.Awake() (플레이어 스폰 시)
    ↓
[DefaultExecutionOrder(100)] PlayerCameraController.Awake() (플레이어 스폰 시)
```

## 👤 **4단계: 플레이어 스폰**
```
MultiplayerGameController.StartGame() → PlayerSpawner.SpawnLocalPlayer() → PhotonNetwork.Instantiate()
    ↓
PlayerStateMachine.Awake() → InitializeComponents() → SetupGameEventSubscriptions()
    ↓
PlayerNetworkHandler.Start() → SubscribeToGameEvents() → SetPlayerBasicInfo()
    ↓
Invoke(InitializePlayerStats, 0.2f) → Invoke(SendInitialStatusToUI, 0.5f)
```

## 📡 **5단계: 플레이어 동기화**
```
PlayerStateMachine (게임 로직) → PlayerNetworkHandler (네트워크 동기화)
```

## 🌍 **6단계: 환경 시스템**
```
마스터 클라이언트 → 시간/날씨/계절 변경 → 모든 클라이언트에 전송
```

---

## 🔄 **핵심 데이터 플로우**

### **실시간 동기화 (60fps)**
```
로컬 플레이어 → OnPhotonSerializeView → 원격 플레이어들
```

### **상태 동기화 (이벤트 기반)**
```
스탯 변경 → UpdatePlayerProperties → CustomProperties → UI 업데이트
```

### **환경 동기화 (주기적)**
```
마스터 클라이언트 → PhotonEvent → 모든 클라이언트
```
---


## 🎯 **새 기능 추가 가이드**

### **네트워크 동기화 필요한가?**
```
네트워크 동기화 → IPunObservable 구현 → OnPhotonSerializeView() 추가
```

### **이벤트 통신 필요한가?**
```
이벤트 통신 → EventStruct 정의 → EventHub.RegisterEvent() → EventHub.RaiseEvent()
```

### **UI 업데이트 필요한가?**
```
UI 업데이트 → UIEvents 추가 → UI 컴포넌트에서 이벤트 구독
```

## 💡 **핵심 예시**

### **플레이어 스폰 과정**
```
MultiplayerGameController.StartGame() → PlayerSpawner.SpawnLocalPlayer() → PhotonNetwork.Instantiate()
```
### **플레이어 이동 동기화**
```
PlayerInputHandler → PlayerStateMachine → PlayerNetworkHandler → OnPhotonSerializeView()
```

### **스탯 변경 동기화**
```
PlayerStateMachine (게임 로직) → networkHandler.SyncStatsToNetwork() → UpdatePlayerProperties()
```

---

## 🎯 **핵심 요약**

```
🎬 게임 시작: NetworkManager → 방 생성/참가 → Ready → GameScene
👤 플레이어: 스폰 → 초기화 → 입력 활성화
📡 동기화: 실시간(60fps) + 상태(이벤트) + 환경(주기적)
🔄 이벤트: 구독 → 발송 → 해제 패턴
```

**협업자들이 빠르게 이해할 수 있도록 간단하게 정리했습니다!** 🎯
