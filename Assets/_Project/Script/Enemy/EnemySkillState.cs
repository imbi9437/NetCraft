using System.Collections;
using System.Collections.Generic;
using _Project.Script.StateMachine.Native;
using UnityEngine;

public class EnemySkillState : BaseState<Enemy>
{
    public override void Enter(Enemy owner)
    {
        // 스킬 사용 시도
        owner.TryUseSkills();
    }

    public override void Update(Enemy owner)
    {
        // 스킬 사용이 끝났는지 체크 (isAtk 플래그로 확인)
        if (!owner.IsAttacking)
        {
            // 스킬 사용이 끝나면 추적 상태로 복귀
            owner.ChangeState(new EnemyChaseState());
        }
    }

    public override void Exit(Enemy owner)
    {
        owner.ResetAttackFlag(); // 상태 나가면 공격 플래그 리셋
        Debug.Log("스킬 상태 종료");
    }
}
