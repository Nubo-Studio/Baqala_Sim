using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float range = 4f;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private float throwForce = 6f;
    [SerializeField] private Material validGhostMat;
    [SerializeField] private Material invalidGhostMat;

    public IPickupable HeldItem { get; private set; }
    public IInteractable CurrentTarget { get; private set; }

    private PlacementGhost ghost;

    private void Awake()
    {
        ghost = GetComponent<PlacementGhost>();
        if (ghost == null) ghost = gameObject.AddComponent<PlacementGhost>();
    }

    private void Start()
    {
        if (inputReader != null)
        {
            inputReader.OnInteractPressed += HandleInteract;
            inputReader.OnThrowPressed += HandleThrow;
        }
        if (playerCamera == null) playerCamera = Camera.main;

        if (validGhostMat == null || invalidGhostMat == null) CreateDefaultMaterials();
        ghost.Initialize(validGhostMat, invalidGhostMat);
    }

    private void CreateDefaultMaterials()
    {
        if (validGhostMat == null)
        {
            validGhostMat = new Material(Shader.Find("Standard"));
            validGhostMat.color = new Color(0, 1, 0, 0.4f);
            SetupTransparent(validGhostMat);
        }
        if (invalidGhostMat == null)
        {
            invalidGhostMat = new Material(Shader.Find("Standard"));
            invalidGhostMat.color = new Color(1, 0, 0, 0.4f);
            SetupTransparent(invalidGhostMat);
        }
    }

    private void SetupTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
    }

    private void Update()
    {
        if (HeldItem == null)
        {
            UpdateTarget();
            ghost.Clear();
        }
        else
        {
            CurrentTarget = null;
            UpdatePlacementVisuals();
        }
    }

    private void HandleInteract()
    {
        if (HeldItem != null)
        {
            int mask = interactMask & ~(1 << 7);
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, range, mask))
            {
                Shelf shelf = hit.collider.GetComponent<Shelf>();
                if (shelf != null && shelf.TryPlaceItem((PickupableItem)HeldItem, hit.point, hit.normal))
                {
                    HeldItem = null;
                    ghost.Clear();
                    return;
                }
            }
            HeldItem.Drop(Vector3.zero);
            HeldItem = null;
            ghost.Clear();
            return;
        }

        if (CurrentTarget != null && CurrentTarget.CanInteract(gameObject))
        {
            if (CurrentTarget is IPickupable pickupable)
            {
                pickupable.Pickup(holdPoint, gameObject);
                HeldItem = pickupable;
                ghost.CreateGhost(((PickupableItem)HeldItem).gameObject);
            }
            else CurrentTarget.Interact(gameObject);
        }
    }

    private void HandleThrow()
    {
        if (HeldItem == null) return;
        HeldItem.Drop(playerCamera.transform.forward * throwForce);
        HeldItem = null;
        ghost.Clear();
    }

    private void UpdateTarget()
    {
        CurrentTarget = null;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, range, interactMask))
        {
            CurrentTarget = hit.collider.GetComponent<IInteractable>();
        }
    }

    private void UpdatePlacementVisuals()
    {
        int mask = interactMask & ~(1 << 7);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, range, mask))
        {
            Shelf shelf = hit.collider.GetComponent<Shelf>();
            if (shelf != null)
            {
                bool valid = shelf.ValidatePlacement((PickupableItem)HeldItem, hit.point, hit.normal, out Vector3 pos, out Quaternion rot);
                ghost.UpdateGhost(pos != Vector3.zero ? pos : hit.point, pos != Vector3.zero ? rot : Quaternion.LookRotation(hit.normal), valid, true);
            }
            else ghost.UpdateGhost(hit.point, Quaternion.LookRotation(hit.normal), false, true);
        }
        else ghost.UpdateGhost(Vector3.zero, Quaternion.identity, false, false);
    }
}
