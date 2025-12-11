using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private PhotonView photonView;
    private Vector3 playerVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        // 이 캐릭터가 내 소유일 경우에만 입력을 받아 움직입니다.
        // 이렇게 하지 않으면 모든 플레이어가 내 키보드 입력으로 같이 움직이게 됩니다.
        if (photonView.IsMine)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        // 캐릭터가 땅에 닿아있으면 수직 속도를 초기화 (계속해서 아래로 떨어지는 것을 방지)
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // 키보드 입력 받기 (WASD 또는 방향키)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 중력 적용
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}