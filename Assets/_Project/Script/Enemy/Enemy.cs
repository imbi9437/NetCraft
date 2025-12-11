using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Script.Interface;
using UnityEngine;
using UnityEngine.AI;
using _Project.Script.StateMachine.Native;
using Photon.Pun;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

/// <summary>
/// 적에 대한 스크립트로 보스만 스킬을 사용할수 있으며 보스의 스킬은 BossSkill파일에 있습니다 컴포넌트에 넣으시면 됩니다
/// 적은 근접,원거리가 있습니다
/// 스턴도 보스한테만 있습니다
/// 적의 Hit함수를 가져올려면 HitRPC함수를 불러와야합니다  
/// </summary>
public class Enemy : MonoBehaviour, IHitAble
{
    [Header("Boss settings")] public bool isBoss = false; //이몹이 보스인지 
    public DecalProjector decalProjector; //보스판정 보여줄 데칼용
    public float dizzyTime; //몇초 스턴을 먹일건지 
    private float chargeDamage; //차지스킬데미지 

    private int currentStunPhase = 0; // 마지막으로 스턴을 발동시킨 구간용 1 : 75% , 2 : 50% , 3 : 25% 

    [Header("Enemy settings")] public LayerMask playerLayer; //플레이어 레이마스크 
    public Transform raycastPoint; //raycast 시작 지점
    public Animator mainAnim; //메인 애니메이터 
    public float maxHp = 100; //최대 체력
    private float currentHp = 0; //현재 체력

    public float atkDamage; //공격력 
    public float atkRange; //공격사거리 
    public float atkDelay = 3f; //곻격 쿨타임 
    public float atkDamageDelay; //공격 판정 시간 공격애니매이션시작후 몇초후에 공격raycast를 쏠지 

    public float chaseRange = 10f; // 추적사거리
    public float chaseSpeed = 3f; //추적속도
    public float raycastRange; //raycast거리

    private float atknextTiem = 0; //atk쿨 
    private float wanderTimer = 0f; //랜덤으로 움직일 텀
    private Vector3 returnPos; //추적을 놓친후 돌아올 pos
    private Vector3 spawnerpos; //스포너 주변을 돌기위한 pos

    [Header("Range Enemy settings")] public GameObject projectilePrefab; //투사체 프리펩
    public bool isRange; //원기리 적인가 
    public bool isprojectileTargeting = false; //투사체가 타겟팅인가
    public bool isdummy = false; //투사체가 더미인가 

    [Header("Enemy State")] private StateMachine<Enemy> stateMachine; //스테이트 머신 
    private bool isDead = false; //죽음판정
    private bool isHitCor = false; //맞은 코루틴 중복방지용 
    private bool isAtk = false; //공격중인지 아닌지 
    private bool isDizzy = false; //스턴중인지 아닌지
    private bool isReturn = false; //돌아가는 상황인가 

    private NavMeshAgent agent; //Nav
    private Transform target; //포착한 플레이어
    private Collider enemyCollider; //적의 콜라이더 보스한테만 필요하긴함 

    private SpawnObject _spawner; //스포너 관리 목적 
    [SerializeField] private PhotonView photonView; //포톤뷰


    //스킬 리스트
    private List<ISkill> skills = new List<ISkill>();

    // ===== Getter (읽기 전용) =====
    public Animator Animator => mainAnim;
    public NavMeshAgent Agent => agent;
    public bool HasTarget => target != null;
    public Transform Target => target;
    public bool IsAttacking => isAtk;
    public PhotonView PhotonView => photonView;

    // ===== Setter/행동 메서드 =====
    public void SetTarget(Transform newTarget) => target = newTarget;
    public void ResetAttackFlag() => isAtk = false;
    public void StartAttackFlag() => isAtk = true;
    public void ChangeState(BaseState<Enemy> newState) => stateMachine.ChangeState(newState);
    public void ChargeDamageSet(float damage) => chargeDamage = damage; //차지 스킬용 
    public void BossColliderOn() => enemyCollider.enabled = true; //차지 판정 on용 
    public void BossColliderOff() => enemyCollider.enabled = false; //차지 판정 off용 

    private void Awake()
    {
        mainAnim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        currentHp = maxHp;
        enemyCollider = GetComponent<Collider>();
        atknextTiem = Time.time;
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)//마스터 클라이언트가 아닐떄 and 포톤에 연결 안되있을때
        {
            return;
        }
        
        // 이 적을 Owner로 하는 상태머신 생성
        stateMachine = new StateMachine<Enemy>(this);
        stateMachine.ChangeState(new EnemyIdelState());

        if (isBoss) //보스가 아닐떄는  스킬등록안함
        {
            // Enemy가 가진 모든 ISkill 자동 등록
            skills.AddRange(GetComponents<ISkill>());

            // 디버그 추가: 컴포넌트 개수와 이름 출력
            Debug.Log($"{gameObject.name}에 발견된 ISkill 컴포넌트 개수: {skills.Count}");

            foreach (var s in skills)
            {
                Debug.Log($"{gameObject.name} 스킬 추가됨: {s.Name}, 준비 상태: {s.IsReady()}");
            }
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)//마스터 클라이언트가 아닐떄 and 포톤에 연결 안되있을때
        {
            return;
        } 

        if (isDead)
        {
            return; //죽음 상태일떄 리턴 
        }
        stateMachine.Update();
    }

    #region EnemyIdelState Methods

    //IdelState에서 쓸 메소드(함수) 모음 

    public void Patrol() //Raycast를 구로 쏘고 palyer를 찾으면 target으로 하고 chaseState로 전환
    {
        Collider[] hits = Physics.OverlapSphere(raycastPoint.position, raycastRange);

        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
            {
                if (!isReturn)
                {
                    returnPos = gameObject.transform.position; // Patrol()이 처음 실행될 때만 원래 위치를 저장
                    isReturn = true;
                }

                Debug.Log("플레이어 발견!");
                target = col.transform; //타겟을 플레이어 Transform으로 변경 
                stateMachine.ChangeState(new EnemyChaseState()); //추적 스태이터스로 변경
                return;
            }
        }
    }

    public void Getback()
    {
        if (isReturn) //돌아가는 상황에서만 함수시작 
        {
            agent.SetDestination(returnPos);
            if (Vector3.Distance(transform.position, returnPos) < 0.5f)
            {
                isReturn = false;
                float speed = agent.velocity.magnitude;
                mainAnim.SetFloat("Walk", speed, 0.1f, Time.deltaTime);
            }
        }
    }

    public void Walking() //2~3초 쿨타임으로 랜덤한 위치로 이동하는 함수 
    {
        if (isReturn || isAtk)
        {
            return;
        } //돌아가는 상황에선 return

        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            // 스포너 위치에서 반경 10m 내의 랜덤한 위치로 이동
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += spawnerpos;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            wanderTimer = Random.Range(2f, 3f); //2~3초 사이에 랜덤으로 이동 
        }
        
        float speed = agent.velocity.magnitude;
        mainAnim.SetFloat("Walk", speed, 0.1f, Time.deltaTime);
    }

    #endregion

    #region EnemyChaseState Methods

    public void TargetChase()
    {
        if (target == null)
        {
            stateMachine.ChangeState(new EnemyIdelState());
            return;
        }

        if (isAtk)
        {
            return;
        } //공격중이면 return

        //Target의 Transform을 이용하여 따라감 
        float distance = Vector3.Distance(target.position, raycastPoint.position);
        if (distance > chaseRange)
        {
            //플레이어를 놓쳤을때 
            target = null;
            Debug.Log("타겟을 놓침");
            isReturn = true;
            stateMachine.ChangeState(new EnemyIdelState()); //기본 state로 전환
            return; //혹시모르는 return;
        }

        if (isRange)
        {
            MaintainDistanceFromTarget();
        }
        else
        {
            agent.speed = chaseSpeed;
            agent.destination = target.position;
        }

        float speed = agent.velocity.magnitude;
        mainAnim.SetFloat("Walk", speed, 0.1f, Time.deltaTime);

        Vector3 lookDirection = target.position - transform.position; //플레이어를 바라보게하기위한 Direction
        lookDirection.y = 0; // y축 회전 고정
        if (lookDirection != Vector3.zero)
        {
            transform.LookAt(transform.position + lookDirection); //플레이어 보기 
        }

        if (atkRange > distance && atknextTiem <= Time.time)
        {
            //공격사거리안에 적이 있을 경우 
            stateMachine.ChangeState(new EnemyAttackState()); //공격 state로 전환
        }
    }


    /// <summary>
    /// 원거리 적이 타겟과의 거리를 유지하며 이동
    /// </summary>
    private void MaintainDistanceFromTarget()
    {
        float desiredDistance = 7f; // 유지할 거리
        float currentDistance = Vector3.Distance(transform.position, target.position);

        if (currentDistance > desiredDistance + 1f)
        {
            // 너무 멀면 가까이 이동
            agent.speed = chaseSpeed;
            agent.destination = target.position;
        }
        else if (currentDistance < desiredDistance - 1f)
        {
            // 너무 가까우면 멀어짐
            Vector3 retreatDirection = (transform.position - target.position).normalized;
            Vector3 retreatPosition = transform.position + retreatDirection * 2f;
            agent.speed = chaseSpeed;
            agent.destination = retreatPosition;
        }
    }
    
    #endregion

    #region EnemyAttack Methods

    public void EnemyAttack()
    {
        if (isAtk)
        {
            return;
        }

        if (target == null)
        {
            stateMachine.ChangeState(new EnemyIdelState());
            return;
        }

        // 보스일 경우 ISkill 인터페이스로 스킬 사용
        if (isBoss)
        {
            foreach (var skill in skills)
            {
                if (skill.IsReady())
                {
                    stateMachine.ChangeState(new EnemySkillState());
                    return;
                }
            }
        }

        mainAnim.SetBool("atk", true); //공격 애니매이션 실행 
        atknextTiem = Time.time + atkDelay; //공격 쿨타임 초기화
        if (isRange) //원거리 적인지  
        {
            StartCoroutine(RangeAtk(atkDamageDelay, 5f));
            isAtk = true; //지금 공격중
            agent.destination = gameObject.transform.position; //그자리에 멈춤
            Debug.Log("플레이어에게 원거리 공격");
        }
        else
        {
            StartCoroutine(hitzone(atkDamageDelay));
            isAtk = true;
            agent.destination = gameObject.transform.position; //그자리에 멈춤
            Debug.Log("플레이어에게 근접 공격!");
        }
    }

    private IEnumerator hitzone(float hitDelay) //근접공격판정 함수
    {
        // hitDelay만큼  기다린 후 공격 판정
        yield return new WaitForSeconds(hitDelay);

        // 공격 판정 구의 중심점을 계산
        Vector3 sphereCenter = transform.position + transform.forward * (atkRange / 2f);

        // 구의 반지름을 공격 사거리의 절반으로 설정
        float sphereRadius = atkRange / 2f;

        // 계산된 중심점과 반지름으로 OverlapSphere 판정
        Collider[] hits = Physics.OverlapSphere(sphereCenter, sphereRadius, playerLayer);

        // 구에 맞은 모든 오브젝트를 순회
        foreach (var hit in hits)
        {
            //todo : 플레이어에게 데미지줄 함수 넣기 
            hit.transform.GetComponent<IHitAble>().Hit(atkDamage);
        }

        // 공격 판정 후 상태를 추격 상태로 변경
        stateMachine.ChangeState(new EnemyChaseState());
        isAtk = false;
        mainAnim.SetBool("atk", false);
    }

    private List<Projectile> _myProjectiles = new List<Projectile>(); //투사체 관리리스트

    private IEnumerator RangeAtk(float hitDelay, float endtime) //원거리 공격 판정 함수 
    {
        yield return new WaitForSeconds(hitDelay); //애니매이션에 맞춰서 기다림 

        Vector3 targetPosition = target.position; // 타겟의 현재 위치
        float endtimer = Time.time + endtime;

        photonView.RPC(
            "FireProjectile",
            RpcTarget.All,
            raycastPoint.position, // 1. 발사 위치
            transform.rotation, // 2. 발사 회전 (방향)
            targetPosition, // 3. 목표 위치 (Dead Reckoning의 방향 계산용)
            atkDamage, // 4. 데미지
            isprojectileTargeting, // 5. 타겟팅 여부
            isdummy // 6 .더미 여부 
        );
        if (isprojectileTargeting) //타겟팅 일때만 코루틴 실행 
        {
            StartCoroutine(HomingProjectileCoroutine(endtime)); //타겟팅일때 타겟의 위치를 업데이트 해주는 코루틴
        }

        // 공격 판정 후 상태를 추격 상태로 변경
        stateMachine.ChangeState(new EnemyChaseState());
        isAtk = false;
        mainAnim.SetBool("atk", false);
    }

    [PunRPC] //각 클라이언트에서 투사체를 쏘는 PRC
    public void FireProjectile(Vector3 startPos, Quaternion rotation, Vector3 targetPos, float damage, bool isTargeting,
        bool isdummy)
    {
        // RPC를 통해 모든 클라이언트가 로컬에서 투사체를 생성합니다.
        GameObject projectile = Instantiate(projectilePrefab, startPos, rotation);

        // Projectile 스크립트에 초기 Dead Reckoning에 필요한 정보를 전달합니다.
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            // Projectile 스크립트에 Initialize 메서드가 있다고 가정합니다.
            // 타겟 위치, 데미지, 타겟팅 여부 등 Dead Reckoning에 필요한 초기 데이터를 전달
            projectileScript.Initialize(targetPos, damage, isTargeting, isdummy);

            // 호밍 투사체면 리스트에 추가
            if (isTargeting)
            {
                _myProjectiles.Add(projectileScript);
            }
        }
    }

    private IEnumerator HomingProjectileCoroutine(float duration) //타겟 업데이트를 위한 코루틴 
    {
        float startTime = Time.time;
        float updateInterval = 0.2f; // 0.2초마다 업데이트
        float lastUpdateTime = 0f;

        while (Time.time < startTime + duration && target != null)
        {
            // 일정 간격으로만 RPC 호출 (성능 최적화)
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                // 모든 클라이언트에게 최신 타겟 위치 전달
                photonView.RPC("UpdateProjectileTarget", RpcTarget.All, target.position);
                lastUpdateTime = Time.time;
            }

            yield return null;
        }
    }

    [PunRPC]
    public void UpdateProjectileTarget(Vector3 newTargetPos)
    {
        // 내가 쏜 투사체만 업데이트
        for (int i = _myProjectiles.Count - 1; i >= 0; i--)
        {
            if (_myProjectiles[i] != null)
            {
                _myProjectiles[i].targetPosUpdate(newTargetPos);
            }
            else
            {
                // 파괴된 투사체 제거
                _myProjectiles.RemoveAt(i);
            }
        }
    }

    #endregion

    #region SkillState Methods

    public void TryUseSkills()
    {
        foreach (var skill in skills)
        {
            if (skill.IsReady())
            {
                StartAttackFlag(); // 공격 시작
                skill.Use(this);
                break; // 한 번에 하나만 사용
            }
        }
    }

    private void OnTriggerEnter(Collider other) //차지때만 활성화
    {
        if (other.CompareTag("Player"))
        {
            //todo : 플레이어에게 데미지 주는것 
            Debug.Log($"현재 차지 데미지 {chargeDamage}"); //차지 데미지를 플레이어에게 주어야합니다 
        }
    }
    
    [PunRPC] //Smash데칼 표시 및 시작지점으로 이동 
    public void ShowSmashDecal(Vector3 position, float baseSize)
    {
        if (decalProjector != null)
        {
            decalProjector.enabled = true;
            decalProjector.transform.position = position;
            decalProjector.size = new Vector3(0f, 0f, decalProjector.size.z);
        }
    }

    [PunRPC] //SmashRPC 데칼크기 업데이트
    public void UpdateSmashDecalSize(float currentSize)
    {
        if (decalProjector != null)
        {
            decalProjector.size = new Vector3(currentSize, currentSize, decalProjector.size.z);
        }
    }

    [PunRPC] //SmashRPC 데칼 숨기기 
    public void HideSmashDecal() //
    {
        if (decalProjector != null)
        {
            decalProjector.enabled = false;
        }
    }

    #endregion

    #region DizzyState Methods

    public void Dizzy() //dizzytime만큼 스턴 
    {
        if (isDizzy) return; // 스턴 중일 때는 중복 실행 방지

        // Dizzy 상태로 전환만 하고,
        // 실제 애니메이션 제어와 코루틴 시작은 EnemyDizzyState의 Enter()에서 합니다.
        stateMachine.ChangeState(new EnemyDizzyState());
    }

    public IEnumerator Dizzytimer()
    {
        // 1. 스턴 상태 진입 처리 (isDizzy 플래그를 코루틴이 관리)
        isDizzy = true;

        // NavMeshAgent가 이미 멈춰있지만, 명시적으로 멈춤을 유지
        agent.isStopped = true;
        agent.destination = transform.position;

        yield return new WaitForSeconds(dizzyTime); //dizzyTime후 스턴 해제
        mainAnim.SetBool("Dizzy", false);
        yield return new WaitForSeconds(2.15f); //포효하는것 기다리기 

        // 2. 스턴 상태 해제 처리
        isDizzy = false;
        agent.isStopped = false; // Agent 이동 재개 허용 (다음 상태에서 목적지 설정하면 이동)
        stateMachine.ChangeState(new EnemyChaseState());
    }

    #endregion

    #region Hit Methods

    [PunRPC]
    public void Hit(float damage)
    {
        //맞으면 1.5배 raycastRange 거리를 탐색하여 가장 가까운 플레이어를 타겟으로 지정 그리고 raycastrange거리가 일시적으로 1.5배로 증가

        currentHp -= damage; //데미지 적용 

        // 사망 처리
        if (currentHp <= 0)
        {
            isDead = true;
            mainAnim.SetBool("Die", true);
            agent.enabled = false; //상호작용을 막기위해 
            return;
        }

        // 스턴 중이고 공격중이지 않으며 사망하지않았으면 
        if (isDizzy || isAtk) return;

        if (isBoss)
        {
            // 현재 체력 비율을 계산합니다.
            float healthRatio = currentHp / maxHp;

            // 다음에 도달해야 할 스턴 단계를 확인합니다.
            int nextStunPhase = 0;
            if (healthRatio <= 0.25f)
            {
                nextStunPhase = 3; // 25% 구간
            }
            else if (healthRatio <= 0.50f)
            {
                nextStunPhase = 2; // 50% 구간
            }
            else if (healthRatio <= 0.75f)
            {
                nextStunPhase = 1; // 75% 구간
            }

            // 현재 스턴 구간보다 다음 스턴 구간이 더 깊어졌다면 (단 한 번만 조건 충족)
            if (nextStunPhase > currentStunPhase)
            {
                // 1. 스턴 구간을 업데이트합니다.
                currentStunPhase = nextStunPhase;
                Debug.Log("!! 체력 " + (currentStunPhase * 25) + "% 구간 진입! 스턴 발동 !!");
                stateMachine.ChangeState(new EnemyDizzyState());
                return;
            }
        }

        if (!isHitCor)
        {
            StartCoroutine(DetectTargetAfterHit()); //1.5배 range를 늘리고 5초후에 복구하는 코루틴 시작
            isHitCor = true;
        }
        mainAnim.SetTrigger("Hit");
        
        // ✅ 수정: 가장 가까운 플레이어 탐색
        Transform nearestPlayer = FindNearestPlayer();
        if (nearestPlayer != null)
        {
            target = nearestPlayer;
            float distance = Vector3.Distance(transform.position, nearestPlayer.position);
            Debug.Log($"맞은 후 가장 가까운 플레이어 추적: {nearestPlayer.name}, 거리: {distance}m");
            stateMachine.ChangeState(new EnemyChaseState());
        }
        else
        {
            stateMachine.ChangeState(new EnemyIdelState()); //못찻으면 기본 탐색으로 전환
        }
    }
    

    public Transform FindNearestPlayer() //맞은후 가장 가까운 플레이어 탐색 
    {
        Collider[] hits = Physics.OverlapSphere(raycastPoint.position, raycastRange, playerLayer);
    
        if (hits.Length == 0) return null;
    
        Transform nearestPlayer = null;
        float nearestDistance = Mathf.Infinity;
    
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
            
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPlayer = hit.transform;
                }
            }
        }
    
        return nearestPlayer;
    }

    IEnumerator DetectTargetAfterHit() //맞은후 raycastRange거리가 1.5배가 되고 5초후 정상적으로 변환
    {
        raycastRange = raycastRange * 1.5f;
        chaseRange = chaseRange * 1.5f;
        yield return new WaitForSeconds(5f);
        chaseRange = chaseRange / 1.5f;
        raycastRange = raycastRange / 1.5f;

        isHitCor = false;
    }


    private void Die() //death애니매이션 끝에 이벤트로 넣어주세요
    {
        if (PhotonNetwork.IsMasterClient) //마스터클라이언트에서만 Die 호출 
        {
            if (_spawner != null)
            {
                _spawner.OnObjectDestroyed(gameObject);
            }
            PhotonNetwork.Destroy(gameObject);
        }
    }

    //HIT의 애니매이션이 Trigger이라 넣은 함수 RPC로 불러와야해서 플레이어가 때렸을때 이 함수를 불러와야합니다 
    public void HitRPC(float damage)
    {
        photonView.RPC("Hit", RpcTarget.All, damage);
    }

    #endregion

    // Scene 뷰에서 Gizmos로 시야각 시각화
    private void OnDrawGizmosSelected()
    {
        if (raycastPoint == null) return;
        // 탐색 범위 
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(raycastPoint.position, raycastRange);

        // 정면 방향
        Vector3 forward = raycastPoint.forward * raycastRange;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(raycastPoint.position, forward);

        //공격 판정 기즈모 
        Gizmos.color = Color.green;
        Vector3 sphereCenter = transform.position + transform.forward * (atkRange / 2f); //공격판정 중심 
        float sphereRadius = atkRange / 2f; //공격범위
        Gizmos.DrawWireSphere(sphereCenter, sphereRadius);
        //공격 사거리 기즈모 
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(raycastPoint.position, raycastPoint.forward * atkRange);
    }

    public void SetSpawn(SpawnObject spawner) //자신을 소환한 스포너 알기위한 함수 
    {
        _spawner = spawner;
        if (_spawner != null)
            spawnerpos = _spawner.transform.position;
        else
            spawnerpos = transform.position; //스포너가 없으면 대체로 자기자신 넣기 
    }
}