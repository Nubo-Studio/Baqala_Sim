using UnityEngine;
using DG.Tweening;
using Interaction;

namespace Systems.Store
{
    [RequireComponent(typeof(BoxCollider))]
    public class MovableObject : MonoBehaviour, IMovable
    {
        [Header("UI")]
        [SerializeField] private string movePrompt = "إضغط E للتحريك";

        [Header("Placement Settings")]
        [SerializeField] private LayerMask placementMask;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private float maxPlacementDistance = 5f;
        [SerializeField] private float placementHeightOffset = 0.01f;

        [Header("Current Place Ghost")]
        [SerializeField] private Material currentPlaceMaterial;

        [Header("Animation")]
        [SerializeField, Range(0.1f, 1f)] private float moveDuration = 0.4f;
        [SerializeField] private Ease moveEase = Ease.OutQuart;

        private BoxCollider[] objectColliders;
        private BoxCollider rootCollider;
        private bool isBeingMoved;
        private Renderer[] objectRenderers;
        private Material[] originalMaterials;

        public string Prompt => movePrompt;
        public Transform Transform => transform;
        public LayerMask PlacementMask => placementMask;
        public float MaxPlacementDistance => maxPlacementDistance;

        private void Awake()
        {
            objectColliders = GetComponentsInChildren<BoxCollider>();
            rootCollider = GetComponent<BoxCollider>();
            objectRenderers = GetComponentsInChildren<Renderer>();

        }

        public bool CanInteract(GameObject interactor)
        {
            return !isBeingMoved;
        }

        public void Interact(GameObject interactor) { }

        public void StartMoving(GameObject interactor)
        {
            isBeingMoved = true;

            foreach (BoxCollider col in objectColliders)
                col.enabled = false;

            if (currentPlaceMaterial == null) return;

            originalMaterials = new Material[objectRenderers.Length];
            for (int i = 0; i < objectRenderers.Length; i++)
            {
                originalMaterials[i] = objectRenderers[i].sharedMaterial;
                objectRenderers[i].sharedMaterial = currentPlaceMaterial;
            }
        }

        private void RestoreObject()
        {
            foreach (BoxCollider col in objectColliders)
                col.enabled = true;

            if (originalMaterials == null) return;

            for (int i = 0; i < objectRenderers.Length && i < originalMaterials.Length; i++)
            {
                objectRenderers[i].sharedMaterial = originalMaterials[i];
            }

            originalMaterials = null;
        }

        public void MoveTo(Vector3 position, Quaternion rotation)
        {
            transform.DOKill();
            isBeingMoved = false;

            Sequence moveSequence = DOTween.Sequence();
            moveSequence.Join(transform.DOMove(position, moveDuration).SetEase(moveEase));
            moveSequence.Join(transform.DORotateQuaternion(rotation, moveDuration).SetEase(moveEase));
            moveSequence.OnComplete(RestoreObject);
        }

        public void CancelMoving()
        {
            isBeingMoved = false;
            RestoreObject();
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
            
            Vector3 shelfSize = Vector3.Scale(rootCollider.size, transform.localScale);
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
