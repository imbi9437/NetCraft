# 🌐 **네트워크 플로우 간단 가이드**

> **돈스타브 멀티플레이어 게임의 핵심 네트워크 흐름을 간단히 정리**

---

## 🚀 **게임 시작 플로우**

### **1단계: 네트워크 연결**
```csharp
NetworkManager.Start() 
→ ConnectToMaster() 
→ OnConnectedToMaster() 
→ PhotonNetwork.JoinLobby()
```

### **2단계: 방 생성/입장**
```csharp
RoomManager.CreateRoom() 
→ OnCreatedRoom() 
→ MultiplayerGameController.StartGame()
```

### **3단계: 신 전환 및 초기화**
```csharp
PhotonNetwork.LoadLevel("GameScene")
→ 새 씬 로드 완료
→ DefaultExecutionOrder에 따른 Awake() 호출
→ EventHub(-100) → SceneController(-99) → NetworkManager(-90) → DataManager(-90)
→ MainMenuPanel(-80) → NetworkItemManager(-80) → NetworkWorldManager(-60)
→ InputManager(-50) → GameManager(-50) → NetworkPlayerInteraction(-50)
→ NetworkPvESystem(-45) → NetworkGameEventManager(-40)
```

### **4단계: 플레이어 스폰**
```csharp
PlayerSpawner.SpawnLocalPlayer() 
→ PhotonNetwork.Instantiate() 
→ PlayerStateMachine.Awake() → InitializeComponents()
→ PlayerNetworkHandler.Start() → SubscribeToGameEvents()
→ SetPlayerBasicInfo() → Invoke(InitializePlayerStats, 0.2f)
→ Invoke(SendInitialStatusToUI, 0.5f)
```

---

## 🎮 **플레이어 시스템**

### **입력 → 이동 → 네트워크 동기화**
```csharp
PlayerInputHandler.HandleMovementInput()
→ OnMoveInput 이벤트
→ PlayerStateMachine.HandleMoveInput()
→ PlayerNetworkHandler.OnPhotonSerializeView()
→ 다른 클라이언트에게 동기화
```

### **플레이어 스탯 동기화**
```csharp
PlayerStateMachine 스탯 변경
→ PlayerNetworkHandler.UpdatePlayerProperties()
→ PhotonNetwork.LocalPlayer.SetCustomProperties()
→ 다른 클레이어 UI 업데이트
```

---

## 🏗️ **월드 시스템**

### **구조물 건설**
```csharp
플레이어가 건설 버튼 클릭
→ NetworkWorldManager.BuildStructure()
→ BuildStructureRPC() 전송
→ 모든 클라이언트에서 구조물 생성
```

### **리소스 채집**
```csharp
플레이어가 리소스와 상호작용
→ NetworkResource.HarvestResource()
→ HarvestResourceRPC() 전송
→ 리소스 양 감소 및 UI 업데이트
```

---

## 🌍 **환경 시스템**

### **날씨/계절 동기화**
```csharp
NetworkGameEventManager.Update()
→ 시간/계절/날씨 계산 (MasterClient만)
→ PhotonNetwork.RaiseEvent() 전송
→ 모든 클라이언트에서 환경 효과 적용
```

---

## 📡 **네트워크 동기화 방식**

### **실시간 데이터 (위치, 회전)**
- **방식**: `IPunObservable` → `OnPhotonSerializeView()`
- **사용**: 플레이어 이동, 몬스터 AI
- **주기**: 20-60Hz (핑에 따라 조정)

### **상태 데이터 (체력, 스탯)**
- **방식**: `CustomProperties`
- **사용**: 플레이어 스탯, 구조물 상태
- **주기**: 변경 시에만

### **이벤트 데이터 (공격, 채집)**
- **방식**: `RPC` 또는 `RaiseEvent`
- **사용**: 일회성 액션, 환경 변화
- **주기**: 필요할 때

---

## 🔧 **핵심 컴포넌트**

### **매니저들**
- `NetworkManager`: PUN2 연결 관리
- `RoomManager`: 방 생성/입장 관리
- `NetworkWorldManager`: 월드 상태 동기화
- `NetworkGameEventManager`: 환경 시스템 관리

### **플레이어 컴포넌트**
- `PlayerStateMachine`: 상태 관리
- `PlayerNetworkHandler`: 네트워크 동기화
- `PlayerInputHandler`: 입력 처리
- `PlayerCameraController`: 카메라 제어

### **월드 컴포넌트**
- `NetworkResource`: 리소스 동기화
- `StructureManager`: 구조물 관리
- `WorldDataManager`: 월드 데이터 관리

---

## ⚡ **성능 최적화**

### **네트워크 최적화**
```csharp
// 핑에 따른 전송률 조정
if (ping <= 50) PhotonNetwork.SerializationRate = 40;
else if (ping <= 100) PhotonNetwork.SerializationRate = 30;
else PhotonNetwork.SerializationRate = 20;
```

### **보간 설정**
```csharp
// 원격 플레이어 부드러운 움직임
ApplyInterpolation() // SmoothDamp 사용
```

---

## 🎯 **개발 시 체크리스트**

### **새 기능 추가할 때**
1. **네트워크 동기화 필요한가?** → `IPunObservable` 구현
2. **이벤트 통신 필요한가?** → `EventHub` 사용
3. **UI 업데이트 필요한가?** → `UIEvents` 추가

### **디버깅할 때**
```csharp
Debug.Log($"[NetworkManager] 핑: {PhotonNetwork.GetPing()}ms");
Debug.Log($"[PlayerNetworkHandler] 동기화 상태: {hasReceivedData}");
```

---

## 📚 **관련 문서**
- `Function_Call_Flow_Reference.md`: 상세한 함수 호출 흐름
- `README_Network_System.md`: 네트워크 시스템 전체 설명

---

🎯 현재 상황 분석
✅ 구현된 것들
네트워크 연결/방 관리
플레이어 스폰/동기화
월드 데이터 구조
이벤트 시스템

❌ 구현이 필요한 것들
리소스 채집 시스템 (나무 베기, 돌 채굴 등)
날씨/계절 실제 변화 (비, 눈, 태풍 등)
몬스터 전투 시스템 (공격, 데미지, 사망)
인벤토리 시스템
제작 시스템
구조물 건설/파괴

**💡 이 가이드만 보고도 기본적인 네트워크 플로우를 이해하고 개발할 수 있습니다!**
