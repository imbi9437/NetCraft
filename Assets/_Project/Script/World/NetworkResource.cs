using _Project.Script.World;
using _Project.Script.Interface;
using Photon.Pun;
using UnityEngine;

namespace _Project.Script.World
{
    /// <summary>
    /// 네트워크 리소스 프리팹 전용 스크립트
    /// NetworkWorldManager와 연동하여 리소스 동기화
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class NetworkResource : MonoBehaviour, IPunObservable, IInteractable
    {
        [Header("리소스 설정")]
        [SerializeField] private ResourceType resourceType = ResourceType.Wood;
        [SerializeField] private int maxAmount = 100;
        [SerializeField] private int currentAmount = 100;
        [SerializeField] private bool isDepleted = false;
        [SerializeField] private float respawnTime = 300f; // 5분 후 재생성

        [Header("상호작용 설정")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private string interactionText = "리소스";
        [SerializeField] private int harvestAmount = 10; // 한 번에 채집하는 양

        [Header("프리팹 전용 설정")]
        [SerializeField] private string resourceName = "Resource";
        [SerializeField] private bool autoHarvest = false;
        [SerializeField] private float harvestDelay = 1f;

        // 네트워크 동기화용
        private PhotonView photonView;
        private NetworkWorldManager worldManager;

        // 상호작용 관련
        private IInteractor currentInteractor;

        // 재생성 관련
        private float lastHarvestTime = 0f;
        private bool isRespawning = false;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
            worldManager = NetworkWorldManager.Instance;
        }

        private void Start()
        {
            // 리소스 정보를 월드 매니저에 등록
            if (worldManager != null && photonView.IsMine)
            {
                RegisterToWorldManager();
            }
        }

        private void Update()
        {
            // 재생성 처리
            if (isDepleted && !isRespawning && Time.time - lastHarvestTime >= respawnTime)
            {
                RespawnResource();
            }
        }

        private void OnDestroy()
        {
            // 리소스 정보를 월드 매니저에서 제거
            if (worldManager != null && photonView.IsMine)
            {
                UnregisterFromWorldManager();
            }
        }

        #region PUN2 네트워크 동기화

        /// <summary>
        /// PUN2 네트워크 동기화 (리소스 데이터)
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터 전송 (로컬 플레이어)
                stream.SendNext(currentAmount);
                stream.SendNext(isDepleted);
                stream.SendNext(isRespawning);
            }
            else
            {
                // 데이터 수신 (원격 플레이어)
                currentAmount = (int)stream.ReceiveNext();
                isDepleted = (bool)stream.ReceiveNext();
                isRespawning = (bool)stream.ReceiveNext();

                // 리소스 상태 업데이트
                UpdateResourceVisual();
            }
        }

        #endregion

        #region 월드 매니저 연동

        /// <summary>
        /// 월드 매니저에 리소스 등록
        /// </summary>
        private void RegisterToWorldManager()
        {
            if (worldManager != null)
            {
                // 리소스 재생성 이벤트 발생
                worldManager.RegenerateResource(
                    new Vector3Int(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z)),
                    resourceType, currentAmount);
            }
        }

        /// <summary>
        /// 월드 매니저에서 리소스 제거
        /// </summary>
        private void UnregisterFromWorldManager()
        {
            if (worldManager != null)
            {
                // 리소스 고갈 이벤트 발생
                worldManager.HarvestResource(
                    new Vector3Int(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z)),
                    currentAmount);
            }
        }

        #endregion

        #region 리소스 관리

        /// <summary>
        /// 리소스 채집
        /// </summary>
        public void HarvestResource(int amount, int harvesterActorNumber)
        {
            if (isDepleted || isRespawning) return;

            int actualAmount = Mathf.Min(amount, currentAmount);
            currentAmount -= actualAmount;
            lastHarvestTime = Time.time;

            if (currentAmount <= 0)
            {
                isDepleted = true;
                DepleteResource();
            }

            Debug.Log($"[NetworkResource] 리소스 채집: {actualAmount}, 남은 양: {currentAmount}");
        }

        /// <summary>
        /// 리소스 고갈 처리
        /// </summary>
        private void DepleteResource()
        {
            isDepleted = true;
            isRespawning = true;
            lastHarvestTime = Time.time;

            // 고갈 이펙트 (나중에 추가)
            // Instantiate(depleteEffect, transform.position, transform.rotation);

            // 리소스 비활성화
            gameObject.SetActive(false);

            Debug.Log($"[NetworkResource] 리소스 고갈: {resourceType}");
        }

        /// <summary>
        /// 리소스 재생성
        /// </summary>
        private void RespawnResource()
        {
            if (!isDepleted) return;

            currentAmount = maxAmount;
            isDepleted = false;
            isRespawning = false;

            // 재생성 이펙트 (나중에 추가)
            // Instantiate(respawnEffect, transform.position, transform.rotation);

            // 리소스 활성화
            gameObject.SetActive(true);

            Debug.Log($"[NetworkResource] 리소스 재생성: {resourceType}");
        }

        /// <summary>
        /// 리소스 시각적 업데이트
        /// </summary>
        private void UpdateResourceVisual()
        {
            // 리소스 양에 따른 시각적 변화 (나중에 추가)
            // 예: 양이 적으면 크기 축소, 고갈되면 투명도 변경 등
        }

        #endregion

        #region IInteractable 인터페이스 구현

        /// <summary>
        /// 호버 진입
        /// </summary>
        public void HoveredEnter(IInteractor interactor)
        {
            currentInteractor = interactor;
            Debug.Log($"[NetworkResource] 리소스 호버 진입: {resourceType}");
        }

        /// <summary>
        /// 상호작용 실행 (채집)
        /// </summary>
        public void Interact(IInteractor interactor)
        {
            if (isDepleted || isRespawning) return;

            // 채집 실행 (ActorNumber는 -1로 설정)
            HarvestResource(harvestAmount, -1);

            Debug.Log($"[NetworkResource] 리소스 채집: {resourceType} x{harvestAmount}");
        }

        /// <summary>
        /// 호버 종료
        /// </summary>
        public void HoveredExit(IInteractor interactor)
        {
            currentInteractor = null;
            Debug.Log($"[NetworkResource] 리소스 호버 종료: {resourceType}");
        }

        #endregion

        #region 디버그

        private void OnDrawGizmosSelected()
        {
            // 상호작용 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            // 리소스 상태 표시
            if (isDepleted)
            {
                Gizmos.color = Color.red;
            }
            else if (isRespawning)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }

        #endregion
    }
}
