using DG.Tweening;
using UnityEngine;
using Interaction;
using Systems.Store;

namespace Systems.Items
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class PickupableItem : MonoBehaviour, IPickupable
    {
        [Header("Data")]
        [Tooltip("The ScriptableObject containing all product information.")]
        [SerializeField] private ItemData itemData;

        [Header("UI")]
        [Tooltip("Arabic prompt shown when looking at the item.")]
        [SerializeField] private string prompt = "???? E ?????";

        [Header("Settings")]
        [Tooltip("Should colliders be disabled while held to avoid physics glitches?")]
        [SerializeField] private bool disableCollidersWhileHeld = true;

        [Header("Hold Offsets")]
        [Tooltip("Local position offset relative to the hand.")]
        [SerializeField] private Vector3 localHoldPositionOffset = Vector3.zero;
        [Tooltip("Local rotation offset relative to the hand.")]
        [SerializeField] private Vector3 localHoldEulerOffset = Vector3.zero;

        [Header("Animations")]
        [Tooltip("How long the pickup animation takes.")]
        [SerializeField, Range(0.05f, 1f)] private float pickupDuration = 0.2f;
        [Tooltip("The animation curve type.")]
        [SerializeField] private Ease pickupEase = Ease.OutBack;

        private Rigidbody rb;
        private Collider[] cols;
        private Transform holdPoint;
        private bool held;

        public ItemData Data => itemData;
        public string Prompt => prompt;
        public bool IsHeld => held;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            cols = GetComponentsInChildren<Collider>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public bool CanInteract(GameObject interactor) => !held;
        public void Interact(GameObject interactor) { }

        public void Pickup(Transform newHoldPoint, GameObject interactor)
        {
            // Notify shelf if we were on one
            Shelf shelf = GetComponentInParent<Shelf>();
            if (shelf != null)
            {
                shelf.ReleaseItem(this);
            }

            transform.DOKill();
            held = true;
            holdPoint = newHoldPoint;
            if (disableCollidersWhileHeld) ToggleColliders(false);
            rb.interpolation = RigidbodyInterpolation.None;
            rb.useGravity = false;
            rb.isKinematic = true;
            transform.SetParent(holdPoint, true);
            transform.DOLocalMove(localHoldPositionOffset, pickupDuration).SetEase(pickupEase);
            transform.DOLocalRotate(localHoldEulerOffset, pickupDuration).SetEase(pickupEase);
        }

        public void Drop(Vector3 throwVelocity)
        {
            if (!held) return;
            transform.DOKill();
            held = false;
            transform.SetParent(null, true);
            if (disableCollidersWhileHeld) ToggleColliders(true);
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            if (throwVelocity != Vector3.zero) rb.linearVelocity = throwVelocity;
            holdPoint = null;
        }

        private void ToggleColliders(bool state)
        {
            if (cols == null) return;
            for (int i = 0; i < cols.Length; i++) cols[i].enabled = state;
        }
    }
}
