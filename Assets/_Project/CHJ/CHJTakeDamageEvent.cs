using _Project.Script.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CHJTakeDamageEvent : IEvent
{
	public int takedamage;

	public CHJTakeDamageEvent(int takedamage)
	{
		this.takedamage = takedamage;
	}

}
