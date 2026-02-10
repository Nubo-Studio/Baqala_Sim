using UnityEngine;
using System;
using System.Collections.Generic;
using Systems.Items;
using Systems.Customers;
using DG.Tweening;

namespace Systems.Store
{
    public class CheckoutCounter : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private List<Transform> dropPoints;
        [SerializeField] private Transform customerStandPoint;
        [SerializeField] private Transform moneySpawnPoint;

        [Header("Animation")]
        [SerializeField] private float placeDuration = 0.3f;

        [Header("Runtime State")]
        [SerializeField] private CustomerAI currentCustomer;
        [SerializeField] private List<PickupableItem> scannedItems = new List<PickupableItem>();
        [SerializeField] private float totalBill = 0f;
        
        // Payment State
        private bool paymentReceived = false;
        private float cashOnCounter = 0f;

        private PickupableItem[] dropPointContents;

        public Transform CustomerStandPoint => customerStandPoint;
        public Transform MoneySpawnPoint => moneySpawnPoint;
        public bool IsOccupied => currentCustomer != null;
        public float TotalBill => totalBill;
        public CustomerAI CurrentCustomer => currentCustomer;
        public bool PaymentReceived => paymentReceived;

        // --- Events for UI ---
        public event Action<PickupableItem> OnItemScanned;
        public event Action<float> OnTotalUpdated;
        public event Action OnTransactionStarted;
        public event Action OnTransactionEnded;
        public event Action<float, float> OnPaymentProcessed; // received, total
        // ---------------------

        private void Awake()
        {
            if (dropPoints != null)
                dropPointContents = new PickupableItem[dropPoints.Count];
        }

        public bool TryOccupy(CustomerAI customer)
        {
            if (IsOccupied) return false;
            currentCustomer = customer;
            
            // Clear previous session data
            scannedItems.Clear();
            totalBill = 0f;
            paymentReceived = false;
            cashOnCounter = 0f;
            
            OnTransactionStarted?.Invoke();
            OnTotalUpdated?.Invoke(0f);
            
            return true;
        }

        public void ReleaseCustomer()
        {
            currentCustomer = null;
            scannedItems.Clear();
            totalBill = 0f;
            OnTransactionEnded?.Invoke();
        }

        public int GetFreeDropPointIndex()
        {
            for (int i = 0; i < dropPointContents.Length; i++)
            {
                if (dropPointContents[i] == null) return i;
            }
            return -1;
        }

        public void PlaceItemOnCounter(PickupableItem item, int index)
        {
            if (index < 0 || index >= dropPoints.Count) return;

            dropPointContents[index] = item;
            
            item.SetScannable(this);
            item.gameObject.SetActive(true);
            
            BoxCollider itemCol = item.GetComponent<BoxCollider>();
            float yOffset = 0f;
            if (itemCol != null)
            {
                yOffset = Mathf.Abs((itemCol.size.y * item.transform.localScale.y * 0.5f) - (itemCol.center.y * item.transform.localScale.y));
            }

            Vector3 targetPos = dropPoints[index].position + (dropPoints[index].up * (yOffset + 0.005f));
            Quaternion targetRot = dropPoints[index].rotation;

            item.transform.SetParent(null);
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb) 
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            item.transform.DOKill();
            item.transform.DOMove(targetPos, placeDuration).SetEase(Ease.OutQuad);
            item.transform.DORotateQuaternion(targetRot, placeDuration).SetEase(Ease.OutQuad);
        }

        public bool TryScanItem(PickupableItem item)
        {
            for (int i = 0; i < dropPointContents.Length; i++)
            {
                if (dropPointContents[i] == item)
                {
                    totalBill += item.Data.sellPrice;
                    scannedItems.Add(item);
                    dropPointContents[i] = null;
                    
                    item.ClearScannable();
                    item.gameObject.SetActive(false); 
                    
                    OnItemScanned?.Invoke(item);
                    OnTotalUpdated?.Invoke(totalBill);
                    return true;
                }
            }
            return false;
        }
        
        public bool IsAllScanned()
        {
            if (currentCustomer == null) return true;
            if (currentCustomer.ItemsInBasket.Count > 0) return false;
            
            foreach (var item in dropPointContents)
            {
                if (item != null) return false;
            }
            return true;
        }

        // Called by Player picking up the cash object
        public void ProcessPayment(float amount)
        {
            cashOnCounter = amount;
            paymentReceived = true;
            OnPaymentProcessed?.Invoke(amount, totalBill);
            
            // Logic to give change could be handled here or by player interaction
            float change = amount - totalBill;
            if (change > 0)
            {
                Debug.Log($"[Counter] Change due: {change}");
                // In future: Spawn "Change" object for customer to take?
            }
        }
    }
}
