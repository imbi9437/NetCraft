using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherObject : MonoBehaviour
{   
    public bool isReusable; //다회용 수집오브젝트인지
    public float cooltime; //다회용이면 다시자라날 시간
    private float interval; //쿨타임을 계산할 변수  
    public GameObject materialPrefab; //유저인벤토리에 넣을 재료프리펩 
    
    public MeshFilter meshFilter; //다회용 수집오브젝트의 메쉬
    public Mesh ripeMesh; // 채집 가능한 상태의 메쉬
    public Mesh emptyMesh; // 수확 후의 메쉬
    

    private void Awake()
    {
        if (isReusable) //다회용 수집 오브젝트에서만 쿨타임 계산 
        {
            interval = Time.time; 
        }
    }
    
    public void TryGather() //채집을 시도할때 호출할 함수 
    {
        if (isReusable) //다회용 수집 오브젝트일때
        {
            if(Time.time < interval) { Debug.Log("아직 수확시키기 아닙니다 "); return;} //아직 수확시기가 아닌 상태

            if (Time.time >= interval) //수집하는 상태가 될때 
            {
                Collect();
                interval = Time.time + cooltime; //쿨타임 초기화

                meshFilter.mesh = emptyMesh; //수확해버린 메쉬로
                StartCoroutine(ResetState()); //코루틴을 이용하여 쿨타임이 됬을때 자동 메쉬변환
            }
        }
        else
        {   //일회용 수집오브젝트 일떄 인벤토리가 꽉차이있으면 판정 생각해야함 
            Collect();
        }
        
    }

    private void Collect() //수집에 성공했을때의 함수 
    {   //TODO : 유저의 인베토리로 materialPrefab을 넣을 함수를 만들어야함
        Debug.Log("Collect");
        Destroy(gameObject); //수집에 성공하여 파괴
    }

    private IEnumerator ResetState() //자동으로 메쉬를 변하기위한 코루틴 
    {
        yield return new WaitForSeconds(cooltime);
        
        // 쿨타임이 끝나면 메쉬를 다시 채집 가능한 상태로 변경
        meshFilter.mesh = ripeMesh;
    }

}
