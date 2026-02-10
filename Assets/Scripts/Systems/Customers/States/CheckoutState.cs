using UnityEngine;
using Systems.Store;
using Systems.Items;
using DG.Tweening;

namespace Systems.Customers.States
{
    public class CheckoutState : CustomerBaseState
    {
        private CheckoutCounter counter;
        private bool isAtCounter = false;
        private float actionTimer = 0f;
        private float placeItemInterval = 1.0f;

        private bool waitingForPayment = false;
        private bool hasPaid = false;

        public CheckoutState(CustomerAI customer) : base(customer) { }

        public override void Enter()
        {
            counter = StoreManager.Instance.CheckoutCounter;
            
            if (counter != null && counter.CustomerStandPoint != null)
            {
                customer.Agent.SetDestination(counter.CustomerStandPoint.position);
            }
            else
            {
                customer.SwitchState(new LeavingState(customer));
            }
        }

        public override void Update()
        {
            if (!isAtCounter)
            {
                if (customer.HasReachedDestination())
                {
                    if (counter.TryOccupy(customer))
                    {
                        isAtCounter = true;
                        if (counter.CustomerStandPoint != null)
                        {
                            customer.transform.DORotateQuaternion(counter.CustomerStandPoint.rotation, 0.5f);
                        }
                    }
                }
                return;
            }

            if (customer.ItemsInBasket.Count > 0)
            {
                actionTimer += Time.deltaTime;
                if (actionTimer >= placeItemInterval)
                {
                    int freeSlot = counter.GetFreeDropPointIndex();
                    if (freeSlot != -1)
                    {
                        PickupableItem itemToPlace = customer.ItemsInBasket[0];
                        customer.ItemsInBasket.RemoveAt(0);
                        counter.PlaceItemOnCounter(itemToPlace, freeSlot);
                        actionTimer = 0f;
                    }
                }
            }
            else if (!hasPaid)
            {
                if (counter.IsAllScanned())
                {
                     if (!waitingForPayment)
                     {
                         waitingForPayment = true;
                         SpawnCash();
                     }
                     
                     if (counter.PaymentReceived)
                     {
                         hasPaid = true;
                         Debug.Log("[Customer] Payment accepted. Leaving...");
                     }
                }
            }
            else
            {
                counter.ReleaseCustomer();
                customer.SwitchState(new LeavingState(customer));
            }
        }

        private void SpawnCash()
        {
            float amountToPay = Mathf.Ceil(counter.TotalBill);
            if (amountToPay < counter.TotalBill) amountToPay = counter.TotalBill; 

            StoreManager.Instance.SpawnCash(amountToPay, counter.MoneySpawnPoint);
        }
    }
}
