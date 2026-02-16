using UnityEngine;
using DG.Tweening;
using Interaction;

namespace Systems.Store
{
    [RequireComponent(typeof(BoxCollider))]
    public class MovableShelf : MonoBehaviour, IMovable
    {
        [Header("UI")]
        [SerializeField] private string movePrompt = "إضغط E لتحريك";

        [Header("Placement Settings")]
        [SerializeField] private LayerMask placementMask;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private float maxPlacementDistance = 5f;
        [SerializeField] private float placementHeightOffset = 0.01f;

        [Header("Animation")]
        [SerializeField, Range(0.1f, 1f)] private float moveDuration = 0.4f;
        [SerializeField] private Ease moveEase = Ease.OutQuart;

        private BoxCollider shelfCollider;
        private bool isBeingMoved;

        public string Prompt => movePrompt;
        public Transform Transform => transform;
        public LayerMask PlacementMask => placementMask;
        public float MaxPlacementDistance => maxPlacementDistance;

        private void Awake()
        {
            shelfCollider = GetComponent<BoxCollider>();
        }

        public bool CanInteract(GameObject interactor)
        {
            return !isBeingMoved;
        }

        public void Interact(GameObject interactor) { }

        public void StartMoving(GameObject interactor)
        {
            isBeingMoved = true;
        }

        public void MoveTo(Vector3 position, Quaternion rotation)
        {
            transform.DOKill();
            isBeingMoved = false;

            Sequence moveSequence = DOTween.Sequence();
            moveSequence.Join(transform.DOMove(position, moveDuration).SetEase(moveEase));
            moveSequence.Join(transform.DORotateQuaternion(rotation, moveDuration).SetEase(moveEase));
        }

        public void CancelMoving()
        {
            isBeingMoved = false;
        }

        public bool ValidatePlacement(Vector3 hitPoint, Vector3 hitNormal, int hitLayer, out Vector3 targetPosition, out Quaternion targetRotation)
        {
            targetPosition = Vector3.zero;
            targetRotation = Quaternion.identity;

            if (!IsLayerInMask(hitLayer, placementMask))
                return false;

            if (Vector3.Dot(hitNormal, Vector3.up) < 0.99f)
                return false;

            targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hitNormal), hitNormal);
            
            Vector3 shelfSize = Vector3.Scale(shelfCollider.size, transform.localScale);
            float shelfHalfHeight = shelfSize.y * 0.5f;
            targetPosition = hitPoint + (hitNormal * (shelfHalfHeight + placementHeightOffset));

            Vector3 checkSize = shelfSize * 0.45f;
            Collider[] overlaps = Physics.OverlapBox(targetPosition, checkSize, targetRotation, obstacleMask);
            
            foreach (Collider col in overlaps)
            {
                if (col.transform == transform)
                    continue;
                return false;
            }

            return true;
        }

        private bool IsLayerInMask(int layer, LayerMask mask)
        {
            return ((1 << layer) & mask) != 0;
        }
    }
}
