using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using DG.Tweening;
using Core.Events;
using Systems.Items;

namespace UI
{
    public class InteractionUI : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private InteractionEvents events;

        [Header("Prompts")]
        [SerializeField] private CanvasGroup promptCanvas;
        [SerializeField] private RTLTextMeshPro promptText;

        [Header("Holding Instructions")]
        [SerializeField] private CanvasGroup holdCanvas;
        [SerializeField] private RTLTextMeshPro holdText;

        [Header("Item Info Card")]
        [SerializeField] private CanvasGroup infoCanvas;
        [SerializeField] private RTLTextMeshPro infoName;
        [SerializeField] private RTLTextMeshPro infoPrice;
        [SerializeField] private Image infoIcon;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.15f;

        private float displayedPrice;
        private string currentPrompt;

        private void Awake()
        {
            // Auto-setup text for RTL
            SetupRTL(promptText);
            SetupRTL(holdText);
            SetupRTL(infoName);
            SetupRTL(infoPrice);

            // Set initial state
            if (promptCanvas) promptCanvas.alpha = 0;
            if (holdCanvas) holdCanvas.alpha = 0;
            if (infoCanvas) infoCanvas.alpha = 0;

            if (holdText) holdText.text = "إضغط E لترك\nإضغط Q للرمي";
        }

        private void OnEnable()
        {
            if (events == null) return;
            events.OnPromptChanged += HandlePrompt;
            events.OnHeldStateChanged += HandleHeldState;
            events.OnItemInfoChanged += HandleItemInfo;
        }

        private void OnDisable()
        {
            if (events == null) return;
            events.OnPromptChanged -= HandlePrompt;
            events.OnHeldStateChanged -= HandleHeldState;
            events.OnItemInfoChanged -= HandleItemInfo;
        }

        private void HandlePrompt(string text)
        {
            if (currentPrompt == text) return;
            currentPrompt = text;

            if (string.IsNullOrEmpty(text))
            {
                Fade(promptCanvas, 0);
            }
            else
            {
                if (promptText) promptText.text = text;
                Fade(promptCanvas, 1);
                if (promptText) promptText.transform.DOPunchScale(Vector3.one * 0.1f, fadeDuration, 1, 0.1f);
            }
        }

        private void HandleHeldState(bool isHolding)
        {
            Fade(holdCanvas, isHolding ? 1 : 0);
        }

        private void HandleItemInfo(ItemData data)
        {
            if (infoCanvas == null) return;

            if (data == null)
            {
                Fade(infoCanvas, 0);
                infoCanvas.transform.DOScale(0.95f, fadeDuration);
            }
            else
            {
                Fade(infoCanvas, 1);
                infoCanvas.transform.DOScale(1f, fadeDuration).SetEase(Ease.OutBack);
                infoCanvas.transform.DOPunchScale(Vector3.one * 0.05f, fadeDuration);

                if (infoName) infoName.text = $"اسم المنتج: {data.itemNameArabic}";
                if (infoIcon)
                {
                    infoIcon.sprite = data.icon;
                    infoIcon.gameObject.SetActive(data.icon != null);
                }

                if (infoPrice)
                {
                    DOTween.To(() => displayedPrice, x => displayedPrice = x, data.sellPrice, 0.5f)
                        .OnUpdate(() => infoPrice.text = $"سعر البيع: {displayedPrice:0.##} ريال");
                }
            }
        }

        private void Fade(CanvasGroup cg, float alpha)
        {
            if (cg == null) return;
            cg.DOKill();
            cg.DOFade(alpha, fadeDuration);
        }

        private void SetupRTL(RTLTextMeshPro text)
        {
            if (text == null) return;
            text.ForceFix = true;
            text.FixTags = true;
            text.Farsi = false;
        }
    }
}