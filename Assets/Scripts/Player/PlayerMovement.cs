using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Transform used for checking ground collision.")]
        [SerializeField] private Transform groundCheck;

        [Header("Movement")]
        [Tooltip("Speed when walking.")]
        [SerializeField, Range(1f, 10f)] private float walkSpeed = 2.2f;
        [Tooltip("Speed when sprinting.")]
        [SerializeField, Range(1f, 20f)] private float sprintSpeed = 3.4f;
        [Tooltip("How fast the player reaches max speed.")]
        [SerializeField, Range(1f, 50f)] private float acceleration = 10f;
        [Tooltip("How fast the player stops.")]
        [SerializeField, Range(1f, 50f)] private float deceleration = 14f;

        [Header("Physics")]
        [Tooltip("Force of gravity.")]
        [SerializeField, Range(-20f, -1f)] private float gravity = -9.81f;
        [Tooltip("Keeps player snapped to floor when moving down slopes.")]
        [SerializeField, Range(-5f, 0f)] private float groundedStickForce = -2f;
        [Tooltip("Radius of the ground detection sphere.")]
        [SerializeField, Range(0.1f, 1f)] private float groundRadius = 0.35f;
        [Tooltip("Layers recognized as ground.")]
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
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            CheckGround();
            ApplyGravity(dt);
            Move(dt);
        }

        private void CheckGround()
        {
            isGrounded = groundCheck != null
                ? Physics.CheckSphere(groundCheck.position, groundRadius, groundMask, QueryTriggerInteraction.Ignore) 
                : controller.isGrounded;
        }

        private void ApplyGravity(float dt)
        {
            if (isGrounded && verticalVelocity < 0f) verticalVelocity = groundedStickForce;
            else verticalVelocity += gravity * dt;
        }

        private void Move(float dt)
        {
            if (inputReader == null) return;
            Vector2 moveInput = inputReader.MoveInput;
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
            float targetSpeed = inputReader.IsSprinting ? sprintSpeed : walkSpeed;
            Vector3 camFwd = cameraTransform ? cameraTransform.forward : t.forward;
            Vector3 camRight = cameraTransform ? cameraTransform.right : t.right;
            camFwd.y = 0f; camRight.y = 0f;
            camFwd.Normalize(); camRight.Normalize();
            Vector3 desiredMove = (camRight * moveInput.x + camFwd * moveInput.y) * targetSpeed;
            float rate = (desiredMove.sqrMagnitude > 0.0001f) ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredMove, rate * dt);
            Vector3 finalMotion = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(finalMotion * dt);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}