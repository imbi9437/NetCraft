using _Project.Script.StateMachine.Mono;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHJPlayerStateMachine : MonoStateMachine
{
	public enum PlayerState
	{
		Idle,
		Walk,
		Run,
		Hit,
	}

	public void ChangeState(PlayerState state)
	{
		ChangeState((int)state);
	}


}
