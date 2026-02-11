using UnityEngine;
using Systems.Store;
using Systems.Items;

namespace Systems.Customers.States
{
    public class PickingItemState : CustomerBaseState
    {
        private Shelf shelf;
        private float pickDelay = 1.0f;
        private float timer;

        public PickingItemState(CustomerAI customer, Shelf shelf) : base(customer)
        {
            this.shelf = shelf;
        }

        public override void Enter()
        {
            timer = 0;
            customer.transform.LookAt(new Vector3(shelf.transform.position.x, customer.transform.position.y, shelf.transform.position.z));
        }

        public override void Update()
        {
            timer += Time.deltaTime;
            if (timer >= pickDelay)
            {
                GrabItem();
            }
        }

        private void GrabItem()
        {
            PickupableItem item = shelf.GetRandomItem();
            if (item != null)
            {
                shelf.ReleaseItem(item);
                
                // Add to customer inventory
                customer.AddItemToBasket(item);
                
                Debug.Log($"[Customer] Picked up {item.Data.itemNameArabic}. Basket: {customer.ItemsInBasket.Count}/{customer.TargetItemCount}");
                
                if (customer.ItemsInBasket.Count >= customer.TargetItemCount)
                {
                    customer.SwitchState(new CheckoutState(customer));
                }
                else
                {
                    // Either stay at this shelf if it has items, or browse for another shelf
                    if (shelf.HasItems && Random.value > 0.3f)
                    {
                        timer = 0; // Pick another from the same shelf after delay
                    }
                    else
                    {
                        customer.SwitchState(new BrowsingState(customer));
                    }
                }
            }
            else
            {
                // Current shelf is empty, check if there are any other items in the store
                if (StoreManager.Instance.HasAnyItems())
                {
                    customer.SwitchState(new BrowsingState(customer));
                }
                else
                {
                    // No items left in the entire store
                    if (customer.ItemsInBasket.Count > 0)
                    {
                        customer.SwitchState(new CheckoutState(customer));
                    }
                    else
                    {
                        customer.SwitchState(new LeavingState(customer));
                    }
                }
            }
        }
    }
}
