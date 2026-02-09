using UnityEngine;
using System.Collections.Generic;
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
        [SerializeField] private string placementPrompt = "???? E ?????";

        [Header("Animation")]
        [Tooltip("How fast the item slides into place.")]
        [SerializeField, Range(0.1f, 1f)] private float placeDuration = 0.3f;
        [Tooltip("The animation curve type.")]
        [SerializeField] private Ease placeEase = Ease.OutQuart;

        [Header("Runtime Data")]
        [SerializeField] private List<PickupableItem> itemsOnShelf = new List<PickupableItem>();

        private BoxCollider shelfCollider;
        private readonly Collider[] overlapBuffer = new Collider[1];

        public string PlacementPrompt => placementPrompt;
        public bool HasItems => GetAvailableItemsCount() > 0;

        private void Awake()
        {
            shelfCollider = GetComponent<BoxCollider>();
        }

        public PickupableItem GetRandomItem()
        {
            CleanItemList();
            if (itemsOnShelf.Count == 0) return null;
            return itemsOnShelf[Random.Range(0, itemsOnShelf.Count)];
        }

        public bool TryPlaceItem(PickupableItem item, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (ValidatePlacement(item, hitPoint, hitNormal, out Vector3 targetPosition, out Quaternion targetRotation))
            {
                // Kill animations to ensure a clean start
                item.transform.DOKill();
                
                // Detach from hand (this keeps it in world space, scale 0.1)
                item.Drop(Vector3.zero);
                
                if (!itemsOnShelf.Contains(item))
                    itemsOnShelf.Add(item);

                // WE DO NOT PARENT HERE. 
                // This avoids the scale distortion caused by the shelf scale.

                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.detectCollisions = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }

                // Smoothly animate position and rotation in world space
                item.transform.DOMove(targetPosition, placeDuration).SetEase(placeEase);
                item.transform.DORotateQuaternion(targetRotation, placeDuration).SetEase(placeEase)
                    .OnComplete(() => { if (rb) rb.detectCollisions = true; });
                
                return true;
            }
            return false;
        }

        public void ReleaseItem(PickupableItem item)
        {
            if (itemsOnShelf.Contains(item))
            {
                itemsOnShelf.Remove(item);
            }
        }

        private int GetAvailableItemsCount()
        {
            CleanItemList();
            return itemsOnShelf.Count;
        }

        private void CleanItemList()
        {
            // We only check if the item still exists and isn't being held.
            // Since they aren't children, we don't check the parent.
            itemsOnShelf.RemoveAll(item => item == null || item.IsHeld);
        }

        public bool ValidatePlacement(PickupableItem item, Vector3 hitPoint, Vector3 hitNormal, out Vector3 targetPosition, out Quaternion targetRotation)
        {
            targetPosition = Vector3.zero;
            targetRotation = Quaternion.identity;

            if (Vector3.Dot(hitNormal, transform.up) < 0.99f) return false;

            BoxCollider itemCol = item.GetComponent<BoxCollider>();
            if (itemCol == null) return false;

            targetRotation = Quaternion.LookRotation(transform.forward, transform.up);
            
            Vector3 worldScale = item.transform.lossyScale;
            float itemHalfHeight = Mathf.Abs((itemCol.size.y * worldScale.y * 0.5f) - (itemCol.center.y * worldScale.y));
            targetPosition = hitPoint + (hitNormal * (itemHalfHeight + 0.002f));

            if (IsPartiallyOffShelf(targetPosition, item, itemCol)) return false;

            Vector3 checkSize = Vector3.Scale(itemCol.size, worldScale) * 0.45f;
            return Physics.OverlapBoxNonAlloc(targetPosition, checkSize, overlapBuffer, targetRotation, obstacleMask) == 0;
        }

        private bool IsPartiallyOffShelf(Vector3 targetWorldPos, PickupableItem item, BoxCollider itemCol)
        {
            Vector3 localPos = transform.InverseTransformPoint(targetWorldPos);
            Vector3 worldScale = item.transform.lossyScale;
            
            float itemHalfWidth = (itemCol.size.x * worldScale.x) * 0.5f;
            float itemHalfDepth = (itemCol.size.z * worldScale.z) * 0.5f;

            float shelfMaxX = shelfCollider.center.x + (shelfCollider.size.x * 0.5f);
            float shelfMinX = shelfCollider.center.x - (shelfCollider.size.x * 0.5f);
            float shelfMaxZ = shelfCollider.center.z + (shelfCollider.size.z * 0.5f);
            float shelfMinZ = shelfCollider.center.z - (shelfCollider.size.z * 0.5f);

            return (localPos.x + itemHalfWidth > shelfMaxX) || 
                   (localPos.x - itemHalfWidth < shelfMinX) || 
                   (localPos.z + itemHalfDepth > shelfMaxZ) || 
                   (localPos.z - itemHalfDepth < shelfMinZ);
        }
    }
}
