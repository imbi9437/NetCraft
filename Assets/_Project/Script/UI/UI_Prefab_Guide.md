# UI 프리팹 생성 가이드

## 🎯 **원형 게이지 UI 프리팹 생성 방법**

### **1. CircularGauge 프리팹 생성**

#### **기본 구조:**
```
CircularGauge (GameObject)
├── Background (Image) - 원형 배경
├── Gauge (Image) - 원형 게이지 (CircularGauge 컴포넌트)
│   ├── ValueText (TextMeshPro) - 수치 텍스트
│   └── LabelText (TextMeshPro) - 라벨 텍스트
```

#### **설정 방법:**

1. **Background Image 설정:**
   - Image Type: Simple
   - Color: 어두운 회색 (배경)
   - 원형 스프라이트 사용

2. **Gauge Image 설정:**
   - Image Type: Filled
   - Fill Method: Radial 360
   - Fill Origin: 2 (위에서 시작)
   - Fill Clockwise: false (시계 반대 방향)
   - Color: 게이지 색상

3. **CircularGauge 컴포넌트 설정:**
   - Gauge Image: Gauge Image 할당
   - Value Text: ValueText 할당
   - Label Text: LabelText 할당

### **2. PlayerStatusPanel 프리팹 생성**

#### **기본 구조:**
```
PlayerStatusPanel (GameObject)
├── HealthGauge (CircularGauge)
├── SanityGauge (CircularGauge)
├── HungerGauge (CircularGauge)
├── ThirstGauge (CircularGauge)
└── ColdGauge (CircularGauge)
```

#### **각 게이지 설정:**

1. **체력 게이지 (HealthGauge):**
   - 색상: 빨간색 계열
   - 라벨: "체력"
   - 위험 수준: 20% 이하

2. **정신력 게이지 (SanityGauge):**
   - 색상: 파란색 계열
   - 라벨: "정신력"
   - 위험 수준: 20% 이하

3. **배고픔 게이지 (HungerGauge):**
   - 색상: 노란색 계열
   - 라벨: "배고픔"
   - 위험 수준: 20% 이하

4. **수분 게이지 (ThirstGauge):**
   - 색상: 청록색 계열
   - 라벨: "수분"
   - 위험 수준: 20% 이하

5. **추위 게이지 (ColdGauge):**
   - 색상: 흰색 계열
   - 라벨: "추위"
   - 위험 수준: 80% 이상

### **3. GameInfoPanel 프리팹 생성**

#### **기본 구조:**
```
GameInfoPanel (GameObject)
├── TimeInfo (GameObject)
│   ├── TimeText (TextMeshPro) - "시간: 12:34"
│   ├── DateText (TextMeshPro) - "봄 15일"
│   └── DayText (TextMeshPro) - "Day 15"
├── SeasonInfo (GameObject)
│   ├── SeasonText (TextMeshPro) - "계절: 봄"
│   └── SeasonIcon (Image) - 계절 아이콘
├── WeatherInfo (GameObject)
│   ├── WeatherText (TextMeshPro) - "날씨: 맑음"
│   └── WeatherIcon (Image) - 날씨 아이콘
└── TemperatureInfo (GameObject)
    ├── TemperatureText (TextMeshPro) - "온도: 20.5°C"
    └── TemperatureSlider (Slider) - 온도 슬라이더
```

### **4. UIManager 프리팹 생성**

#### **기본 구조:**
```
UIManager (GameObject)
├── PlayerStatusPanel (PlayerStatusPanel)
├── GameInfoPanel (GameInfoPanel)
├── MainMenuPanel (GameObject)
├── LobbyPanel (GameObject)
├── RoomPanel (GameObject)
├── CreatePanel (GameObject)
└── PasswordInputPanel (GameObject)
```

## 🎨 **UI 디자인 팁**

### **원형 게이지 디자인:**
1. **크기:** 100x100 픽셀 권장
2. **색상:** 각 상태별로 구분되는 색상 사용
3. **애니메이션:** 부드러운 전환 효과 적용
4. **텍스트:** 명확하고 읽기 쉬운 폰트 사용

### **레이아웃:**
1. **플레이어 상태:** 화면 좌상단에 배치
2. **게임 정보:** 화면 우상단에 배치
3. **반응형:** 다양한 해상도에 대응

### **색상 팔레트:**
- **체력:** 빨간색 (#FF4444)
- **정신력:** 파란색 (#4444FF)
- **배고픔:** 노란색 (#FFFF44)
- **수분:** 청록색 (#44FFFF)
- **추위:** 흰색 (#FFFFFF)

## 🔧 **사용 방법**

### **코드에서 사용:**
```csharp
// 플레이어 상태 설정
playerStatusPanel.SetPlayerStatus(80f, 60f, 40f, 30f, 20f);

// 게임 정보 설정
gameInfoPanel.SetGameInfo(12.5f, 15, 0, 0, 25.0f);

// UI 토글
uiManager.TogglePlayerStatus();
uiManager.ToggleGameInfo();
```

### **이벤트 기반 업데이트:**
- **PlayerStatusPanel:** OnPlayerPropertiesUpdate 이벤트로 자동 업데이트
- **GameInfoPanel:** OnRoomPropertiesUpdate 이벤트로 자동 업데이트

## 📱 **모바일 최적화**

1. **터치 친화적:** 충분한 크기의 UI 요소
2. **성능 최적화:** 불필요한 애니메이션 최소화
3. **배터리 절약:** UI 업데이트 빈도 조절

## 🎮 **게임플레이 통합**

1. **실시간 동기화:** 네트워크 이벤트와 연동
2. **상태 관리:** 플레이어 데이터와 연동
3. **시각적 피드백:** 상태 변화 시 즉시 반영
