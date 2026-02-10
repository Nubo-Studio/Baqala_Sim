using UnityEngine;
using System.Collections.Generic;
using Systems.Store;
using Systems.Items;
using RTLTMPro;
using DG.Tweening;

namespace UI
{
    public class CashierUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CheckoutCounter counter;
        
        [Header("Screens")]
        [SerializeField] private CanvasGroup mainScreenCanvas;
        [SerializeField] private CanvasGroup paymentScreenCanvas;

        [Header("List UI")]
        [SerializeField] private Transform rowContainer;
        [SerializeField] private ScannedItemRow rowPrefab;
        [SerializeField] private RTLTextMeshPro grandTotalText;

        [Header("Payment UI")]
        [SerializeField] private RTLTextMeshPro paymentTotalText;
        [SerializeField] private RTLTextMeshPro receivedCashText;
        [SerializeField] private RTLTextMeshPro changeText;

        // Runtime Tracking
        private Dictionary<ItemData, ScannedItemRow> activeRows = new Dictionary<ItemData, ScannedItemRow>();
        private Dictionary<ItemData, int> itemCounts = new Dictionary<ItemData, int>();
        private Dictionary<ItemData, float> itemTotals = new Dictionary<ItemData, float>();

        private void Start()
        {
            if (counter == null)
                counter = StoreManager.Instance.CheckoutCounter;

            if (counter != null)
            {
                counter.OnItemScanned += HandleItemScanned;
                counter.OnTotalUpdated += HandleTotalUpdated;
                counter.OnTransactionStarted += HandleTransactionStarted;
                counter.OnTransactionEnded += HandleTransactionEnded;
            }
            
            // Initial State
            ClearList();
            UpdateGrandTotal(0);
            ShowMainScreen();
        }

        private void OnDestroy()
        {
            if (counter != null)
            {
                counter.OnItemScanned -= HandleItemScanned;
                counter.OnTotalUpdated -= HandleTotalUpdated;
                counter.OnTransactionStarted -= HandleTransactionStarted;
                counter.OnTransactionEnded -= HandleTransactionEnded;
            }
        }

        private void HandleTransactionStarted()
        {
            ClearList();
            ShowMainScreen();
        }

        private void HandleTransactionEnded()
        {
            // Delay or immediate? For now, let's keep the last bill visible until next customer
        }

        private void HandleItemScanned(PickupableItem item)
        {
            ItemData data = item.Data;

            if (!itemCounts.ContainsKey(data))
            {
                itemCounts[data] = 0;
                itemTotals[data] = 0f;
            }

            itemCounts[data]++;
            itemTotals[data] += data.sellPrice;

            if (activeRows.ContainsKey(data))
            {
                // Update existing row
                activeRows[data].UpdateValues(itemCounts[data], data.sellPrice, itemTotals[data]);
            }
            else
            {
                // Create new row
                ScannedItemRow newRow = Instantiate(rowPrefab, rowContainer);
                newRow.Setup(data.itemNameArabic, itemCounts[data], data.sellPrice, itemTotals[data]);
                activeRows.Add(data, newRow);
            }
        }

        private void HandleTotalUpdated(float total)
        {
            UpdateGrandTotal(total);
        }

        private void UpdateGrandTotal(float amount)
        {
            if (grandTotalText) grandTotalText.text = amount.ToString("0.00");
        }

        private void ClearList()
        {
            foreach (Transform child in rowContainer)
            {
                Destroy(child.gameObject);
            }
            activeRows.Clear();
            itemCounts.Clear();
            itemTotals.Clear();
            UpdateGrandTotal(0);
        }
        
        // --- Screen Management ---

        private void ShowMainScreen()
        {
            FadeGroup(mainScreenCanvas, 1);
            FadeGroup(paymentScreenCanvas, 0);
        }

        public void ShowPaymentScreen(float received, float total)
        {
            FadeGroup(mainScreenCanvas, 0);
            FadeGroup(paymentScreenCanvas, 1);
            
            if (paymentTotalText) paymentTotalText.text = total.ToString("0.00");
            if (receivedCashText) receivedCashText.text = received.ToString("0.00");
            
            float change = received - total;
            if (change < 0) change = 0;
            if (changeText) changeText.text = change.ToString("0.00");
        }

        private void FadeGroup(CanvasGroup cg, float alpha)
        {
            if (cg == null) return;
            cg.DOKill();
            cg.DOFade(alpha, 0.2f);
            cg.interactable = alpha > 0.5f;
            cg.blocksRaycasts = alpha > 0.5f;
        }
    }
}
