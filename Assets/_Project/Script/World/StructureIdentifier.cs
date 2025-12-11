using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 구조물 식별자 컴포넌트
/// PhotonNetwork.Instantiate로 생성된 구조물에 부착되어 구조물 ID를 관리
/// </summary>
public class StructureIdentifier : MonoBehaviour
{
    [Header("구조물 정보")]
    [SerializeField] private int structureId = -1;
    [SerializeField] private float health = 100f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool isDestroyable = true;

    /// <summary>
    /// 구조물 ID
    /// </summary>
    public int StructureId
    {
        get => structureId;
        set => structureId = value;
    }

    /// <summary>
    /// 현재 체력
    /// </summary>
    public float Health
    {
        get => health;
        set => health = Mathf.Clamp(value, 0f, maxHealth);
    }

    /// <summary>
    /// 최대 체력
    /// </summary>
    public float MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }

    /// <summary>
    /// 파괴 가능 여부
    /// </summary>
    public bool IsDestroyable
    {
        get => isDestroyable;
        set => isDestroyable = value;
    }

    /// <summary>
    /// 체력 비율 (0~1)
    /// </summary>
    public float HealthRatio => maxHealth > 0 ? health / maxHealth : 0f;

    /// <summary>
    /// 구조물이 파괴되었는지 확인
    /// </summary>
    public bool IsDestroyed => health <= 0f;

    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(float amount)
    {
        Health += amount;
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(float damage)
    {
        Health -= damage;

        if (IsDestroyed)
        {
            OnStructureDestroyed();
        }
    }

    /// <summary>
    /// 구조물 파괴 시 호출
    /// </summary>
    private void OnStructureDestroyed()
    {
        Debug.Log($"[StructureIdentifier] 구조물 {structureId} 파괴됨");
    }

    private void Start()
    {
        // 초기화
        if (structureId == -1)
        {
            Debug.LogWarning($"[StructureIdentifier] 구조물 ID가 설정되지 않음: {gameObject.name}");
        }
    }
}
