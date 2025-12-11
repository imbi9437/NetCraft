using UnityEngine;
using System;
using _Project.Script.EventStruct;

namespace _Project.Script.Data
{
    /// <summary>
    /// 활성 게임 이벤트 데이터
    /// </summary>
    [System.Serializable]
    public class ActiveGameEvent
    {
        [Header("이벤트 정보")]
        public string eventId;
        public GameEventType eventType;
        public string eventName;
        public string description;

        [Header("시간 정보")]
        public float startTime;
        public float duration;
        public float remainingTime;
        public bool isActive;

        [Header("효과 정보")]
        public float intensity;
        public string[] affectedSystems;
        public string[] affectedPlayers;

        [Header("위치 정보")]
        public Vector3 position;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public ActiveGameEvent()
        {
            eventId = "";
            eventType = GameEventType.None;
            eventName = "";
            description = "";
            startTime = 0f;
            duration = 0f;
            remainingTime = 0f;
            isActive = false;
            intensity = 0f;
            affectedSystems = new string[0];
            affectedPlayers = new string[0];
            position = Vector3.zero;
        }

        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        public ActiveGameEvent(string id, GameEventType type, string name, string desc, float dur, float inten)
        {
            eventId = id;
            eventType = type;
            eventName = name;
            description = desc;
            startTime = Time.time;
            duration = dur;
            remainingTime = dur;
            isActive = true;
            intensity = inten;
            affectedSystems = new string[0];
            affectedPlayers = new string[0];
            position = Vector3.zero;
        }

        /// <summary>
        /// 이벤트 시작
        /// </summary>
        public void StartEvent()
        {
            startTime = Time.time;
            remainingTime = duration;
            isActive = true;
        }

        /// <summary>
        /// 이벤트 업데이트
        /// </summary>
        public void UpdateEvent(float deltaTime)
        {
            if (isActive && remainingTime > 0f)
            {
                remainingTime -= deltaTime;
                if (remainingTime <= 0f)
                {
                    EndEvent();
                }
            }
        }

        /// <summary>
        /// 이벤트 종료
        /// </summary>
        public void EndEvent()
        {
            isActive = false;
            remainingTime = 0f;
        }

        /// <summary>
        /// 이벤트 강제 종료
        /// </summary>
        public void ForceEnd()
        {
            EndEvent();
        }

        /// <summary>
        /// 이벤트 복사
        /// </summary>
        public ActiveGameEvent Clone()
        {
            return new ActiveGameEvent(eventId, eventType, eventName, description, duration, intensity)
            {
                startTime = this.startTime,
                remainingTime = this.remainingTime,
                isActive = this.isActive,
                affectedSystems = (string[])this.affectedSystems.Clone(),
                affectedPlayers = (string[])this.affectedPlayers.Clone()
            };
        }
    }

}
