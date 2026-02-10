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
                
                Debug.Log($"[Customer] Picked up {item.Data.itemNameArabic}. Basket Count: {customer.ItemsInBasket.Count}");
                
                // For this prototype, pick 1 item then checkout. 
                // Later we can loop this to pick multiple items.
                customer.SwitchState(new CheckoutState(customer));
            }
            else
            {
                customer.SwitchState(new BrowsingState(customer));
            }
        }
    }
}
