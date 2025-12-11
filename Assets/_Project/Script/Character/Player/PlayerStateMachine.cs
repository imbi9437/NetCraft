using System;
using UnityEngine;
using _Project.Script.StateMachine.Mono;
using _Project.Script.Interface;
using _Project.Script.Data;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Generic;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;

namespace _Project.Script.Character.Player
{
    [RequireComponent(typeof(CharacterController),typeof(Animator))]
    public class PlayerStateMachine : MonoStateMachine, IHitAble, IInteractor
    {
        public static readonly int IsMove = Animator.StringToHash("IsMove");
        public static readonly int IsAttack = Animator.StringToHash("IsAttack");
        public static readonly int IsInteract = Animator.StringToHash("IsInteract");
        public static readonly int IsHit = Animator.StringToHash("IsHit");
        public static readonly int IsDie = Animator.StringToHash("IsDie");

        [Header("컴포넌트 참조")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator animator;
        [SerializeField] private PhotonView photonView;
        [SerializeField] private PlayerCameraController cameraController;
        [SerializeField] private PlayerNetworkHandler handler;

        [Header("이동 설정")]
        [SerializeField] private float groundCheckDistance = 0.2f;

        // 이동 상태
        private Vector3 moveDirection;
        private Vector2 inputVector;
        public bool isMoving = false;
        public bool isOtherAction = false;

        // 물리 상태
        private Vector3 velocity;
        private bool isGrounded;

        private Collider[] cols = new Collider[1];
        private Collider[] attackCols = new Collider[3];

        // 기존 코드 호환성을 위한 프로퍼티들
        public Animator Animator => animator;
        public PhotonView PhotonView => photonView;

        public IInteractable HoveredInteractable { get; set; }

        protected override void Awake()
        {
            base.Awake();

            characterController ??= GetComponent<CharacterController>();
            animator ??= GetComponent<Animator>();
            photonView ??= GetComponent<PhotonView>();
            cameraController ??= GetComponent<PlayerCameraController>();
            handler ??= GetComponent<PlayerNetworkHandler>();
        }

        private void Start()
        {
            if (photonView != null && photonView.IsMine == false) return;

            cameraController.Initialize(photonView.IsMine);

            RegisterEvents();
        }

        private float delay = 0f;
        
        private void Update()
        {
            if (photonView != null && photonView.IsMine == false) return;

            CheckGrounded();
            ApplyGravity();

            if (isMoving) UpdateMovement();
            
            FindInteractable();

            if (DataManager.Instance.localPlayerData != null && PhotonView.IsMine)
            {
                PlayerData.SetPosition(DataManager.Instance.localPlayerData, transform.position);
                PlayerData.SetRotation(DataManager.Instance.localPlayerData, transform.rotation);
            }

            if (delay <= 3f)
            {
                delay += Time.deltaTime;
                return;
            }
            delay = 0f;
            if (PhotonNetwork.IsMasterClient == false && PhotonNetwork.InRoom)
            {
                

                

                string uid = DataManager.Instance.localUserData.uid;
                string json = JsonConvert.SerializeObject(DataManager.Instance.localPlayerData);
                photonView.RPC(nameof(SavePlayerData), RpcTarget.MasterClient, uid, json);
            }
            else if (PhotonNetwork.IsMasterClient) DataManager.Instance.SaveUserData();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }


        private void RegisterEvents()
        {
            EventHub.Instance?.RegisterEvent<InputEvents.ScrollInputEvent>(CameraZoomInputEvent);
            EventHub.Instance?.RegisterEvent<InputEvents.KeyInputEvent>(CameraRotateInputEvent);

            EventHub.Instance?.RegisterEvent<InputEvents.MoveInputEvent>(MoveInputEvent);
            
            EventHub.Instance?.RegisterEvent<InputEvents.InteractInputEvent>(InteractInputEvent);
            EventHub.Instance?.RegisterEvent<InputEvents.AttackInputEvent>(AttackInputEvent);
        }
        private void UnregisterEvents()
        {
            EventHub.Instance?.UnregisterEvent<InputEvents.ScrollInputEvent>(CameraZoomInputEvent);
            EventHub.Instance?.UnregisterEvent<InputEvents.KeyInputEvent>(CameraRotateInputEvent);

            EventHub.Instance?.UnregisterEvent<InputEvents.MoveInputEvent>(MoveInputEvent);
            
            EventHub.Instance?.UnregisterEvent<InputEvents.InteractInputEvent>(InteractInputEvent);
            EventHub.Instance?.UnregisterEvent<InputEvents.AttackInputEvent>(AttackInputEvent);
        }
        
        public void ChangeState(PlayerStateType type) => ChangeState((int)type);

        
        #region 이벤트 래퍼 함수


        private void CameraZoomInputEvent(InputEvents.ScrollInputEvent evt) => cameraController.ZoomCamera(evt.isDown);
        private void CameraRotateInputEvent(InputEvents.KeyInputEvent evt)
        {
            if (evt.key != Key.Q && evt.key != Key.E) return;

            cameraController.RotateCamera(evt.key != Key.Q);
        }


        private void MoveInputEvent(InputEvents.MoveInputEvent evt) => HandleMoveInput(evt.value);
        
        private void InteractInputEvent(InputEvents.InteractInputEvent evt) => Interaction();
        private void AttackInputEvent(InputEvents.AttackInputEvent evt) => Attack();


        #endregion


        #region 이동 관련 함수


        /// <summary> 이동 입력 처리 - 최적화됨 </summary>
        private void HandleMoveInput(Vector2 input)
        {
            inputVector = input;
            isMoving = input.magnitude > 0.1f && isOtherAction == false;
        }

        /// <summary> 이동 업데이트 </summary>
        private void UpdateMovement()
        {
            if (inputVector.magnitude > 0.1f)
            {
                // 카메라 기준 이동 방향 계산 (돈스타브 스타일)
                Vector3 inputDir = new Vector3(inputVector.x, 0, inputVector.y).normalized;

                if (cameraController != null)
                {
                    // 카메라 회전을 적용하여 카메라가 바라보는 방향 기준으로 이동
                    Quaternion camRot = Quaternion.Euler(0, cameraController.CurrentRotation, 0);
                    moveDirection = camRot * inputDir;
                }
                else
                {
                    moveDirection = inputDir;
                }

                // 캐릭터를 이동 방향으로 회전
                RotateTowardsMovement(moveDirection);

                // 실제 이동 실행
                MovePlayer();
            }
        }

        /// <summary> 이동 방향으로 캐릭터 회전 (부드럽게) </summary>
        private void RotateTowardsMovement(Vector3 direction)
        {
            if (direction.magnitude < 0.1f) return;

            // 목표 회전 계산
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 부드러운 회전 (Slerp)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        /// <summary> 바닥 감지 (Raycast 기반) </summary>
        private void CheckGrounded()
        {
            if (characterController == null) return;

            // 발 위치를 기준으로 아래로 Ray 쏘기
            Vector3 origin = transform.position + Vector3.up * 0.1f; // 살짝 위에서 쏘기
            float rayLength = 0.5f; // 충분한 길이
            int groundMask = LayerMask.GetMask("Ground"); // Ground Layer 확인

            // Raycast로 바닥 감지
            bool hit = Physics.Raycast(origin, Vector3.down, rayLength, groundMask);

            // CharacterController의 isGrounded도 확인
            bool controllerGrounded = characterController.isGrounded;

            // 둘 중 하나라도 바닥에 닿으면 바닥에 있는 것으로 판단
            isGrounded = hit || controllerGrounded;

            // 바닥에 닿았을 때 velocity.y 리셋
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // 바닥에 붙이기
            }
        }

        /// <summary> 중력 적용 </summary>
        private void ApplyGravity()
        {
            if (characterController == null) return;

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // 바닥에 닿았을 때 작은 음수 값
            }
            else
            {
                velocity.y += Physics.gravity.y * Time.deltaTime;

                // 최대 낙하 속도 제한 (무한 낙하 방지)
                velocity.y = Mathf.Max(velocity.y, -50f);
            }
        }

        /// <summary> 플레이어 이동 (로컬 플레이어만) </summary>
        private void MovePlayer()
        {
            // 로컬 플레이어만 물리 이동 수행
            if (photonView != null && photonView.IsMine && characterController != null)
            {
                // 수평 이동
                Vector3 horizontalMovement = moveDirection * (handler.PlayerData.speed * Time.deltaTime);

                // 수직 이동 (중력 포함)
                Vector3 verticalMovement = velocity * Time.deltaTime;

                // 전체 이동 벡터
                Vector3 totalMovement = horizontalMovement + verticalMovement;

                characterController.Move(totalMovement);
            }
        }


        #endregion

        private void Attack() => ChangeState(PlayerStateType.Attack);
        public void Hit(float damage)
        {
            //TODO : 데미지 입기 구현
            ChangeState(PlayerStateType.Hit);
        }

        public void Interaction()
        {
            if (HoveredInteractable == null) return;
            ChangeState(PlayerStateType.Interact);
        }

        public void FindInteractable()
        {
            int layer = 1 << LayerMask.NameToLayer("Interactable");
            int count = Physics.OverlapSphereNonAlloc(transform.position, 3f, cols, layer);
            if (count <= 0) return;
            if (cols[0].TryGetComponent(out IInteractable interactable) == false) return;
            
            HoveredInteractable?.HoveredExit(this);
            HoveredInteractable = interactable;
            HoveredInteractable?.HoveredEnter(this);
        }
        
        
        [PunRPC]
        public void PlayAttackAnimationRPC(PhotonMessageInfo info)
        {
            animator.SetTrigger(IsAttack);
        }

        [PunRPC]
        public void PerformAttackRPC(PhotonMessageInfo info)
        {
            Debug.Log("attack");
            PerformAttack();
        }
        
        public void PerformAttack()
        {
            int layer = 1 << LayerMask.NameToLayer("Enemy");
            var size = Physics.OverlapSphereNonAlloc(transform.position, 5f, attackCols, layer);
            if (size <= 0) return;

            for (int i = 0; i < size; i++)
            {
                if (attackCols[i].TryGetComponent(out IHitAble hitAble) == false) continue;
                if (PhotonNetwork.IsMasterClient == false) continue;
                
                //TODO : NetworkHandler의 데미지로 수정
                hitAble.Hit(handler.PlayerData.attack);
            }
        }
        
        [PunRPC]
        public void PlayInteractAnimationRPC(PhotonMessageInfo info)
        {
            animator.SetTrigger(PlayerStateMachine.IsInteract);
        }

        [PunRPC]
        public void PerformInteractRPC(PhotonMessageInfo info)
        {
            PerformInteract();
        }

        [PunRPC]
        public void SavePlayerData(string uid,string json, PhotonMessageInfo info)
        {
            PlayerData data = JsonConvert.DeserializeObject<PlayerData>(json);
            DataManager.Instance.localUserData.worldData.playerData[uid] = data;
            DataManager.Instance.SaveUserData();
        }
        
        
        public void PerformInteract()
        {
            if (PhotonNetwork.IsMasterClient == false) return;
            HoveredInteractable.Interact(this);
        }
    }
}