using System.Collections;
using System.Collections.Generic;
using _Project.Script.StateMachine.Native;
using UnityEngine;

public class EnemyDizzyState : BaseState<Enemy>
{   
    //스턴 상태일떄 
    public override void Enter(Enemy owner)
    {
        // Dizzy 상태에 진입할 때 단 한 번만 호출
        owner.mainAnim.SetBool("Dizzy", true);
        
        // 스턴 코루틴 시작은 Enter에서 하는 것이 가장 좋습니다.
        owner.StartCoroutine(owner.Dizzytimer());
    }

    public override void Update(Enemy owner)
    {
        owner.Dizzy();
    }

    public override void Exit(Enemy owner)
    {
    }
}
