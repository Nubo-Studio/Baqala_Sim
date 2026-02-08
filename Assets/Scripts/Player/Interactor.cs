using UnityEngine;
using RTLTMPro;
using Interaction;
using Systems.Store;
using Systems.Items;
using DG.Tweening;
using UnityEngine.UI;

namespace Player
{
    [DisallowMultipleComponent]
    public class Interactor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform holdPoint;

        [Header("Settings")]
        [SerializeField, Range(1f, 10f)] private float range = 4f;
        [SerializeField] private LayerMask interactMask;
        [SerializeField, Range(1f, 20f)] private float throwForce = 6f;

        [Header("Visuals (Ghost)")]
        [SerializeField] private Material validGhostMat;
        [SerializeField] private Material invalidGhostMat;

        [Header("UI - Prompts")]
        [SerializeField] private RTLTextMeshPro promptText;
        [SerializeField] private RTLTextMeshPro holdInstructionsText;

        [Header("UI - Item Info Card")]
        [SerializeField] private CanvasGroup itemInfoCanvasGroup;
        [SerializeField] private RTLTextMeshPro itemNameText;
        [SerializeField] private RTLTextMeshPro itemPriceText;
        [SerializeField] private Image itemIconImage;

        [Header("UI Animation")]
        [SerializeField, Range(0.05f, 0.5f)] private float uiFadeDuration = 0.15f;

        public IPickupable HeldItem { get; private set; }
        public IInteractable CurrentTarget { get; private set; }

        private PlacementGhost ghost;
        private static Shader ghostShader;
        private string lastPrompt;
        private ItemData lastItemData;
        private CanvasGroup promptCanvasGroup;
        private CanvasGroup holdCanvasGroup;
        private RaycastHit rayHit;
        private float displayedPrice;

        private void Awake()
        {
            DOTween.Init();
            ghost = GetComponent<PlacementGhost>();
            if (ghost == null) ghost = gameObject.AddComponent<PlacementGhost>();

            SetupUI();
        }

        private void SetupUI()
        {
            if (promptText != null)
            {
                promptCanvasGroup = EnsureCanvasGroup(promptText.gameObject);
                ConfigureRTLText(promptText);
                promptCanvasGroup.alpha = 0;
            }

            if (holdInstructionsText != null)
            {
                holdCanvasGroup = EnsureCanvasGroup(holdInstructionsText.gameObject);
                ConfigureRTLText(holdInstructionsText);
                holdCanvasGroup.alpha = 0;
            }

            if (itemInfoCanvasGroup != null)
            {
                itemInfoCanvasGroup.alpha = 0;
                itemInfoCanvasGroup.transform.localScale = Vector3.one * 0.95f;
                if (itemNameText != null) ConfigureRTLText(itemNameText);
                if (itemPriceText != null) ConfigureRTLText(itemPriceText);
            }
        }

        private CanvasGroup EnsureCanvasGroup(GameObject obj)
        {
            var cg = obj.GetComponent<CanvasGroup>();
            return cg != null ? cg : obj.AddComponent<CanvasGroup>();
        }

        private void ConfigureRTLText(RTLTextMeshPro text)
        {
            text.ForceFix = true;
            text.FixTags = true;
            text.Farsi = false;
            text.gameObject.SetActive(true);
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
            WarmupTexts();

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

        private void WarmupTexts()
        {
            if (promptText != null)
            {
                promptText.text = "إضغط E للاخذ";
                promptText.text = "إضغط E لوضعه";
                promptText.text = string.Empty;
            }
            if (holdInstructionsText != null)
            {
                holdInstructionsText.text = "إضغط E لترك\nإضغط Q للرمي";
            }
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
            bool isHolding = HeldItem != null;
            FadeCanvasGroup(holdCanvasGroup, isHolding ? 1 : 0);

            if (!isHolding)
            {
                HandleFreeLook();
                UpdateItemInfo(null);
            }
            else
            {
                CurrentTarget = null;
                HandleHoldingItem();

                if (HeldItem is PickupableItem item)
                    UpdateItemInfo(item.Data);
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
                    Quaternion ghostRot = pos != Vector3.zero ? rot : Quaternion.LookRotation(rayHit.normal); // Fixed variable from 'hit.normal' to 'rayHit.normal'
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
                FadeCanvasGroup(promptCanvasGroup, 0);
            }
            else
            {
                promptText.text = text;
                FadeCanvasGroup(promptCanvasGroup, 1);
                promptText.transform.DOPunchScale(Vector3.one * 0.1f, uiFadeDuration, 1, 0.1f);
            }
        }

        private void UpdateItemInfo(ItemData data)
        {
            if (itemInfoCanvasGroup == null) return;
            if (lastItemData == data) return;
            lastItemData = data;

            if (data == null)
            {
                FadeCanvasGroup(itemInfoCanvasGroup, 0);
                itemInfoCanvasGroup.transform.DOScale(0.95f, uiFadeDuration).SetEase(Ease.InQuad);
                displayedPrice = 0;
            }
            else
            {
                FadeCanvasGroup(itemInfoCanvasGroup, 1);
                itemInfoCanvasGroup.transform.DOPunchScale(Vector3.one * 0.05f, uiFadeDuration, 1, 0.1f);
                itemInfoCanvasGroup.transform.DOScale(1f, uiFadeDuration).SetEase(Ease.OutBack);

                if (itemNameText != null) itemNameText.text = $"اسم المنتج: {data.itemNameArabic}";

                if (itemPriceText != null)
                {
                    DOTween.To(() => displayedPrice, x => displayedPrice = x, data.sellPrice, 0.5f)
                        .SetEase(Ease.OutCubic)
                        .OnUpdate(() =>
                        {
                            itemPriceText.text = $"سعر المنتج: {displayedPrice:0} ريال";
                        });
                }

                if (itemIconImage != null)
                {
                    itemIconImage.sprite = data.icon;
                    itemIconImage.gameObject.SetActive(data.icon != null);
                }
            }
        }

        private void FadeCanvasGroup(CanvasGroup cg, float targetAlpha)
        {
            if (cg == null) return;
            if (Mathf.Approximately(cg.alpha, targetAlpha)) return;

            cg.DOKill();
            cg.DOFade(targetAlpha, uiFadeDuration).SetUpdate(true);
            cg.blocksRaycasts = targetAlpha > 0.5f;
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
                        ClearHeldItem();
                        return;
                    }
                }
            }
            HeldItem.Drop(Vector3.zero);
            ClearHeldItem();
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
            ClearHeldItem();
        }

        private void ClearHeldItem()
        {
            HeldItem = null;
            ghost.Clear();
            UpdatePrompt(null);
            UpdateItemInfo(null);
        }
    }
}