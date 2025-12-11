using _Project.Script.Interface;
using UnityEngine;

public class PlayerTest : MonoBehaviour ,IHitAble
{
    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 destination;
    private bool isMoving = false;
    public GameObject target;

    public float moveSpeed = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        destination = transform.position; // 초기 목적지를 현재 위치로 설정
    }

    private void Update()
    {
        // 마우스 왼쪽 버튼을 클릭했을 때
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                destination = hit.point;
                isMoving = true;
            }
        }

        // 목적지에 거의 도달했는지 확인
        if (isMoving && Vector3.Distance(transform.position, destination) < 0.5f)
        {
            isMoving = false;
            rb.velocity = Vector3.zero; // 이동 중지
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // target.GetComponent<SpawnObject>().Hit(100);
            target.GetComponent<Enemy>().HitRPC(100);
        }
    }

    private void FixedUpdate()
    {
        // FixedUpdate는 물리 계산에 사용되는 함수
        if (isMoving)
        {
            Vector3 direction = (destination - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
    }

    public void Hit(float damage)
    {
        Debug.Log($"데미지를 입었습니다 : {damage}");
    }
}