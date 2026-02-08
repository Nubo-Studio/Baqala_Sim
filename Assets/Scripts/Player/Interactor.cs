using UnityEngine;
using RTLTMPro;
using Interaction;
using Systems.Store;
using Systems.Items;
using DG.Tweening;

namespace Player
{
    [DisallowMultipleComponent]
    public class Interactor : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Link to the InputReader ScriptableObject.")]
        [SerializeField] private InputReader inputReader;
        [Tooltip("The main FPS camera.")]
        [SerializeField] private Camera playerCamera;
        [Tooltip("Where the item will be positioned when held.")]
        [SerializeField] private Transform holdPoint;

        [Header("Settings")]
        [Tooltip("Distance the player can reach to interact.")]
        [SerializeField, Range(1f, 10f)] private float range = 4f;
        [Tooltip("Which layers the raycast should hit.")]
        [SerializeField] private LayerMask interactMask;
        [Tooltip("How hard the item is thrown.")]
        [SerializeField, Range(1f, 20f)] private float throwForce = 6f;

        [Header("Visuals (Ghost)")]
        [Tooltip("Material shown when placement is valid.")]
        [SerializeField] private Material validGhostMat;
        [Tooltip("Material shown when placement is blocked.")]
        [SerializeField] private Material invalidGhostMat;

        [Header("UI")]
        [Tooltip("The RTL Text component for Arabic prompts.")]
        [SerializeField] private RTLTextMeshPro promptText;

        public IPickupable HeldItem { get; private set; }
        public IInteractable CurrentTarget { get; private set; }

        private PlacementGhost ghost;
        private static Shader ghostShader;
        private string lastPrompt;
        private CanvasGroup promptCanvasGroup;
        private RaycastHit rayHit;

        private void Awake()
        {
            DOTween.Init();
            ghost = GetComponent<PlacementGhost>();
            if (ghost == null) ghost = gameObject.AddComponent<PlacementGhost>();

            if (promptText != null)
            {
                promptCanvasGroup = promptText.GetComponent<CanvasGroup>();
                if (promptCanvasGroup == null) promptCanvasGroup = promptText.gameObject.AddComponent<CanvasGroup>();
                promptText.ForceFix = true;
                promptText.FixTags = true;
                promptText.Farsi = false;
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
            WarmupPromptText();
            ghost.Initialize(validGhostMat, invalidGhostMat);
        }
        
        private void OnDestroy()
        {
            if (inputReader != null)
            {
                inputReader.OnInteractPressed -= HandleInteract;
                inputReader.OnThrowPressed -= HandleThrow;
            }
        }

        private void WarmupPromptText()
        {
            if (promptText == null) return;
            promptText.text = "إضغط E للاخذ";
            promptText.text = "إضغط E لوضعه";
            promptText.text = string.Empty;
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
                validGhostMat = new Material(ghostShader) { color = new Color(0, 1, 0, 0.4f) };
                SetupTransparent(validGhostMat);
            }
            if (invalidGhostMat == null)
            {
                invalidGhostMat = new Material(ghostShader) { color = new Color(1, 0, 0, 0.4f) };
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
                HandleFreeLook();
            }
            else
            {
                CurrentTarget = null;
                HandleHoldingItem();
            }
        }

        private void HandleFreeLook()
        {
            UpdateTarget();
            if (ghost.isActiveAndEnabled) ghost.Clear();
            UpdatePrompt(CurrentTarget?.Prompt);
        }

        private void HandleHoldingItem()
        {
            int mask = interactMask & ~(1 << 7);
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out rayHit, range, mask))
            {
                if (rayHit.collider.TryGetComponent(out Shelf shelf))
                {
                    UpdatePrompt(shelf.PlacementPrompt);
                    bool valid = shelf.ValidatePlacement((PickupableItem)HeldItem, rayHit.point, rayHit.normal, out Vector3 pos, out Quaternion rot);
                    Vector3 ghostPos = pos != Vector3.zero ? pos : rayHit.point;
                    Quaternion ghostRot = pos != Vector3.zero ? rot : Quaternion.LookRotation(rayHit.normal);
                    ghost.UpdateGhost(ghostPos, ghostRot, valid, true);
                }
                else
                {
                    UpdatePrompt(null);
                    ghost.UpdateGhost(rayHit.point, Quaternion.LookRotation(rayHit.normal), false, true);
                }
            }
            else
            {
                UpdatePrompt(null);
                ghost.UpdateGhost(Vector3.zero, Quaternion.identity, false, false);
            }
        }

        private void UpdateTarget()
        {
            CurrentTarget = null;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out rayHit, range, interactMask))
            {
                rayHit.collider.TryGetComponent(out IInteractable interactable);
                CurrentTarget = interactable;
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

        private void HandleInteract()
        {
            if (HeldItem != null) AttemptPlace();
            else if (CurrentTarget != null && CurrentTarget.CanInteract(gameObject)) AttemptPickupOrInteract();
        }

        private void AttemptPlace()
        {
            int mask = interactMask & ~(1 << 7);
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out rayHit, range, mask))
            {
                if (rayHit.collider.TryGetComponent(out Shelf shelf))
                {
                    if (shelf.TryPlaceItem((PickupableItem)HeldItem, rayHit.point, rayHit.normal))
                    {
                        HeldItem = null;
                        ghost.Clear();
                        UpdatePrompt(null);
                        return;
                    }
                }
            }
            HeldItem.Drop(Vector3.zero);
            HeldItem = null;
            ghost.Clear();
            UpdatePrompt(null);
        }

        private void AttemptPickupOrInteract()
        {
            if (CurrentTarget is IPickupable pickupable)
            {
                pickupable.Pickup(holdPoint, gameObject);
                HeldItem = pickupable;
                if (HeldItem is MonoBehaviour monoItem) ghost.CreateGhost(monoItem.gameObject);
                UpdatePrompt(null);
            }
            else CurrentTarget.Interact(gameObject);
        }

        private void HandleThrow()
        {
            if (HeldItem == null) return;
            HeldItem.Drop(playerCamera.transform.forward * throwForce);
            HeldItem = null;
            ghost.Clear();
            UpdatePrompt(null);
        }
    }
}