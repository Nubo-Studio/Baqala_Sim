using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(BoxCollider))]
public class Shelf : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask obstacleMask;
    [Header("Animation")]
    [SerializeField] private float placeDuration = 0.3f;
    [SerializeField] private Ease placeEase = Ease.OutQuart;

    private BoxCollider shelfCollider;

    private void Awake()
    {
        shelfCollider = GetComponent<BoxCollider>();
    }

    public bool TryPlaceItem(PickupableItem item, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (Vector3.Dot(hitNormal, transform.up) < 0.99f) return false;

        BoxCollider itemCol = item.GetComponent<BoxCollider>();
        if (itemCol == null) return false;

        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, transform.up);

        float itemHalfHeight = (itemCol.size.y * item.transform.localScale.y * 0.5f)
                             - (itemCol.center.y * item.transform.localScale.y);
        itemHalfHeight = Mathf.Abs(itemHalfHeight);

        Vector3 targetPosition = hitPoint + (hitNormal * (itemHalfHeight + 0.002f));

        if (IsPartiallyOffShelf(targetPosition, item, itemCol))
        {
            Debug.Log("Item hanging off edge.");
            return false;
        }

        Vector3 checkSize = Vector3.Scale(itemCol.size, item.transform.localScale) * 0.45f;
        Collider[] hits = Physics.OverlapBox(targetPosition, checkSize, targetRotation, obstacleMask);
        if (hits.Length > 0) return false;


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

        // Animate (Tween)
        Sequence placeSequence = DOTween.Sequence();

        // Use DOMove (World Space) because we are not parented
        placeSequence.Join(item.transform.DOMove(targetPosition, placeDuration).SetEase(placeEase));
        placeSequence.Join(item.transform.DORotateQuaternion(targetRotation, placeDuration).SetEase(placeEase));

        placeSequence.OnComplete(() =>
        {
            if (rb) rb.detectCollisions = true;
        });

        return true;
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

        bool overhangX = (localPos.x + itemHalfWidth > shelfMaxX) || (localPos.x - itemHalfWidth < shelfMinX);
        bool overhangZ = (localPos.z + itemHalfDepth > shelfMaxZ) || (localPos.z - itemHalfDepth < shelfMinZ);

        return overhangX || overhangZ;
    }
}