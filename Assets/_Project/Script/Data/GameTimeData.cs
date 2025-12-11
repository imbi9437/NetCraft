using UnityEngine;
using _Project.Script.EventStruct;
using _Project.Script.UI;

namespace _Project.Script.Data
{
    /// <summary>
    /// 게임 시간 데이터
    /// </summary>
    [System.Serializable]
    public class GameTimeData
    {
        [Header("시간 정보")]
        private float _gameTime;
        private int _dayNumber;
        private int _segment;
        private TimeOfDay _timeOfDay;

        [Header("계절 정보")]
        private Season _currentSeason;
        private int _seasonDay;

        [Header("날씨 정보")]
        private _Project.Script.EventStruct.WeatherType _currentWeather;
        private float _weatherIntensity;

        [Header("상태 정보")]
        public bool isInitialized;

        public float gameTime
        {
            get => _gameTime;
            set
            {
                float oldValue = _gameTime;
                _gameTime = value;
                if (Mathf.Abs(oldValue - _gameTime) > 0.01f)
                {
                    UIEventDispatcher.DispatchGameTimeChanged(oldValue, _gameTime);
                }
            }
        }

        public int dayNumber
        {
            get => _dayNumber;
            set
            {
                int oldValue = _dayNumber;
                _dayNumber = value;
                if (oldValue != _dayNumber)
                {
                    UIEventDispatcher.DispatchUIRefresh("GameInfo");
                }
            }
        }

        public int segment
        {
            get => _segment;
            set => _segment = value;
        }

        public TimeOfDay timeOfDay
        {
            get => _timeOfDay;
            set => _timeOfDay = value;
        }

        public Season currentSeason
        {
            get => _currentSeason;
            set
            {
                Season oldValue = _currentSeason;
                _currentSeason = value;
                if (oldValue != _currentSeason)
                {
                    UIEventDispatcher.DispatchSeasonChanged((int)oldValue, (int)_currentSeason);
                }
            }
        }

        public int seasonDay
        {
            get => _seasonDay;
            set => _seasonDay = value;
        }

        public _Project.Script.EventStruct.WeatherType currentWeather
        {
            get => _currentWeather;
            set
            {
                _Project.Script.EventStruct.WeatherType oldValue = _currentWeather;
                _currentWeather = value;
                if (oldValue != _currentWeather)
                {
                    UIEventDispatcher.DispatchWeatherChanged((int)oldValue, (int)_currentWeather);
                }
            }
        }

        public float weatherIntensity
        {
            get => _weatherIntensity;
            set => _weatherIntensity = value;
        }

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public GameTimeData()
        {
            _gameTime = 0f;
            _dayNumber = 1;
            _segment = 0;
            _timeOfDay = TimeOfDay.Day;
            _currentSeason = Season.Autumn;
            _seasonDay = 1;
            _currentWeather = _Project.Script.EventStruct.WeatherType.Clear;
            _weatherIntensity = 0f;
            isInitialized = false;
        }

        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        public GameTimeData(float time, int day, int seg, TimeOfDay tod, Season season, int sDay, _Project.Script.EventStruct.WeatherType weather, float intensity)
        {
            _gameTime = time;
            _dayNumber = day;
            _segment = seg;
            _timeOfDay = tod;
            _currentSeason = season;
            _seasonDay = sDay;
            _currentWeather = weather;
            _weatherIntensity = intensity;
            isInitialized = true;
        }

        /// <summary>
        /// 데이터 복사
        /// </summary>
        public GameTimeData Clone()
        {
            return new GameTimeData(_gameTime, _dayNumber, _segment, _timeOfDay, _currentSeason, _seasonDay, _currentWeather, _weatherIntensity);
        }

        /// <summary>
        /// 데이터 업데이트
        /// </summary>
        public void Update(GameTimeData other)
        {
            gameTime = other.gameTime;
            dayNumber = other.dayNumber;
            segment = other.segment;
            timeOfDay = other.timeOfDay;
            currentSeason = other.currentSeason;
            seasonDay = other.seasonDay;
            currentWeather = other.currentWeather;
            weatherIntensity = other.weatherIntensity;
        }

        /// <summary>
        /// 게임 정보 전체 업데이트 이벤트 발송
        /// </summary>
        public void DispatchGameInfoUpdate()
        {
            UIEventDispatcher.DispatchGameInfoUpdate(
                _gameTime,
                _dayNumber,
                (int)_currentSeason,
                (int)_currentWeather,
                20f // 기본 온도 (실제로는 계산된 값이어야 함)
            );
        }
    }
}
