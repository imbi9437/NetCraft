using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SmashSkill : MonoBehaviour , ISkill
{
    [SerializeField] private float atkRange;
    [SerializeField] private float damage;
    [SerializeField] private float cooldown;
    
    private float nextAvailableTime = 0f;
    
    public string Name => "Smash";
    public float Cooldown => cooldown;
    public bool IsReady() => Time.time >= nextAvailableTime;
   
    

    public void Use(Enemy owner)
    {
        if (!IsReady() || !owner.HasTarget) return;

        nextAvailableTime = Time.time + Cooldown;
        owner.StartCoroutine(ChargeRoutine(owner));
    }
    
    private IEnumerator ChargeRoutine(Enemy owner)
    {
        var anim = owner.Animator;
        var agent = owner.Agent;
        var target = owner.Target;
        var photonView = owner.PhotonView;

        if (target == null)
        {
            Debug.Log("타겟이 사라져서 스킬이 중단됨");
            owner.ChangeState(new EnemyIdelState());
            owner.ResetAttackFlag();
            yield break;
        }
        anim.SetBool("Smash_Skill",true);
        agent.isStopped = true;
        agent.updateRotation = false;
        owner.StartAttackFlag(); //isatk = true

        Vector3 dir = (target.position - owner.transform.position).normalized;
        dir.y = 0;
        
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        owner.transform.rotation = targetRotation; //적 바라보기 
        
        Vector3 smashCenter = owner.transform.position + transform.forward * atkRange; //데칼 위치 계산 
        smashCenter.y = owner.transform.position.y; // Y축은 적의 위치에 고정 (땅바닥)
        
        photonView.RPC("ShowSmashDecal", RpcTarget.All, smashCenter, atkRange); //데칼 위치 및 시각화
       
        
        // 3초 동안 크기를 키우는 루프 실행
        float warningTime = 3.0f; // 총 경고 시간
        float startTime = Time.time;
    
        // 최종 도달해야 할 데칼의 너비/높이 (공격 사거리에 맞춤)
        float targetSize = atkRange; 
    
        while (Time.time < startTime + warningTime)
        {
            // 0에서 1 사이의 진행률(Progress) 계산
            float elapsedTime = Time.time - startTime;
            float progress = elapsedTime / warningTime; // 0.0 -> 1.0

            // 크기를 선형적으로 증가 (Lerp 함수 사용)
            float currentSize = Mathf.Lerp(0f, targetSize * 2, progress); 
            
            photonView.RPC("UpdateSmashDecalSize", RpcTarget.All, currentSize); //데칼 크기업데이트

            yield return null; // 다음 프레임까지 대기
        }

        //OverlapSphere 판정
        Collider[] smashTarget =  Physics.OverlapSphere(smashCenter, targetSize , owner.playerLayer );
        foreach (Collider col in smashTarget)
        {
            col.GetComponent<PlayerTest>().Hit(damage);
            Debug.Log("스메쉬 판정 성공 ");
        }
        anim.SetBool("Smash_Skill", false);
        photonView.RPC("HideSmashDecal", RpcTarget.All); //스킬사용 종료후 데칼 숨기기 
        yield return new WaitForSeconds(2.5f); // 포효하는 딜레이를 위해 
        agent.updateRotation = true; // 자동으로 회전하는걸 막기위해 false
        agent.isStopped = false;
        owner.ResetAttackFlag();
        owner.ChangeState(new EnemyChaseState()); // 상태 변경 
        
    }
}
