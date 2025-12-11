using UnityEngine;
using _Project.Script.Character.Player;


namespace _Project.Script.Character.Network
{
    /// <summary>
    /// 플레이어 통계 데이터 구조체
    /// 네트워크 동기화를 위한 플레이어 상태 정보
    /// </summary>
    [System.Serializable]
    public struct PlayerStatData
    {
        [Header("기본 스탯")]
        public float health;
        public float maxHealth;
        public float hunger;
        public float maxHunger;
        public float thirst;
        public float maxThirst;
        public float stamina;
        public float maxStamina;
        public float sanity;
        public float maxSanity;

        [Header("전투 스탯")]
        public float attack;
        public float defense;
        public float speed;
        public float jumpPower;

        [Header("생존 스탯")]
        public float temperature;
        public float wetness;
        public float poison;
        public float disease;

        [Header("경험치")]
        public int level;
        public int experience;
        public int skillPoints;

        /// <summary>
        /// 기본값으로 초기화
        /// </summary>
        public static PlayerStatData Default
        {
            get
            {
                return new PlayerStatData
                {
                    // 기본 스탯
                    health = 100f,
                    maxHealth = 100f,
                    hunger = 100f,
                    maxHunger = 100f,
                    thirst = 100f,
                    maxThirst = 100f,
                    stamina = 100f,
                    maxStamina = 100f,
                    sanity = 100f,
                    maxSanity = 100f,

                    // 전투 스탯
                    attack = 10f,
                    defense = 5f,
                    speed = 5f,
                    jumpPower = 8f,

                    // 생존 스탯
                    temperature = 20f,
                    wetness = 0f,
                    poison = 0f,
                    disease = 0f,

                    // 경험치
                    level = 1,
                    experience = 0,
                    skillPoints = 0
                };
            }
        }

        /// <summary>
        /// 스탯이 유효한 범위 내에 있는지 확인
        /// </summary>
        public bool IsValid()
        {
            return health >= 0f && health <= maxHealth &&
                   hunger >= 0f && hunger <= maxHunger &&
                   thirst >= 0f && thirst <= maxThirst &&
                   stamina >= 0f && stamina <= maxStamina &&
                   sanity >= 0f && sanity <= maxSanity &&
                   level > 0 && experience >= 0 && skillPoints >= 0;
        }

        /// <summary>
        /// 스탯을 안전한 범위로 제한
        /// </summary>
        public void ClampStats()
        {
            health = Mathf.Clamp(health, 0f, maxHealth);
            hunger = Mathf.Clamp(hunger, 0f, maxHunger);
            thirst = Mathf.Clamp(thirst, 0f, maxThirst);
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
            sanity = Mathf.Clamp(sanity, 0f, maxSanity);
            experience = Mathf.Max(0, experience);
            skillPoints = Mathf.Max(0, skillPoints);
        }

        /// <summary>
        /// 스탯 복사
        /// </summary>
        public PlayerStatData Copy()
        {
            return new PlayerStatData
            {
                health = this.health,
                maxHealth = this.maxHealth,
                hunger = this.hunger,
                maxHunger = this.maxHunger,
                thirst = this.thirst,
                maxThirst = this.maxThirst,
                stamina = this.stamina,
                maxStamina = this.maxStamina,
                sanity = this.sanity,
                maxSanity = this.maxSanity,
                attack = this.attack,
                defense = this.defense,
                speed = this.speed,
                jumpPower = this.jumpPower,
                temperature = this.temperature,
                wetness = this.wetness,
                poison = this.poison,
                disease = this.disease,
                level = this.level,
                experience = this.experience,
                skillPoints = this.skillPoints
            };
        }

        /// <summary>
        /// 스탯 비교 (같은지 확인)
        /// </summary>
        public bool Equals(PlayerStatData other)
        {
            return Mathf.Approximately(health, other.health) &&
                   Mathf.Approximately(maxHealth, other.maxHealth) &&
                   Mathf.Approximately(hunger, other.hunger) &&
                   Mathf.Approximately(maxHunger, other.maxHunger) &&
                   Mathf.Approximately(thirst, other.thirst) &&
                   Mathf.Approximately(maxThirst, other.maxThirst) &&
                   Mathf.Approximately(stamina, other.stamina) &&
                   Mathf.Approximately(maxStamina, other.maxStamina) &&
                   Mathf.Approximately(sanity, other.sanity) &&
                   Mathf.Approximately(maxSanity, other.maxSanity) &&
                   Mathf.Approximately(attack, other.attack) &&
                   Mathf.Approximately(defense, other.defense) &&
                   Mathf.Approximately(speed, other.speed) &&
                   Mathf.Approximately(jumpPower, other.jumpPower) &&
                   Mathf.Approximately(temperature, other.temperature) &&
                   Mathf.Approximately(wetness, other.wetness) &&
                   Mathf.Approximately(poison, other.poison) &&
                   Mathf.Approximately(disease, other.disease) &&
                   level == other.level &&
                   experience == other.experience &&
                   skillPoints == other.skillPoints;
        }

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        public override string ToString()
        {
            return $"PlayerStatData: HP({health:F1}/{maxHealth:F1}) " +
                   $"Hunger({hunger:F1}/{maxHunger:F1}) " +
                   $"Thirst({thirst:F1}/{maxThirst:F1}) " +
                   $"Stamina({stamina:F1}/{maxStamina:F1}) " +
                   $"Sanity({sanity:F1}/{maxSanity:F1}) " +
                   $"Level({level}) Exp({experience}) SP({skillPoints})";
        }
    }

    /// <summary>
    /// 스탯 타입 열거형
    /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정(중복이라 삭제)
    /// </summary>
    //public enum StatType
    //{
    //    // 기본 스탯
    //    Health,
    //    MaxHealth,
    //    Hunger,
    //    MaxHunger,
    //    Thirst,
    //    MaxThirst,
    //    Stamina,
    //    MaxStamina,
    //    Sanity,
    //    MaxSanity,

    //    // 전투 스탯
    //    Attack,
    //    Defense,
    //    Speed,
    //    JumpPower,

    //    // 생존 스탯
    //    Temperature,
    //    Wetness,
    //    Poison,
    //    Disease,
    //    Cold,

    //    // 경험치
    //    Level,
    //    Experience,
    //    SkillPoints
    //}

    /// <summary>
    /// 스탯 변경 이벤트 데이터
    /// </summary>
    [System.Serializable]
    public struct StatChangeData
    {
        public StatType statType;
        public float oldValue;
        public float newValue;
        public float changeAmount;
        public bool isIncrease;

        public StatChangeData(StatType type, float oldVal, float newVal)
        {
            statType = type;
            oldValue = oldVal;
            newValue = newVal;
            changeAmount = newVal - oldVal;
            isIncrease = changeAmount > 0f;
        }
    }
}
