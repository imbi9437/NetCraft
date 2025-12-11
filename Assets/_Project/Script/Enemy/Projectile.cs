using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Script.Interface;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // 이 투사체가 날아갈 목표
    public float speed = 10f;
    private Vector3 _targetPosition;
    private float _damage;
    private bool _isTargeting;
    private bool _isdummy;
    
    // Enemy.FireProjectile RPC에서 호출될 초기화 함수
    public void Initialize(Vector3 targetPos, float damage, bool isTargeting ,bool isdummy)
    {
        _targetPosition = targetPos;
        _damage = damage;
        _isTargeting = isTargeting;
        _isdummy = isdummy;
        
        // 투사체는 로컬에서만 존재하므로, 로컬에서 일정 시간 후 파괴 예약합니다.
        Destroy(gameObject, 5f);
    }

    public void targetPosUpdate(Vector3 targetingpos)
    {
        _targetPosition = targetingpos;
    }
    private void Update()
    {
        if (_isTargeting)
        {
            UseTarget();
        }
        else
        {
            Nontarget();
        }
    }

    public void UseTarget() //타겟팅 투사체
    {
        // 현재 위치에서 목표 위치로 speed만큼 이동
        transform.position = Vector3.MoveTowards(transform.position,_targetPosition, speed * Time.deltaTime);
        
        float distance = Vector3.Distance(transform.position, _targetPosition);
        if (distance < 0.01f)
        {
            Destroy(gameObject); //만약 
        }
    }
    
    public void Nontarget() //논타겟 투사체 
    {
        transform.position += transform.forward * speed * Time.deltaTime; //그냥 앞으로 직진하는 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !_isdummy) //자신의 캐릭터만 Layer를 Player로 하고 나머지는 다른레이어라고 생각하고 만든 코드입니다 체력 동기화는 플레이어 쪽에서 해주세요 
        {
            other.GetComponent<IHitAble>().Hit(_damage); //player 데미지 받는건 테스트용 입니다 
            Debug.Log("Hit 성공 ");
        }
        Destroy(gameObject);
    }
}
