using UnityEngine;

namespace _Project.Script.Character.Player
{
    /// <summary>
    /// 플레이어 상태 타입 열거형
    /// 돈스타브 스타일 플레이어 상태 관리
    /// </summary>
    public enum PlayerStateType
    {
        Idle = 0,           // 대기 상태
        Move = 1,           // 이동 상태
        Interact = 2,       // 상호작용 상태 (아이템 줍기, 구조물 조작)
        Attack = 3,         // 공격 상태
        Hit = 4,
        Eat = 5,           // 음식 섭취 상태
        Build = 6,         // 건설 상태
        Dead = 7,          // 사망 상태
        Stunned = 8,        // 기절 상태 (정신력 0일 때)
    }

    /// <summary>
    /// 플레이어 스탯 타입 열거형
    /// 돈스타브의 핵심 생존 스탯들
    /// </summary>
    public enum StatType
    {
        Health,     // 체력
        Hunger,     // 배고픔
        Thirst,
        Sanity,     // 정신력
        Speed,      // 이동 속도
        Attack,     // 공격력
        Cold,       // 추위 저항력
        Wetness,    // 젖음
        Temperature // 체온
    }

    /// <summary>
    /// 데미지 타입 열거형
    /// 다양한 데미지 소스 구분
    /// </summary>
    public enum DamageType
    {
        Physical,   // 물리 데미지 (공격, 추락)
        Hunger,     // 배고픔 데미지
        Cold,       // 추위 데미지
        Sanity,     // 정신력 데미지 (환각)
        Poison,     // 독 데미지
        Fire        // 화상 데미지
    }
}
