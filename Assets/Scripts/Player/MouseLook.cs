using UnityEngine;

namespace Player
{
    public class MouseLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [Tooltip("The body that will rotate on the Y axis.")]
        [SerializeField] private Transform playerBody;

        [Header("Settings")]
        [Tooltip("Mouse sensitivity speed.")]
        [SerializeField, Range(10f, 1000f)] private float mouseSensitivity = 100f;

        private float xRotation = 0f;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (inputReader == null) return;
            Vector2 lookInput = inputReader.LookInput * mouseSensitivity * Time.deltaTime;
            xRotation -= lookInput.y;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            if (playerBody != null) playerBody.Rotate(Vector3.up * lookInput.x);
        }
    }
}