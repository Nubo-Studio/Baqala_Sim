using UnityEngine;
using DG.Tweening;
using Interaction;

namespace Systems.Store
{
    [RequireComponent(typeof(BoxCollider))]
    public class Shelf : MonoBehaviour
    {
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private float placeDuration = 0.3f;
        [SerializeField] private Ease placeEase = Ease.OutQuart;
        [SerializeField] private string placementPrompt = "إضغط E لوضعه";

        private BoxCollider shelfCollider;
        private Collider[] overlapBuffer = new Collider[1];

        public string PlacementPrompt => placementPrompt;

        private void Awake()
        {
            shelfCollider = GetComponent<BoxCollider>();
        }

        public bool TryPlaceItem(Items.PickupableItem item, Vector3 hitPoint, Vector3 hitNormal)
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

                placeSequence.OnComplete(() =>
                {
                    if (rb) rb.detectCollisions = true;
                });

                return true;
            }

            return false;
        }

        public bool ValidatePlacement(Items.PickupableItem item, Vector3 hitPoint, Vector3 hitNormal, out Vector3 targetPosition, out Quaternion targetRotation)
        {
            targetPosition = Vector3.zero;
            targetRotation = Quaternion.identity;

            if (Vector3.Dot(hitNormal, transform.up) < 0.99f) return false;

            BoxCollider itemCol = item.GetComponent<BoxCollider>();
            if (itemCol == null) return false;

            targetRotation = Quaternion.LookRotation(transform.forward, transform.up);

            float itemHalfHeight = (itemCol.size.y * item.transform.localScale.y * 0.5f)
                                 - (itemCol.center.y * item.transform.localScale.y);
            itemHalfHeight = Mathf.Abs(itemHalfHeight);

            targetPosition = hitPoint + (hitNormal * (itemHalfHeight + 0.002f));

            if (IsPartiallyOffShelf(targetPosition, item, itemCol)) return false;

            Vector3 checkSize = Vector3.Scale(itemCol.size, item.transform.localScale) * 0.45f;
            int hitCount = Physics.OverlapBoxNonAlloc(targetPosition, checkSize, overlapBuffer, targetRotation, obstacleMask);
            if (hitCount > 0) return false;

            return true;
        }

        private bool IsPartiallyOffShelf(Vector3 targetWorldPos, Items.PickupableItem item, BoxCollider itemCol) 
        {
            Vector3 localPos = transform.InverseTransformPoint(targetWorldPos);

            float itemHalfWidth = (itemCol.size.x * item.transform.localScale.x) * 0.5f;
            float itemHalfDepth = (itemCol.size.z * item.transform.localScale.z) * 0.5f;

            float shelfMaxX = shelfCollider.center.x + (shelfCollider.size.x * 0.5f);
            float shelfMinX = shelfCollider.center.x - (shelfCollider.size.x * 0.5f);
            float shelfMaxZ = shelfCollider.center.z + (shelfCollider.size.z * 0.5f);
            float shelfMinZ = shelfCollider.center.z - (shelfCollider.size.z * 0.5f);

            bool overhangX = (localPos.x + itemHalfWidth > shelfMaxX) || (localPos.x - itemHalfWidth < shelfMinX);
            bool overhangZ = (localPos.z + itemHalfDepth > shelfMaxZ) || (localPos.z - itemHalfDepth < shelfMinZ);

            return overhangX || overhangZ;
        }
    }
}