using System;
using System.Collections;
using UnityEngine;

public class ChargeSkill : MonoBehaviour, ISkill
{
    [SerializeField] private float moveRange = 15f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float cooldown = 30f;

    private float nextAvailableTime = 0f;

    public string Name => "Charge";
    public float Cooldown => cooldown;
    public bool IsReady() => Time.time >= nextAvailableTime;

    private void Start()
    {
        Debug.Log($"ChargeSkill 초기화: Name={Name}, Cooldown={Cooldown}, IsReady={IsReady()}");
    }

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
        
        if (target == null)
        {
            Debug.Log("타겟이 사라져서 스킬이 중단됨");
            owner.ChangeState(new EnemyIdelState());
            owner.ResetAttackFlag();
            yield break;
        }
        anim.SetBool("Charge_Skill",true);
        agent.isStopped = true;
        agent.updateRotation = false;
        
        owner.StartAttackFlag();
        owner.ChargeDamageSet(damage);
        owner.BossColliderOn();

        Vector3 dir = (target.position - owner.transform.position).normalized;
        dir.y = 0;
        
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        owner.transform.rotation = targetRotation;

        float distance = 0f;
        while (distance < moveRange)
        {
            float step = speed * Time.deltaTime;
            owner.transform.position += dir * step;
            distance += step;
            yield return null;
        }
        
        anim.SetBool("Charge_Skill", false);
        yield return new  WaitForSeconds(2.5f); //포효를 기다리는 시간 
        agent.isStopped = false;
        agent.updateRotation = true;
        owner.ResetAttackFlag();
        owner.BossColliderOff();
        Debug.Log("Charge Skill 종료");
        owner.ChangeState(new EnemyChaseState());
    }
}