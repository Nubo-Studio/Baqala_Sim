using UnityEngine;
using Systems.Store;

namespace Systems.Customers.States
{
    public class CheckoutState : CustomerBaseState
    {
        private Transform checkoutPoint;
        private float waitTime = 2.0f;
        private float timer;

        public CheckoutState(CustomerAI customer) : base(customer) { }

        public override void Enter()
        {
            checkoutPoint = StoreManager.Instance.GetRandomCheckoutPoint();
            if (checkoutPoint != null)
            {
                customer.Agent.SetDestination(checkoutPoint.position);
            }
            else
            {
                // No checkout? Just leave
                customer.SwitchState(new LeavingState(customer));
            }
        }

        public override void Update()
        {
            if (customer.HasReachedDestination())
            {
                timer += Time.deltaTime;
                if (timer >= waitTime)
                {
                    Debug.Log("[Customer] Finished Checkout. Leaving...");
                    customer.SwitchState(new LeavingState(customer));
                }
            }
        }
    }
}
