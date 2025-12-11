using _Project.Script.StateMachine.Mono;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHJIdleState : MonoState
{
	public override int index => (int)CHJPlayerStateMachine.PlayerState.Idle;


	public override void Update()
	{
	}
	public override void OnEnable()
	{
	}

	public override void OnDisable()
	{
	}


}
