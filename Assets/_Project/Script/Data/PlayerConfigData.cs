using UnityEngine;
using System.Collections.Generic;
using _Project.Script.Character.Player;
using _Project.Script.Generic;

namespace _Project.Script.Data
{
    /// <summary>
    /// 플레이어 설정 데이터 (하드코딩 제거)
    /// ScriptableObject를 통한 중앙 집중식 데이터 관리
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/Player Config")]
    public class PlayerConfigData : ScriptableObject
    {
        [Header("기본 스탯 설정")]
        [SerializeField] private float initialHealth = 100f;
        [SerializeField] private float initialHunger = 100f;
        [SerializeField] private float initialSanity = 100f;
        [SerializeField] private float initialSpeed = 5f;
        [SerializeField] private float initialAttack = 10f;

        [Header("스탯 변화율")]
        [SerializeField] private float healthRegenRate = 0.1f;
        [SerializeField] private float hungerDecayRate = 0.1f;
        [SerializeField] private float sanityDecayRate = 0.05f;
        [SerializeField] private float coldDecayRate = 0.02f;

        [Header("데미지 설정")]
        [SerializeField] private float hungerDamageRate = 1f;
        [SerializeField] private float coldDamageRate = 0.5f;
        [SerializeField] private float sanityDamageRate = 0.2f;

        [Header("이동 설정")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float interactionRange = 2f;

        [Header("인벤토리 설정")]
        [SerializeField] private int maxSlots = 15;


        public float RemainHunger => hungerDecayRate;
        public float HungerDamage => hungerDamageRate;

        public PlayerData CreatePlayerData(string uid, string nickName)
        {
            var player = new PlayerData();

            player.playerName = nickName;
            player.maxHp = initialHealth;
            player.maxHunger = initialHunger;
            player.maxSanity = initialSanity;
            
            player.hp = player.maxHp;
            player.hunger = player.maxHunger;
            player.sanity = player.maxSanity;
            player.attack = initialAttack;
            player.speed = initialSpeed;

            player.equippedItems = new Equipment();
            player.inventory = new Inventory();
            
            PlayerData.SetPosition(player, Vector3.zero);
            PlayerData.SetRotation(player, Quaternion.identity);

            return player;
        }
    }
}
