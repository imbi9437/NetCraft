using System.Collections;
using System.Collections.Generic;
using _Project.Script.StateMachine.Native;
using UnityEngine;

public class EnemyIdelState : BaseState<Enemy>
{
    public override void Enter(Enemy owner)
    {
    }

    public override void Update(Enemy owner)
    {
        owner.Patrol();
        owner.Walking();
        owner.Getback();
    }

    public override void Exit(Enemy owner)
    {
    }
    
}
