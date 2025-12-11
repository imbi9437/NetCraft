using System.Collections;
using System.Collections.Generic;
using _Project.Script.StateMachine.Native;
using UnityEngine;

public class EnemyChaseState : BaseState<Enemy>
{
    public override void Enter(Enemy owner)
    {
    }

    public override void Update(Enemy owner)
    {
        owner.TargetChase();
    }

    public override void Exit(Enemy owner)
    {
    }
}
