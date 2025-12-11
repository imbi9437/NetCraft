using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Script.Interface;
using UnityEngine;

public class ResourcesObject : MonoBehaviour ,IHitAble
{
    private float maxHp = 100f; //최대 체력
    [SerializeField]private float currentHp = 0; //현재 체력 
    public Animator mainAnim; //메인 애니메이션
    public GameObject materialPrefab; //부서진후 나올 재료 프리펩
    public Collider ObjectCollider; //죽는 판정후 상호작용을 막기위함
    public float spawnDelay; //재료 나올떄 애니메이션에 따른 딜레이 
    private void Awake()
    {
        currentHp = maxHp; //꺠어날때 체력 초기화
        mainAnim = GetComponent<Animator>();
        ObjectCollider = GetComponent<Collider>();
    }

    public void Hit(float damage)
    {
        if(currentHp <= 0){return;} //애니매이션끝나기전에 다시 상호작용할수있으니 
        
        //데미지를 받았을때 오브젝트가 살짝 떨리면 좋을것 같음
        currentHp -= damage;
        if (currentHp <= 0) //만약 체력이 0이하이면 Die()호출
        {
            mainAnim.SetBool("Die",true); //이 애니메이션 끝에 Die()함수 이벤트로 넣기
            ObjectCollider.enabled = false; //enabled를 이용해서 상호작용을 막을지 or isTrigger을 이용해서 상호작용을 막을지 정해야함
            
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

   

    IEnumerator SpawnResources() //부서질떄 재료를 소환하는 함수 
    {   //TODO : 프리펩을 이용해서 자기 주위에 랜덤으로 떨군다거나 바로밑에 떨구기 

        yield return new WaitForSeconds(spawnDelay); //죽는 애니매이션에 끝날떄 맞춰서 딜레이 주기 
        GameObject objTest = Instantiate(materialPrefab , transform.position, Quaternion.identity);
        Debug.Log("재료가 떨어졌습니다");
    }
    
    
}
