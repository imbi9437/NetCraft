using _Project.Script.Interface;
using Photon.Pun;
using UnityEngine;

namespace _Project.Script.Items.Network
{
    /// <summary>
    /// 네트워크 월드 아이템 프리팹 전용 스크립트
    /// 바닥에 떨어진 아이템을 관리하고 상호작용 처리
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Collider))]
    public class NetworkWorldItem : MonoBehaviour, IPunObservable, IInteractable
    {
        [Header("아이템 설정")]
        [SerializeField] private ItemInstance itemInstance;
        [SerializeField] private int itemUID = 0;
        [SerializeField] private int count = 1;
        [SerializeField] private Vector3 dropPosition;
        [SerializeField] private int dropperActorNumber = -1;

        [Header("상호작용 설정")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private string interactionText = "아이템";

        [Header("프리팹 전용 설정")]
        [SerializeField] private string itemName = "Item";
        [SerializeField] private bool autoPickup = true;
        [SerializeField] private float pickupDelay = 0.5f;

        // 네트워크 동기화용
        private PhotonView photonView;
        private NetworkItemManager itemManager;

        // 상호작용 관련
        private IInteractor currentInteractor;

        // 시각적 요소
        private Renderer itemRenderer;
        private Collider itemCollider;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
            itemManager = NetworkItemManager.Instance;
            itemRenderer = GetComponent<Renderer>();
            itemCollider = GetComponent<Collider>();

            // 트리거로 설정
            if (itemCollider != null)
            {
                itemCollider.isTrigger = true;
            }
        }

        private void Start()
        {
            // 아이템 시각적 설정
            SetupItemVisual();
        }

        #region PUN2 네트워크 동기화

        /// <summary>
        /// PUN2 네트워크 동기화 (아이템 데이터)
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터 전송 (로컬 플레이어)
                stream.SendNext(itemUID);
                stream.SendNext(count);
                stream.SendNext(dropperActorNumber);
            }
            else
            {
                // 데이터 수신 (원격 플레이어)
                itemUID = (int)stream.ReceiveNext();
                count = (int)stream.ReceiveNext();
                dropperActorNumber = (int)stream.ReceiveNext();

                // 아이템 데이터 업데이트
                UpdateItemData();
            }
        }

        #endregion

        #region 아이템 관리

        /// <summary>
        /// 아이템 데이터 설정 (RPC)
        /// </summary>
        [PunRPC]
        public void SetItemDataRPC(int uid, int itemCount, int dropper)
        {
            itemUID = uid;
            count = itemCount;
            dropperActorNumber = dropper;

            // 아이템 인스턴스 생성
            itemInstance = new ItemInstance
            {
                itemData = new ItemData { uid = uid },
                count = itemCount
            };

            UpdateItemVisual();
            Debug.Log($"[NetworkWorldItem] 아이템 데이터 설정: UID {uid}, 수량 {itemCount}");
        }

        /// <summary>
        /// 아이템 데이터 업데이트
        /// </summary>
        private void UpdateItemData()
        {
            if (itemUID > 0)
            {
                itemInstance = new ItemInstance
                {
                    itemData = new ItemData { uid = itemUID },
                    count = count
                };
            }

            UpdateItemVisual();
        }

        /// <summary>
        /// 아이템 시각적 설정
        /// </summary>
        private void SetupItemVisual()
        {
            // 아이템 모델 설정 (나중에 구현)
            // 예: 아이템 타입에 따른 모델 로드
        }

        /// <summary>
        /// 아이템 시각적 업데이트
        /// </summary>
        private void UpdateItemVisual()
        {
            // 아이템 수량에 따른 시각적 변화 (나중에 추가)
            // 예: 수량이 많으면 크기 증가, 스택 표시 등
        }

        #endregion

        #region IInteractable 인터페이스 구현

        /// <summary>
        /// 호버 진입
        /// </summary>
        public void HoveredEnter(IInteractor interactor)
        {
            currentInteractor = interactor;
            Debug.Log($"[NetworkWorldItem] 아이템 호버 진입: UID {itemUID}");
        }

        /// <summary>
        /// 상호작용 실행 (픽업)
        /// </summary>
        public void Interact(IInteractor interactor)
        {
            if (itemUID <= 0) return;

            // 픽업 실행
            PickupItem(interactor);

            Debug.Log($"[NetworkWorldItem] 아이템 픽업: UID {itemUID}, 수량 {count}");
        }

        /// <summary>
        /// 호버 종료
        /// </summary>
        public void HoveredExit(IInteractor interactor)
        {
            currentInteractor = null;
            Debug.Log($"[NetworkWorldItem] 아이템 호버 종료: UID {itemUID}");
        }

        #endregion

        #region 아이템 픽업

        /// <summary>
        /// 아이템 픽업 처리
        /// </summary>
        private void PickupItem(IInteractor interactor)
        {
            if (itemManager == null) return;

            // 플레이어로 캐스팅
            var player = interactor as _Project.Script.Character.Player.PlayerStateMachine;
            if (player == null) return;

            // 아이템 매니저를 통해 픽업 처리
            itemManager.PickupItem(itemUID, count);

            // 픽업 시 아이템 제거
            DestroyWorldItem();
        }

        /// <summary>
        /// 월드 아이템 제거
        /// </summary>
        private void DestroyWorldItem()
        {
            // 픽업 이펙트 (나중에 추가)
            // Instantiate(pickupEffect, transform.position, transform.rotation);

            // 아이템 제거
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        #endregion

        // #region 트리거 처리

        // /// <summary>
        // /// 트리거 진입 (자동 픽업)
        // /// </summary>
        // private void OnTriggerEnter(Collider other)
        // {
        //     if (itemUID <= 0) return;

        //     // 플레이어 확인
        //     var player = other.GetComponent<_Project.Script.Character.Player.PlayerStateMachine>();
        //     if (player == null) return;

        //     // 자동 픽업 실행
        //     PickupItem(player);
        // }

        // #endregion

        #region 공개 API
        // 사용하지 않는 getter 함수들 제거됨
        #endregion

        #region 디버그

        private void OnDrawGizmosSelected()
        {
            // 상호작용 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            // 아이템 상태 표시
            Gizmos.color = (itemUID > 0 && count > 0) ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }

        #endregion
    }
}