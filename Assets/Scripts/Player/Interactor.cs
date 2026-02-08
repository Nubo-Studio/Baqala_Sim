using UnityEngine;
using RTLTMPro;
using Interaction;
using Systems.Store;
using Systems.Items;
using DG.Tweening;
using System.Collections;

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
    [SerializeField] private RTLTextMeshPro promptText;

    public IPickupable HeldItem { get; private set; }
    public IInteractable CurrentTarget { get; private set; }

    private PlacementGhost ghost;
    private static Shader ghostShader;
    private string lastPrompt;
    private CanvasGroup promptCanvasGroup;

    private void Awake()
    {
        DOTween.Init();
        ghost = GetComponent<PlacementGhost>();
        if (ghost == null) ghost = gameObject.AddComponent<PlacementGhost>();
        
        if (promptText != null)
        {
            // Ensure performance components
            promptCanvasGroup = promptText.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null) promptCanvasGroup = promptText.gameObject.AddComponent<CanvasGroup>();
            
            // Force best RTL settings for performance and correctness
            promptText.ForceFix = true;
            promptText.FixTags = true;
            promptText.Farsi = false; // Set to true if you need Farsi specific characters
            
            // Ensure the text object is ready but invisible
            promptText.gameObject.SetActive(true);
            SetPromptVisibility(false);
        }
    }

    private void Start()
    {
        if (inputReader != null)
        {
            inputReader.OnInteractPressed += HandleInteract;
            inputReader.OnThrowPressed += HandleThrow;
        }

        if (playerCamera == null) playerCamera = Camera.main;

        PrepareMaterials();

        if (promptText != null)
        {
            // Pre-process strings during loading to warm up the RTL cache
            promptText.text = "إضغط E للاخذ";
            promptText.text = "إضغط E لوضعه";
            promptText.text = "";
        }

        ghost.Initialize(validGhostMat, invalidGhostMat);
    }

    private void PrepareMaterials()
    {
        if (ghostShader == null)
        {
            ghostShader = Shader.Find("Universal Render Pipeline/Lit");
            if (ghostShader == null) ghostShader = Shader.Find("Standard");
        }

        if (validGhostMat == null)
        {
            validGhostMat = new Material(ghostShader);
            validGhostMat.color = new Color(0, 1, 0, 0.4f);
            SetupTransparent(validGhostMat);
        }
        if (invalidGhostMat == null)
        {
            invalidGhostMat = new Material(ghostShader);
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
            if (ghost.isActiveAndEnabled) ghost.Clear();
            UpdatePrompt(CurrentTarget?.Prompt);
        }
        else
        {
            CurrentTarget = null;
            UpdatePlacementLogic();
        }
    }

    private void UpdatePrompt(string text)
    {
        if (promptText == null) return;
        
        if (lastPrompt == text) return;
        lastPrompt = text;

        if (string.IsNullOrEmpty(text))
        {
            SetPromptVisibility(false);
        }
        else
        {
            promptText.text = text;
            SetPromptVisibility(true);
        }
    }

    private void SetPromptVisibility(bool visible)
    {
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = visible ? 1f : 0f;
            promptCanvasGroup.blocksRaycasts = visible;
        }
    }

    private void UpdatePlacementLogic()
    {
        int mask = interactMask & ~(1 << 7);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, range, mask))
        {
            Shelf shelf = hit.collider.GetComponent<Shelf>();
            if (shelf != null)
            {
                UpdatePrompt(shelf.PlacementPrompt);
                bool valid = shelf.ValidatePlacement((PickupableItem)HeldItem, hit.point, hit.normal, out Vector3 pos, out Quaternion rot);
                ghost.UpdateGhost(pos != Vector3.zero ? pos : hit.point, pos != Vector3.zero ? rot : Quaternion.LookRotation(hit.normal), valid, true);
            }
            else
            {
                UpdatePrompt(null);
                ghost.UpdateGhost(hit.point, Quaternion.LookRotation(hit.normal), false, true);
            }
        }
        else
        {
            UpdatePrompt(null);
            ghost.UpdateGhost(Vector3.zero, Quaternion.identity, false, false);
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
                    UpdatePrompt(null);
                    return;
                }
            }
            HeldItem.Drop(Vector3.zero);
            HeldItem = null;
            ghost.Clear();
            UpdatePrompt(null);
            return;
        }

        if (CurrentTarget != null && CurrentTarget.CanInteract(gameObject))
        {
            if (CurrentTarget is IPickupable pickupable)
            {
                pickupable.Pickup(holdPoint, gameObject);
                HeldItem = pickupable;
                ghost.CreateGhost(((PickupableItem)HeldItem).gameObject);
                UpdatePrompt(null);
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
        UpdatePrompt(null);
    }

    private void UpdateTarget()
    {
        CurrentTarget = null;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, range, interactMask))
        {
            CurrentTarget = hit.collider.GetComponent<IInteractable>();
        }
    }
}