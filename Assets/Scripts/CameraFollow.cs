using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Horizontal Follow")]
    [SerializeField] private float horizontalSmoothSpeed = 6f;

    [Header("Vertical Follow")]
    [SerializeField] private float upperDeadZone = 2.5f;
    [SerializeField] private float lowerDeadZone = 1.2f;
    [SerializeField] private float verticalSmoothSpeed = 3f;

    [Header("Falling Follow")]
    [SerializeField] private float fallSpeedThreshold = -3f;
    [SerializeField] private float fallFollowSpeed = 10f;
    [SerializeField] private float fallOffsetY = 0.5f;

    [Header("Look Down")]
    [SerializeField] private float lookDownAmount = 2f;
    [SerializeField] private float lookDownSmoothSpeed = 5f;

    [Header("Offset")]
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 1f;

    private float cameraY;
    private float currentLookOffset;

    private Rigidbody2D targetRb;
    private PlayerController playerController;

    private bool isFallFollowing;

    private void Awake()
    {
        cameraY = transform.position.y;
    }

    private void Start()
    {
        if (target != null)
        {
            targetRb =
                target.GetComponent<Rigidbody2D>();

            playerController =
                target.GetComponent<PlayerController>();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // -------------------------
        // 横向平滑跟随
        // -------------------------
        float targetX =
            target.position.x + offsetX;

        float newX =
            Mathf.Lerp(
                transform.position.x,
                targetX,
                horizontalSmoothSpeed * Time.deltaTime
            );

        // -------------------------
        // 是否正在向下看
        // -------------------------
        bool lookingDown =
            Keyboard.current != null &&
            (Keyboard.current.sKey.isPressed ||
             Keyboard.current.downArrowKey.isPressed);

        // -------------------------
        // 判断是否进入坠落追踪
        // -------------------------
        if (targetRb != null &&
            targetRb.linearVelocity.y < fallSpeedThreshold)
        {
            isFallFollowing = true;
        }

        // 落地后退出坠落追踪
        if (playerController != null &&
            playerController.IsGrounded)
        {
            isFallFollowing = false;
        }

        // -------------------------
        // Y 轴控制
        // -------------------------
        if (!lookingDown)
        {
            if (isFallFollowing)
            {
                // 坠落时快速向下追角色
                float fallTargetY =
                    target.position.y +
                    fallOffsetY;

                cameraY =
                    Mathf.Lerp(
                        cameraY,
                        fallTargetY,
                        fallFollowSpeed * Time.deltaTime
                    );
            }
            else
            {
                // 正常状态使用 Dead Zone
                float desiredY =
                    target.position.y + offsetY;

                float differenceY =
                    desiredY - cameraY;

                // Player 在镜头上方
                if (differenceY > upperDeadZone)
                {
                    float targetY =
                        desiredY - upperDeadZone;

                    cameraY =
                        Mathf.Lerp(
                            cameraY,
                            targetY,
                            verticalSmoothSpeed * Time.deltaTime
                        );
                }

                // Player 在镜头下方
                else if (differenceY < -lowerDeadZone)
                {
                    float targetY =
                        desiredY + lowerDeadZone;

                    cameraY =
                        Mathf.Lerp(
                            cameraY,
                            targetY,
                            verticalSmoothSpeed * Time.deltaTime
                        );
                }
            }
        }

        // -------------------------
        // 向下看
        // -------------------------
        float targetLookOffset =
            lookingDown
                ? -lookDownAmount
                : 0f;

        currentLookOffset =
            Mathf.Lerp(
                currentLookOffset,
                targetLookOffset,
                lookDownSmoothSpeed * Time.deltaTime
            );

        // -------------------------
        // 最终 Camera 位置
        // -------------------------
        transform.position =
            new Vector3(
                newX,
                cameraY + currentLookOffset,
                -10f
            );
    }
}