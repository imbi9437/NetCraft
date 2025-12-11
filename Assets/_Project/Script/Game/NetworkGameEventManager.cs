using UnityEngine;
using Photon.Pun;
using _Project.Script.EventStruct;
using _Project.Script.Generic;
using _Project.Script.Data;
using _Project.Script.Interface;

namespace _Project.Script.Game
{
    /// <summary>
    /// 리팩토링된 게임 이벤트 관리자
    /// 각 시스템을 조합하여 전체 게임 이벤트 관리
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    [DefaultExecutionOrder(-40)]
    public class NetworkGameEventManager : MonoSingleton<NetworkGameEventManager>, IPunObservable
    {
        [Header("시스템 컴포넌트")]
        [SerializeField] private TimeSystem timeSystem;
        [SerializeField] private SeasonSystem seasonSystem;
        [SerializeField] private WeatherSystem weatherSystem;
        [SerializeField] private EnvironmentalSystem environmentalSystem;

        [Header("네트워크 설정")]
        [SerializeField] private bool enableTimeSync = true;
        [SerializeField] private float syncInterval = 1f;
        [SerializeField] private bool enablePlayerCountOptimization = true; // 플레이어 수 기반 최적화

        [Header("게임 데이터")]
        [SerializeField] private GameTimeData currentGameData;
        [SerializeField] private WeatherData currentWeatherData;
        [SerializeField] private ActiveGameEvent currentEvent;

        // 네트워크 동기화
        private float lastSyncTime = 0f;
        private PhotonView photonView;
        private bool isInitialized = false;

        protected override void Awake()
        {
            base.Awake();
            photonView = GetComponent<PhotonView>();
            InitializeSystems();
            isInitialized = true;
        }

        private void Update()
        {
            if (!enableTimeSync) return;

            // 플레이어가 있을 때만 시스템 업데이트
            if (PhotonNetwork.PlayerList.Length > 0)
            {
                // 각 시스템 업데이트
                timeSystem.UpdateTime();
                seasonSystem.CheckSeasonChange(timeSystem.DayNumber);
                weatherSystem.UpdateWeather(seasonSystem.CurrentSeason);

                // 환경 효과 적용
                ApplyEnvironmentalEffects();

                // 플레이어 수 기반 동기화 간격 조정
                float adjustedSyncInterval = GetAdjustedSyncInterval();

                // 네트워크 동기화 (최적화)
                if (Time.time - lastSyncTime >= adjustedSyncInterval)
                {
                    SyncGameDataOptimized();
                    lastSyncTime = Time.time;
                }
            }
        }

        /// <summary>
        /// 시스템 초기화
        /// </summary>
        private void InitializeSystems()
        {
            if (timeSystem == null) timeSystem = new TimeSystem();
            if (seasonSystem == null) seasonSystem = new SeasonSystem();
            if (weatherSystem == null) weatherSystem = new WeatherSystem();
            if (environmentalSystem == null) environmentalSystem = new EnvironmentalSystem();

            // 게임 데이터 초기화
            if (currentGameData == null) currentGameData = new GameTimeData();
            if (currentWeatherData == null) currentWeatherData = new WeatherData();
            if (currentEvent == null) currentEvent = new ActiveGameEvent();
        }

        /// <summary>
        /// 환경 효과 적용
        /// </summary>
        private void ApplyEnvironmentalEffects()
        {
            TimeOfDay currentTimeOfDay = timeSystem.GetTimeOfDay(seasonSystem.GetCurrentSeasonTimeRatio());

            // 정신력 효과
            environmentalSystem.ApplySanityEffects(currentTimeOfDay);

            // 체온 효과
            environmentalSystem.ApplyTemperatureEffects(seasonSystem.CurrentSeason, currentTimeOfDay);

            // 젖음 효과
            environmentalSystem.ApplyWetnessEffects(seasonSystem.CurrentSeason, weatherSystem.CurrentWeather);
        }

        /// <summary>
        /// 게임 데이터 네트워크 동기화 (최적화)
        /// </summary>
        private void SyncGameDataOptimized()
        {
            // 마스터 클라이언트만 데이터 전송
            if (PhotonNetwork.IsMasterClient)
            {
                // 현재 게임 데이터 업데이트
                UpdateGameData();

                // 이벤트 발생 확인
                CheckForEvents();
            }
        }

        /// <summary>
        /// 게임 데이터 네트워크 동기화 (기존)
        /// </summary>
        private void SyncGameData()
        {
            SyncGameDataOptimized();
        }

        /// <summary>
        /// 게임 데이터 업데이트
        /// </summary>
        private void UpdateGameData()
        {
            currentGameData.gameTime = timeSystem.GameTime;
            currentGameData.dayNumber = timeSystem.DayNumber;
            currentGameData.segment = timeSystem.CurrentSegment;
            currentGameData.timeOfDay = timeSystem.GetTimeOfDay(seasonSystem.GetCurrentSeasonTimeRatio());
            currentGameData.currentSeason = seasonSystem.CurrentSeason;
            currentGameData.seasonDay = seasonSystem.SeasonDay;
            currentGameData.currentWeather = weatherSystem.CurrentWeather;
            currentGameData.weatherIntensity = weatherSystem.WeatherIntensity;

            currentWeatherData.UpdateWeather(weatherSystem.CurrentWeather, weatherSystem.WeatherIntensity, weatherSystem.WeatherDuration);
        }

        /// <summary>
        /// 이벤트 발생 확인
        /// </summary>
        private void CheckForEvents()
        {
            // 계절 변경 이벤트
            if (seasonSystem.HasSeasonChanged)
            {
                CreateSeasonChangeEvent();
            }

            // 날씨 변경 이벤트
            if (weatherSystem.HasWeatherChanged)
            {
                CreateWeatherChangeEvent();
            }
        }

        /// <summary>
        /// 계절 변경 이벤트 생성
        /// </summary>
        private void CreateSeasonChangeEvent()
        {
            currentEvent = new ActiveGameEvent(
                $"SeasonChange_{seasonSystem.CurrentSeason}",
                GameEventType.SeasonChange,
                $"{seasonSystem.CurrentSeason} 시작",
                $"{seasonSystem.CurrentSeason} 계절이 시작되었습니다.",
                10f, // 10초 지속
                1f   // 강도 1
            );
            currentEvent.StartEvent();
        }

        /// <summary>
        /// 날씨 변경 이벤트 생성
        /// </summary>
        private void CreateWeatherChangeEvent()
        {
            currentEvent = new ActiveGameEvent(
                $"WeatherChange_{weatherSystem.CurrentWeather}",
                GameEventType.WeatherChange,
                $"{weatherSystem.CurrentWeather} 날씨",
                $"{weatherSystem.CurrentWeather} 날씨가 시작되었습니다.",
                weatherSystem.WeatherDuration,
                weatherSystem.WeatherIntensity
            );
            currentEvent.StartEvent();
        }

        #region IPunObservable 구현

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (!isInitialized) return; // 초기화 전에는 동기화하지 않음

            if (stream.IsWriting)
            {
                // 압축된 데이터 전송 (성능 최적화)
                SendCompressedData(stream);
            }
            else
            {
                // 압축된 데이터 수신 (성능 최적화)
                ReceiveCompressedData(stream);
            }
        }

        /// <summary>
        /// 수신된 데이터 적용
        /// </summary>
        private void ApplyReceivedData(float gameTime, int dayNumber, int segment, TimeOfDay timeOfDay, Season season, int seasonDay, WeatherType weather, float weatherIntensity,
                                     float weatherIntensity2, float weatherDuration, float weatherRemaining, bool isRaining, bool isSnowing,
                                     bool eventActive, string eventId, GameEventType eventType, string eventName, float eventRemaining)
        {
            // 게임 데이터 동기화
            currentGameData.gameTime = gameTime;
            currentGameData.dayNumber = dayNumber;
            currentGameData.segment = segment;
            currentGameData.timeOfDay = timeOfDay;
            currentGameData.currentSeason = season;
            currentGameData.seasonDay = seasonDay;
            currentGameData.currentWeather = weather;
            currentGameData.weatherIntensity = weatherIntensity;

            // 날씨 데이터 동기화
            currentWeatherData.currentWeather = weather;
            currentWeatherData.intensity = weatherIntensity2;
            currentWeatherData.duration = weatherDuration;
            currentWeatherData.remainingTime = weatherRemaining;
            currentWeatherData.isRaining = isRaining;
            currentWeatherData.isSnowing = isSnowing;

            // 이벤트 데이터 동기화
            currentEvent.isActive = eventActive;
            currentEvent.eventId = eventId;
            currentEvent.eventType = eventType;
            currentEvent.eventName = eventName;
            currentEvent.remainingTime = eventRemaining;

            // 시스템에 데이터 적용
            ApplyDataToSystems();
        }

        /// <summary>
        /// 시스템에 데이터 적용
        /// </summary>
        private void ApplyDataToSystems()
        {
            // 시간 시스템 동기화
            timeSystem.SetGameTime(currentGameData.gameTime);
            timeSystem.SetDayNumber(currentGameData.dayNumber);
            timeSystem.SetCurrentSegment(currentGameData.segment);

            // 계절 시스템 동기화
            seasonSystem.SetCurrentSeason(currentGameData.currentSeason);
            seasonSystem.SetSeasonDay(currentGameData.seasonDay);

            // 날씨 시스템 동기화
            weatherSystem.SetCurrentWeather(currentGameData.currentWeather);
            weatherSystem.SetWeatherIntensity(currentGameData.weatherIntensity);
        }

        #endregion

        #region 네트워크 성능 최적화

        /// <summary>
        /// 플레이어 수 기반 동기화 간격 조정
        /// </summary>
        private float GetAdjustedSyncInterval()
        {
            if (!enablePlayerCountOptimization)
                return syncInterval;

            int playerCount = PhotonNetwork.PlayerList.Length;

            // 플레이어 수에 따른 동기화 간격 조정
            if (playerCount <= 2)
                return syncInterval; // 기본 간격
            else if (playerCount <= 4)
                return syncInterval * 1.5f; // 1.5배 느리게
            else if (playerCount <= 8)
                return syncInterval * 2f; // 2배 느리게
            else
                return syncInterval * 3f; // 3배 느리게 (대규모 서버)
        }

        /// <summary>
        /// 압축된 데이터 전송 (필수 게임 데이터)
        /// </summary>
        private void SendCompressedData(PhotonStream stream)
        {
            // 시간 데이터 전송 (압축)
            stream.SendNext((int)(currentGameData.gameTime * 1000f));
            stream.SendNext((short)currentGameData.dayNumber);
            stream.SendNext((byte)currentGameData.segment);
            stream.SendNext((byte)currentGameData.timeOfDay);
            stream.SendNext((byte)currentGameData.currentSeason);
            stream.SendNext((byte)currentGameData.seasonDay);
            stream.SendNext((byte)currentGameData.currentWeather);
            stream.SendNext((byte)(currentGameData.weatherIntensity * 100f));

            // 이벤트 데이터 전송
            stream.SendNext((byte)(currentEvent.isActive ? 1 : 0));
            stream.SendNext((short)(currentEvent.remainingTime * 10f));
        }

        /// <summary>
        /// 압축된 데이터 수신
        /// </summary>
        private void ReceiveCompressedData(PhotonStream stream)
        {
            try
            {
                // 시간 데이터 수신 (안전한 타입 캐스팅)
                int timeInt = (int)stream.ReceiveNext();
                float gameTime = timeInt / 1000f;

                short dayShort = (short)stream.ReceiveNext();
                int dayNumber = dayShort;

                byte segmentByte = (byte)stream.ReceiveNext();
                int segment = segmentByte;

                byte timeOfDayByte = (byte)stream.ReceiveNext();
                TimeOfDay timeOfDay = (TimeOfDay)timeOfDayByte;

                byte seasonByte = (byte)stream.ReceiveNext();
                Season season = (Season)seasonByte;

                byte seasonDayByte = (byte)stream.ReceiveNext();
                int seasonDay = seasonDayByte;

                byte weatherByte = (byte)stream.ReceiveNext();
                WeatherType weather = (WeatherType)weatherByte;

                byte intensityByte = (byte)stream.ReceiveNext();
                float weatherIntensity = intensityByte / 100f;

                // 이벤트 데이터 수신
                byte eventActiveByte = (byte)stream.ReceiveNext();
                bool eventActive = eventActiveByte == 1;

                short eventRemainingShort = (short)stream.ReceiveNext();
                float eventRemaining = eventRemainingShort / 10f;

                // 데이터 유효성 검증
                if (!IsValidGameData(gameTime, dayNumber, segment, timeOfDay, season, seasonDay, weather, weatherIntensity))
                {
                    Debug.LogWarning("[NetworkGameEventManager] 수신된 데이터가 유효하지 않습니다. 기본값으로 대체합니다.");
                    return;
                }

                // 수신된 데이터 적용
                ApplyReceivedData(gameTime, dayNumber, segment, timeOfDay, season, seasonDay, weather, weatherIntensity,
                                0f, 0f, 0f, false, false,
                                eventActive, "", GameEventType.None, "", eventRemaining);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NetworkGameEventManager] 압축 데이터 수신 오류: {e.Message}");
                Debug.LogError($"[NetworkGameEventManager] 스택 트레이스: {e.StackTrace}");
            }
        }

        /// <summary>
        /// 게임 데이터 유효성 검증
        /// </summary>
        private bool IsValidGameData(float gameTime, int dayNumber, int segment, TimeOfDay timeOfDay,
                                   Season season, int seasonDay, WeatherType weather, float weatherIntensity)
        {
            try
            {
                // 기본 범위 검증 (더 관대하게)
                if (gameTime < -1f || gameTime > 1000f)
                {
                    Debug.LogWarning($"[NetworkGameEventManager] 게임 시간 범위 오류: {gameTime}");
                    return false;
                }
                if (dayNumber < -1)
                {
                    Debug.LogWarning($"[NetworkGameEventManager] 일수 범위 오류: {dayNumber}");
                    return false;
                }
                if (segment < -1 || segment > 20)
                {
                    Debug.LogWarning($"[NetworkGameEventManager] 세그먼트 범위 오류: {segment}");
                    return false;
                }
                if (seasonDay < -1)
                {
                    Debug.LogWarning($"[NetworkGameEventManager] 계절 일수 범위 오류: {seasonDay}");
                    return false;
                }
                if (weatherIntensity < -0.1f || weatherIntensity > 1.1f)
                {
                    Debug.LogWarning($"[NetworkGameEventManager] 날씨 강도 범위 오류: {weatherIntensity}");
                    return false;
                }

                // 열거형 값 검증 (더 안전하게)
                if (!System.Enum.IsDefined(typeof(TimeOfDay), timeOfDay))
                {
                    Debug.LogWarning($"[NetworkGameEventManager] TimeOfDay 열거형 오류: {timeOfDay}");
                    return false;
                }
                if (!System.Enum.IsDefined(typeof(Season), season))
                {
                    Debug.LogWarning($"[NetworkGameEventManager] Season 열거형 오류: {season}");
                    return false;
                }
                if (!System.Enum.IsDefined(typeof(WeatherType), weather))
                {
                    Debug.LogWarning($"[NetworkGameEventManager] WeatherType 열거형 오류: {weather}");
                    return false;
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NetworkGameEventManager] 유효성 검증 중 오류: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 게임 상태 정보를 CustomProperties로 업데이트
        /// </summary>
        public void UpdateGameStateProperties()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var props = new ExitGames.Client.Photon.Hashtable();
                props["GameTime"] = currentGameData.gameTime;
                props["DayNumber"] = currentGameData.dayNumber;
                props["Segment"] = currentGameData.segment;
                props["TimeOfDay"] = (int)currentGameData.timeOfDay;
                props["CurrentSeason"] = (int)currentGameData.currentSeason;
                props["SeasonDay"] = currentGameData.seasonDay;
                props["CurrentWeather"] = (int)currentGameData.currentWeather;
                props["WeatherIntensity"] = currentGameData.weatherIntensity;

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }

        /// <summary>
        /// 날씨 상태 정보를 CustomProperties로 업데이트
        /// </summary>
        public void UpdateWeatherProperties()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var props = new ExitGames.Client.Photon.Hashtable();
                props["WeatherIntensity"] = currentWeatherData.intensity;
                props["WeatherDuration"] = currentWeatherData.duration;
                props["WeatherRemainingTime"] = currentWeatherData.remainingTime;
                props["IsRaining"] = currentWeatherData.isRaining;
                props["IsSnowing"] = currentWeatherData.isSnowing;

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }

        /// <summary>
        /// 룸 프로퍼티 변경 시 호출
        /// </summary>
        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            foreach (var key in propertiesThatChanged.Keys)
            {
                switch (key.ToString())
                {
                    case "GameTime":
                        currentGameData.gameTime = (float)propertiesThatChanged[key];
                        break;
                    case "DayNumber":
                        currentGameData.dayNumber = (int)propertiesThatChanged[key];
                        break;
                    case "Segment":
                        currentGameData.segment = (int)propertiesThatChanged[key];
                        break;
                    case "TimeOfDay":
                        currentGameData.timeOfDay = (TimeOfDay)propertiesThatChanged[key];
                        break;
                    case "CurrentSeason":
                        currentGameData.currentSeason = (Season)propertiesThatChanged[key];
                        break;
                    case "SeasonDay":
                        currentGameData.seasonDay = (int)propertiesThatChanged[key];
                        break;
                    case "CurrentWeather":
                        currentGameData.currentWeather = (WeatherType)propertiesThatChanged[key];
                        break;
                    case "WeatherIntensity":
                        currentGameData.weatherIntensity = (float)propertiesThatChanged[key];
                        break;
                    case "WeatherDuration":
                        currentWeatherData.duration = (float)propertiesThatChanged[key];
                        break;
                    case "WeatherRemainingTime":
                        currentWeatherData.remainingTime = (float)propertiesThatChanged[key];
                        break;
                    case "IsRaining":
                        currentWeatherData.isRaining = (bool)propertiesThatChanged[key];
                        break;
                    case "IsSnowing":
                        currentWeatherData.isSnowing = (bool)propertiesThatChanged[key];
                        break;
                }
            }
        }

        #endregion
    }
}
