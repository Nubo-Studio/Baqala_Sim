using UnityEngine;
using Systems.Store;

namespace Systems.Customers.States
{
    public class LeavingState : CustomerBaseState
    {
        public LeavingState(CustomerAI customer) : base(customer) { }

        public override void Enter()
        {
            Transform exit = StoreManager.Instance.ExitPoint;
            if (exit != null)
            {
                customer.Agent.SetDestination(exit.position);
            }
            else
            {
                // Nowhere to go? Just vanish
                GameObject.Destroy(customer.gameObject);
            }
        }

        public override void Update()
        {
            if (customer.HasReachedDestination())
            {
                GameObject.Destroy(customer.gameObject);
            }
        }
    }
}
