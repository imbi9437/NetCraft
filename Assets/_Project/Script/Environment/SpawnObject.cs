using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Script.Interface;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;
using Photon.Pun;

public class SpawnObject : MonoBehaviour, IHitAble
{
    //스폰 하는 건물 오브젝트 
    [Header("Spawn Settings")] public GameObject spawnPrefabs; //스폰할 프리펩
    public int spawnMaxAmount; //소환맥스치
    public int SpawnNub; //한번에 소환할 개수 
    public float spawnRate; //소환 
    public bool isBossSpawn; //보스 소환용 

    [Header("SpawnerObj settings")] public float maxhp; //오브젝트의 최대체력  
    public float currenthp = 0; //오브젝트의 현재 체력 
    public bool isHP; //체력이 있는 소환옵젝트인지 

    private float spawnDelay; //딜레이 체크용
    private int isBossOk = 0; //보스 소환확인용  
    private List<GameObject> spawnedObjects = new List<GameObject>(); //복사한 prefab관리 리스트 
    public PhotonView photonView; //포톤뷰

    private void Awake()
    {
        spawnDelay = Time.time;
        currenthp = maxhp;
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient == false) return;
        if(isBossOk > 0){return;} //보스 소환후 0보다 크므로 리턴
        if (isBossSpawn) //보스를 소환할때만 
        {
            spawnMaxAmount = 1;
            SpawnNub = 1;
            SpawnPrefab();
            isBossOk += 1;
        }
        // 쿨타임이 지났고, 최대 소환 개체 수를 넘지 않았을 때만 소환 함수를 호출
        else if(Time.time >= spawnDelay && spawnedObjects.Count < spawnMaxAmount) //보스소환이 아닐때 
        {
            SpawnPrefab();
        }
    }

    public void Spawn()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        } //포톤네트워크에서 마스터 클라이언트인지 && photonView에서 

        if (isBossOk > 0)
        {
            return;
        } //보스 소환후 0보다 크므로 리턴

        if (isBossSpawn) //보스를 소환할때만 
        {
            spawnMaxAmount = 1;
            SpawnNub = 1;
            SpawnPrefab();
            isBossOk += 1;
        }
        // 쿨타임이 지났고, 최대 소환 개체 수를 넘지 않았을 때만 소환 함수를 호출
        else if (Time.time >= spawnDelay && spawnedObjects.Count < spawnMaxAmount) //보스소환이 아닐때 
        {
            SpawnPrefab();
        }
    }

    private void SpawnPrefab() //프리펩을 소환맥스치까지 소환하는 함수 
    {
        // 실제로 소환할 개수를 계산합니다.
        int numberToSpawn = SpawnNub;
        if (spawnedObjects.Count + SpawnNub > spawnMaxAmount)
        {
            numberToSpawn = spawnMaxAmount - spawnedObjects.Count;
        }

        // 0마리 이상 소환할 개체가 있을 경우에만 반복문 실행
        if (numberToSpawn > 0)
        {
            for (int i = 0; i < numberToSpawn; i++)
            {
                // GameObject newObject = Instantiate(spawnPrefabs, transform.position + new Vector3(Random.Range(-2,2),0,Random.Range(-2,2)), Quaternion.identity); //기존 소한방법
                GameObject newObject = PhotonNetwork.Instantiate(spawnPrefabs.name,
                    transform.position + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2)),
                    Quaternion.identity); //photonNetwork.Instantiate를 이용한 소환 
                spawnedObjects.Add(newObject);

                Enemy enemyScript = newObject.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    //소환된 적에게 '나(this)를 스포너로 설정해' 라고 알려줍니다.
                    enemyScript.SetSpawn(this);
                }
            }

            // 소환이 완료되면 다음 스폰 딜레이를 초기화합니다.
            spawnDelay = Time.time + spawnRate;
        }
    }

    // 소환된 오브젝트가 파괴될때 이 함수를 호출해야함 
    public void OnObjectDestroyed(GameObject destroyedObject)
    {
        spawnedObjects.Remove(destroyedObject);
    }

    [PunRPC] //
    public void Hit(float damage)
    {
        if (PhotonNetwork.IsMasterClient) //마스터 클라이언트에서만 스포너 삭제 
        {
            if (isHP)
            {
                currenthp -= damage;
                if (currenthp <= 0)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }
}