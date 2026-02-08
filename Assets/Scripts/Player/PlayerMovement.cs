using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Provides movement/sprint input values.")]
    [SerializeField] private InputReader inputReader;

    [Tooltip("Camera used to convert input into camera-relative movement (usually the FPS camera).")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Optional ground check transform. If not assigned, CharacterController.isGrounded is used.")]
    [SerializeField] private Transform groundCheck;

    [Header("Speed")]
    [Tooltip("Base walking speed in meters/second.")]
    [SerializeField, Min(0f)] private float walkSpeed = 2.2f;

    [Tooltip("Sprint speed in meters/second (requires InputReader.IsSprinting).")]
    [SerializeField, Min(0f)] private float sprintSpeed = 3.4f;

    [Header("Acceleration")]
    [Tooltip("How quickly the player reaches the target speed when input is held.")]
    [SerializeField, Min(0f)] private float acceleration = 10f;

    [Tooltip("How quickly the player slows down when input is released.")]
    [SerializeField, Min(0f)] private float deceleration = 14f;

    [Header("Gravity")]
    [Tooltip("Gravity acceleration (keep negative).")]
    [SerializeField] private float gravity = -9.81f;

    [Tooltip("Small downward force when grounded to keep the controller snapped to the floor.")]
    [SerializeField] private float groundedStickForce = -2f;

    [Header("Ground Check")]
    [Tooltip("Radius of the ground check sphere.")]
    [SerializeField, Min(0f)] private float groundRadius = 0.35f;

    [Tooltip("Layers considered as ground for the sphere check.")]
    [SerializeField] private LayerMask groundMask;

    private CharacterController controller;
    private Transform t;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        t = transform;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        isGrounded = groundCheck != null
            ? Physics.CheckSphere(groundCheck.position, groundRadius, groundMask, QueryTriggerInteraction.Ignore)
            : controller.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStickForce;

        Vector2 moveInput = inputReader != null ? inputReader.MoveInput : Vector2.zero;
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        bool sprint = inputReader != null && inputReader.IsSprinting;
        float targetSpeed = sprint ? sprintSpeed : walkSpeed;

        Vector3 camFwd = cameraTransform ? cameraTransform.forward : t.forward;
        Vector3 camRight = cameraTransform ? cameraTransform.right : t.right;
        camFwd.y = 0f; camRight.y = 0f;
        camFwd.Normalize(); camRight.Normalize();

        Vector3 desiredMove = (camRight * moveInput.x + camFwd * moveInput.y) * targetSpeed;

        float rate = (desiredMove.sqrMagnitude > 0.0001f) ? acceleration : deceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredMove, rate * dt);

        verticalVelocity += gravity * dt;

        Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(motion * dt);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
