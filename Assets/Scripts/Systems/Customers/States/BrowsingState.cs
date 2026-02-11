using UnityEngine;
using Systems.Store;

namespace Systems.Customers.States
{
    public class BrowsingState : CustomerBaseState
    {
        private Shelf targetShelf;
        private float browseTimer;
        private float maxBrowseTime = 3f;

        public BrowsingState(CustomerAI customer) : base(customer) { }

        public override void Enter()
        {
            FindNextShelf();
        }

        private void FindNextShelf()
        {
            if (!StoreManager.Instance.HasAnyItems())
            {
                if (customer.ItemsInBasket.Count > 0)
                {
                    customer.SwitchState(new CheckoutState(customer));
                }
                else
                {
                    customer.SwitchState(new LeavingState(customer));
                }
                return;
            }

            targetShelf = StoreManager.Instance.GetRandomShelf();
            
            if (targetShelf != null)
            {
                customer.Agent.SetDestination(targetShelf.transform.position);
            }
            else
            {
                customer.SwitchState(new LeavingState(customer));
            }
        }

        public override void Update()
        {
            if (customer.HasReachedDestination())
            {
                browseTimer += Time.deltaTime;
                if (browseTimer >= maxBrowseTime)
                {
                    if (targetShelf != null && targetShelf.HasItems)
                    {
                        customer.SwitchState(new PickingItemState(customer, targetShelf));
                    }
                    else
                    {
                        // Shelf is empty or null, try another one or leave
                        FindNextShelf();
                    }
                }
            }
        }
    }
}
