using _Project.Script.Interface;
using _Project.Script.Character.Network;
using _Project.Script.World;
using _Project.Script.Items;
using _Project.Script.Character.Player;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;


namespace _Project.Script.EventStruct
{
    /// <summary>
    /// 네트워크 관련 이벤트들을 정의하는 클래스
    /// </summary>
    public static class PunEvents
    {
        #region Photon Callbacks Events
        
        
        public struct OnConnectEvent : IEvent { }
        public struct OnDisconnectEvent : IEvent
        {
            public DisconnectCause cause;

            public OnDisconnectEvent(DisconnectCause cause)
            {
                this.cause = cause;
            }
        }
        
        
        public struct OnJoinedLobbyEvent : IEvent { }
        public struct OnLeftLobbyEvent : IEvent { }
        
        
        public struct OnCreateRoomEvent : IEvent { }
        public struct OnCreateRoomFailedEvent : IEvent
        {
            public int returnCode;
            public string message;
            
            public OnCreateRoomFailedEvent(int returnCode, string message)
            {
                this.returnCode = returnCode;
                this.message = message;
            }
        }
        
        
        public struct OnJoinedRoomEvent : IEvent { }
        public struct OnJoinedRoomFailedEvent : IEvent
        {
            public int returnCode;
            public string message;
            
            public OnJoinedRoomFailedEvent(int returnCode, string message)
            {
                this.returnCode = returnCode;
                this.message = message;
            }
        }
        public struct OnLeftRoomEvent : IEvent { }
        

        /// <summary> 플레이어 입장 이벤트 </summary>
        public struct OnPlayerJoinedEvent : IEvent
        {
            public Player player;

            public OnPlayerJoinedEvent(Player player)
            {
                this.player = player;
            }
        }

        /// <summary> 플레이어 퇴장 이벤트 </summary>
        public struct OnPlayerLeftEvent : IEvent
        {
            public Player player;

            public OnPlayerLeftEvent(Player player)
            {
                this.player = player;
            }
        }

        /// <summary> 방 목록 업데이트 이벤트 </summary>
        public struct OnRoomListUpdateEvent : IEvent
        {
            public List<RoomInfo> roomList;
            
            public OnRoomListUpdateEvent(List<RoomInfo> roomList)
            {
                this.roomList = roomList;
            }
        }

        /// <summary> 플레이어 속성 업데이트 이벤트 </summary>
        public struct OnPlayerPropertiesUpdateEvent : IEvent
        {
            public string playerName;
            public int actorNumber;
            public string propertyName;
            public object propertyValue;
        }
        
        #endregion

        #region Photon Request Events
        
        
        public struct CreateRoomRequestEvent : IEvent
        {
            public string roomName;
            public string password;
            public bool isPublic;
            public int maxPlayers;

            public CreateRoomRequestEvent(string roomName, string password, bool isPublic, int maxPlayers)
            {
                this.roomName = roomName;
                this.password = password;
                this.isPublic = isPublic;
                this.maxPlayers = maxPlayers;
            }
        }
        public struct JoinRoomRequestEvent : IEvent
        {
            public string roomName;
            public string password;

            public JoinRoomRequestEvent(string roomName, string password)
            {
                this.roomName = roomName;
                this.password = password;
            }
        }
        public struct LeaveRoomRequestEvent : IEvent { }


        public struct ChangeRoomOptionsRequestEvent : IEvent
        {
            public bool isVisible;
            public bool isOpen;
            public int maxPlayers;

            public ChangeRoomOptionsRequestEvent(bool isVisible, bool isOpen, int maxPlayers)
            {
                this.isVisible = isVisible;
                this.isOpen = isOpen;
                this.maxPlayers = maxPlayers;
            }

            public ChangeRoomOptionsRequestEvent(bool isVisible, bool isOpen)
            {
                this.isVisible = isVisible;
                this.isOpen = isOpen;
                this.maxPlayers = 0;
            }
        }
        
        
        #endregion

        #region Data Request Events

        public struct RequestServerConnectionEvent : IEvent {}

        #endregion


        #region Data Send Events

        public struct SendServerConnectionEvent : IEvent
        {
            public bool isConnected;
            public SendServerConnectionEvent(bool isConnected) => this.isConnected = isConnected;
        }

        #endregion
        
        
        /// <summary>
        /// 플레이어 레디 상태 변경 이벤트
        /// </summary>
        public struct OnPlayerReadyChangedEvent : IEvent
        {
            public string playerName;
            public int actorNumber;
            public bool isReady;
        }

        public struct OnHungerChangeEvent : IEvent
        {
            public float hungerLossPerMinute;  // 분당 배고픔 감소량
            public string reason;              // 감소 이유 (시간, 활동 등)
        }

        /// <summary>
        /// 수분 변화 이벤트 (돈스타브 실제)
        /// </summary>
        public struct OnThirstChangeEvent : IEvent
        {
            public float thirstLossPerMinute;  // 분당 수분 감소량
            public string reason;              // 감소 이유 (시간, 활동 등)
        }
        
        /// <summary>
        /// 게임 시작 요청 이벤트
        /// </summary>
        public struct OnGameStartRequestEvent : IEvent
        {
            public string sceneName;
        }

        /// <summary>
        /// UI 상태 표시 이벤트
        /// </summary>
        public struct OnUIStatusUpdateEvent : IEvent
        {
            public string message;
            public UnityEngine.Color color;
            public float displayTime; // 표시 시간 (0이면 자동 숨김 안함)
        }

        /// <summary>
        /// 방 참가 요청 이벤트
        /// </summary>
        public struct OnJoinRoomRequestEvent : IEvent
        {
            public string roomName;
        }

        /// <summary>
        /// 플레이어 퇴장 이벤트 (상세 정보 포함)
        /// </summary>
        public struct OnPlayerLeftRoomEvent : IEvent
        {
            public int actorNumber;
            public string playerName;
        }

        /// <summary>
        /// 월드 동기화 이벤트
        /// </summary>
        public struct OnWorldSyncEvent : IEvent
        {
            public int structureCount;
            public int resourceCount;
        }

        /// <summary>
        /// 구조물 건설 이벤트
        /// </summary>
        public struct OnStructureBuiltEvent : IEvent
        {
            public int structureId;
            public Vector3 position;
            public StructureType structureType;
            public int builderActorNumber;
        }

        /// <summary>
        /// 구조물 파괴 이벤트
        /// </summary>
        public struct OnStructureDestroyedEvent : IEvent
        {
            public int structureId;
            public Vector3 position;
            public int destroyerActorNumber;
        }

        /// <summary>
        /// 리소스 채집 이벤트
        /// </summary>
        public struct OnResourceHarvestedEvent : IEvent
        {
            public Vector3Int position;
            public ResourceType resourceType;
            public int amount;
            public int remainingAmount;
            public int harvesterActorNumber;
        }

        /// <summary>
        /// 리소스 재생성 이벤트
        /// </summary>
        public struct OnResourceRegeneratedEvent : IEvent
        {
            public Vector3Int position;
            public ResourceType resourceType;
            public int amount;
        }

        /// <summary>
        /// 아이템 공유 이벤트 (바닥에 버린 아이템)
        /// </summary>
        public struct OnItemSharedEvent : IEvent
        {
            public int fromActorNumber;
            public int toActorNumber;
            public int slotIndex;
            public _Project.Script.Items.ItemInstance item;
        }

        /// <summary>
        /// 플레이어 데미지 이벤트 (항상 켜져있는 PvP)
        /// </summary>
        public struct OnPlayerDamagedEvent : IEvent
        {
            public int attackerActorNumber;
            public int targetActorNumber;
            public float damage;
            public Vector3 damagePosition;
        }

        /// <summary>
        /// 계절 변화 이벤트
        /// </summary>
        public struct OnSeasonChangedEvent : IEvent
        {
            public Season newSeason;
            public Season previousSeason;
            public float transitionDuration;
        }

        /// <summary>
        /// 정신력 변화 이벤트 (돈스타브 실제)
        /// </summary>
        public struct OnSanityChangeEvent : IEvent
        {
            public float sanityLossPerMinute;  // 분당 정신력 감소량
            public string reason;               // 감소 이유 (시간대, 어둠 등)
        }

        /// <summary>
        /// 세그먼트 변화 이벤트 (돈스타브 실제)
        /// </summary>
        public struct OnSegmentChangedEvent : IEvent
        {
            public int currentSegment;      // 현재 세그먼트 (0-15)
            public int totalSegments;      // 총 세그먼트 수 (16)
            public float segmentProgress;  // 세그먼트 진행률 (0-1)
        }

        /// <summary>
        /// 계절 시각 효과 이벤트 (돈스타브 실제)
        /// </summary>
        public struct OnSeasonVisualEffectEvent : IEvent
        {
            public Color seasonColor;      // 계절별 색상
            public string seasonName;      // 계절 이름
            public float transitionDuration; // 전환 시간
        }

        /// <summary>
        /// 체온 변화 이벤트 (돈스타브 실제)
        /// </summary>
        public struct OnTemperatureChangeEvent : IEvent
        {
            public float temperatureChange;  // 체온 변화량
            public string reason;           // 변화 이유 (계절, 시간대, 장비 등)
        }

        /// <summary>
        /// 젖음 상태 변화 이벤트 (돈스타브 실제)
        /// </summary>
        public struct OnWetnessChangeEvent : IEvent
        {
            public float wetnessChange;     // 젖음 변화량
            public string reason;           // 변화 이유 (비, 눈 등)
        }

        /// <summary>
        /// 날씨 변화 이벤트
        /// </summary>
        public struct OnWeatherChangedEvent : IEvent
        {
            public WeatherType newWeather;
            public WeatherType previousWeather;
            public float intensity;
        }

        /// <summary>
        /// 아이템 사용 이벤트
        /// </summary>
        public struct OnItemUsedEvent : IEvent
        {
            public int actorNumber;
            public int itemUID;
            public int slotIndex;
            public Vector3 usePosition;
        }
    }

    
    /// <summary>
    /// 시간대 열거형 (돈스타브 실제 시간 기준)
    /// 돈스타브는 3단계: 낮 → 저녁 → 밤
    /// </summary>
    public enum TimeOfDay
    {
        Day,       // 낮 (주행성 몹 활동, 안전한 시간)
        Evening,   // 저녁 (정신력 -5/분, 야행성 몹 활동 시작)
        Night      // 밤 (정신력 -5~50/분, 어둠 페널티, 야행성 몹 활동)
    }

    /// <summary>
    /// 계절 열거형
    /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정
    /// </summary>
    public enum Season
    {
        Spring,    // 봄
        Summer,    // 여름
        Autumn,    // 가을
        Winter     // 겨울
    }

    /// <summary>
    /// 날씨 타입 열거형
    /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정
    /// </summary>
    public enum WeatherType
    {
        Clear,     // 맑음
        Cloudy,    // 흐림
        Rain,      // 비
        Storm,     // 폭풍
        Snow,      // 눈
        Fog        // 안개
    }

    /// <summary>
    /// 게임 이벤트 타입 열거형 (단순화)
    /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정
    /// </summary>
    public enum GameEventType
    {
        None, // 없음
        SeasonChange, // 계절 변화
        WeatherChange, // 날씨 변화
        TimeChange, // 시간 변화
        PlayerEvent, // 플레이어 이벤트
        SystemEvent, // 시스템 이벤트
        EnvironmentalEvent, // 환경 이벤트
    }
}
