using DG.Tweening;
using UnityEngine;
using Interaction;

namespace Systems.Items
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class PickupableItems : MonoBehaviour, IPickupable
    {
        [SerializeField] private string prompt;
        [SerializeField] private bool disableCollidersWhileHeld = true;
        [SerializeField] private Vector3 localHoldPositionOffset = Vector3.zero;
        [SerializeField] private Vector3 localHoldEulerOffset = Vector3.zero;
        [SerializeField] private float pickupDuration = 0.2f;
        [SerializeField] private Ease pickupEase = Ease.OutBack;

        private Rigidbody rb;
        private Collider[] cols;
        private Transform holdPoint;
        private bool held;

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
            transform.DOKill();
            held = true;
            holdPoint = newHoldPoint;
            if (disableCollidersWhileHeld) foreach (var c in cols) c.enabled = false;
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
            if (disableCollidersWhileHeld) foreach (var c in cols) c.enabled = true;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.linearVelocity = throwVelocity;
            holdPoint = null;
        }
    }
}
