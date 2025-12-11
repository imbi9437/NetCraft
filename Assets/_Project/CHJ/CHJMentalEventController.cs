using _Project.Script.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CHJMentalEventController : MonoBehaviour
{

	//여기서 EvnetHib.Instance.RaiseEvent(new CHJMentalEvent(MentalState.ㅇㅇ))로 이벤트를 발행할수 있음.
	private void Start()
	{
		
	}


	IEnumerator MentalAttack()
	{
		yield return new WaitForSeconds(10f);
		EventHub.Instance.RaiseEvent(new CHJMentalEvent(MentalState.공포));


	}

}
