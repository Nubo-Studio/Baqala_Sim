using UnityEngine;
using DG.Tweening;
using Systems.Items;

namespace Systems.Store
{
    [RequireComponent(typeof(BoxCollider))]
    public class Shelf : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Layers considered obstacles for item placement.")]
        [SerializeField] private LayerMask obstacleMask;
        [Tooltip("Arabic prompt shown when holding an item.")]
        [SerializeField] private string placementPrompt = "إضغط E لوضعه";

        [Header("Animation")]
        [Tooltip("How fast the item slides into place.")]
        [SerializeField, Range(0.1f, 1f)] private float placeDuration = 0.3f;
        [Tooltip("The animation curve type.")]
        [SerializeField] private Ease placeEase = Ease.OutQuart;

        private BoxCollider shelfCollider;
        private readonly Collider[] overlapBuffer = new Collider[1];

        public string PlacementPrompt => placementPrompt;

        private void Awake()
        {
            shelfCollider = GetComponent<BoxCollider>();
        }

        public bool TryPlaceItem(PickupableItem item, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (ValidatePlacement(item, hitPoint, hitNormal, out Vector3 targetPosition, out Quaternion targetRotation))
            {
                item.Drop(Vector3.zero);
                item.transform.SetParent(null, true);
                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.detectCollisions = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
                Sequence placeSequence = DOTween.Sequence();
                placeSequence.Join(item.transform.DOMove(targetPosition, placeDuration).SetEase(placeEase));      
                placeSequence.Join(item.transform.DORotateQuaternion(targetRotation, placeDuration).SetEase(placeEase));
                placeSequence.OnComplete(() => { if (rb) rb.detectCollisions = true; });
                return true;
            }
            return false;
        }

        public bool ValidatePlacement(PickupableItem item, Vector3 hitPoint, Vector3 hitNormal, out Vector3 targetPosition, out Quaternion targetRotation)
        {
            targetPosition = Vector3.zero;
            targetRotation = Quaternion.identity;
            if (Vector3.Dot(hitNormal, transform.up) < 0.99f) return false;
            BoxCollider itemCol = item.GetComponent<BoxCollider>();
            if (itemCol == null) return false;
            targetRotation = Quaternion.LookRotation(transform.forward, transform.up);
            float itemHalfHeight = Mathf.Abs((itemCol.size.y * item.transform.localScale.y * 0.5f) - (itemCol.center.y * item.transform.localScale.y));
            targetPosition = hitPoint + (hitNormal * (itemHalfHeight + 0.002f));
            if (IsPartiallyOffShelf(targetPosition, item, itemCol)) return false;
            Vector3 checkSize = Vector3.Scale(itemCol.size, item.transform.localScale) * 0.45f;
            return Physics.OverlapBoxNonAlloc(targetPosition, checkSize, overlapBuffer, targetRotation, obstacleMask) == 0;
        }

        private bool IsPartiallyOffShelf(Vector3 targetWorldPos, PickupableItem item, BoxCollider itemCol)  
        {
            Vector3 localPos = transform.InverseTransformPoint(targetWorldPos);
            float itemHalfWidth = (itemCol.size.x * item.transform.localScale.x) * 0.5f;
            float itemHalfDepth = (itemCol.size.z * item.transform.localScale.z) * 0.5f;
            float shelfMaxX = shelfCollider.center.x + (shelfCollider.size.x * 0.5f);
            float shelfMinX = shelfCollider.center.x - (shelfCollider.size.x * 0.5f);
            float shelfMaxZ = shelfCollider.center.z + (shelfCollider.size.z * 0.5f);
            float shelfMinZ = shelfCollider.center.z - (shelfCollider.size.z * 0.5f);
            return (localPos.x + itemHalfWidth > shelfMaxX) || (localPos.x - itemHalfWidth < shelfMinX) || (localPos.z + itemHalfDepth > shelfMaxZ) || (localPos.z - itemHalfDepth < shelfMinZ);
        }
    }
}