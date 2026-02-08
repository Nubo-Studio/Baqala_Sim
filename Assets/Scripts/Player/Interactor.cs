using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;

    [Header("Raycast")]
    [SerializeField] private float range = 4f;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private float throwForce = 6f;

    public IPickupable HeldItem { get; private set; }
    public IInteractable CurrentTarget { get; private set; }

    private void Start()
    {
        if (inputReader != null)
        {
            inputReader.OnInteractPressed += HandleInteract;
            inputReader.OnThrowPressed += HandleThrow;
        }
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void Update()
    {
        if (HeldItem == null) UpdateTarget();
        else CurrentTarget = null;
    }

    private void HandleInteract()
    {
        if (HeldItem != null)
        {
            int layerMaskWithoutItems = interactMask & ~(1 << 7);

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, range, layerMaskWithoutItems))
            {
                Shelf shelf = hit.collider.GetComponent<Shelf>();
                if (shelf != null)
                {
                    if (shelf.TryPlaceItem((PickupableItem)HeldItem, hit.point, hit.normal))
                    {
                        HeldItem = null;
                        return;
                    }
                }
            }

            HeldItem.Drop(Vector3.zero);
            HeldItem = null;
            return;
        }

        if (CurrentTarget != null && CurrentTarget.CanInteract(gameObject))
        {
            if (CurrentTarget is IPickupable pickupable)
            {
                pickupable.Pickup(holdPoint, gameObject);
                HeldItem = pickupable;
            }
            else
            {
                CurrentTarget.Interact(gameObject);
            }
        }
    }

    private void HandleThrow()
    {
        if (HeldItem == null) return;
        HeldItem.Drop(playerCamera.transform.forward * throwForce);
        HeldItem = null;
    }

    private void UpdateTarget()
    {
        CurrentTarget = null;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, interactMask))
        {
            CurrentTarget = hit.collider.GetComponent<IInteractable>();
        }
    }
}