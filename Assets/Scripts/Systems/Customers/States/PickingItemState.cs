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
            // Look at the shelf
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
                // Simple visual "pickup"
                shelf.ReleaseItem(item);
                
                // For now, we just disable the item to simulate it being in a basket
                // In a future step, we can parent it to the customer hand
                item.gameObject.SetActive(false);
                
                Debug.Log($"[Customer] Picked up {item.Data.itemNameArabic}");
                
                // After picking, go to checkout
                customer.SwitchState(new CheckoutState(customer));
            }
            else
            {
                // Someone took it first! Back to browsing
                customer.SwitchState(new BrowsingState(customer));
            }
        }
    }
}
