using _Project.Script.Interface;
using UnityEngine;

namespace _Project.Script.EventStruct
{
    // EventHub를 통해 사운드 재생을 요청하는 이벤트
    public struct RequestPlaySoundEvent : IEvent
    {
        public string id;                 // 등록된 id 또는 Resources 경로 키
        public AudioClip clip;            // 직접 재생용
        public Vector3 position;          // 3D 위치
        public Transform parent;          // 부모(선택)
        public float volume;              // 0~1
        public float pitch;               // >0
        public float spatialBlend;        // 0~1
        public bool loop;                 // 루프 여부
        public string mixerGroupName;     // 출력 그룹명
    }

    // EventHub를 통해 이펙트 재생을 요청하는 이벤트
    public struct RequestPlayEffectEvent : IEvent
    {
        public string id;                 // 등록된 id 또는 Resources 경로 키
        public GameObject prefab;         // 직접 재생용 프리팹
        public Vector3 position;          // 위치
        public Quaternion rotation;       // 회전
        public Transform parent;          // 부모(선택)
        public float duration;            // 자동 회수 딜레이
        public bool worldSpace;           // 월드/로컬 좌표
        public Vector3 scale;             // 스케일
    }

    // EventHub를 통해 오디오 믹서 볼륨 변경을 요청하는 이벤트
    public struct RequestChangeVolumeEvent : IEvent
    {
        public string mixerGroupName;     // 믹서 그룹 이름 (Master,MusicVolume, SFXVolume, UIVolume, MicVolume)
        public float volume;              // 볼륨 값 (0~1)

        public RequestChangeVolumeEvent(string mixerGroupName, float volume)
        {
            this.mixerGroupName = mixerGroupName;
            this.volume = volume;
        }
    }

    // 오디오 믹서 볼륨이 변경되었을 때 발생하는 이벤트
    public struct OnVolumeChangedEvent : IEvent
    {
        public string mixerGroupName;
        public float volume;

        public OnVolumeChangedEvent(string mixerGroupName, float volume)
        {
            this.mixerGroupName = mixerGroupName;
            this.volume = volume;
        }
    }

    // EventHub를 통해 마이크 토글을 요청하는 이벤트
    public struct RequestToggleMicrophoneEvent : IEvent
    {
        public bool? enable; // null = 토글, true = 강제 켜기, false = 강제 끄기

        public RequestToggleMicrophoneEvent(bool? enable = null)
        {
            this.enable = enable;
        }
    }

    // 마이크 상태가 변경되었을 때 발생하는 이벤트
    public struct OnMicrophoneStateChangedEvent : IEvent
    {
        public bool isEnabled;

        public OnMicrophoneStateChangedEvent(bool isEnabled)
        {
            this.isEnabled = isEnabled;
        }
    }
}


