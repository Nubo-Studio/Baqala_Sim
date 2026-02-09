using UnityEngine;

namespace Systems.Customers.States
{
    public class EnteringState : CustomerBaseState
    {
        public EnteringState(CustomerAI customer) : base(customer) { }

        public override void Enter()
        {
            if (customer.StoreEntrance != null)
            {
                customer.Agent.SetDestination(customer.StoreEntrance.position);
            }
            else
            {
                Debug.LogWarning("[EnteringState] No Store Entrance assigned!");
                customer.SwitchState(new BrowsingState(customer));
            }
        }

        public override void Update()
        {
            if (customer.HasReachedDestination())
            {
                customer.SwitchState(new BrowsingState(customer));
            }
        }
    }
}
