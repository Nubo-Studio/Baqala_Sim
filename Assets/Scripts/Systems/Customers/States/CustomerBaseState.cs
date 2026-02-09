using UnityEngine;

namespace Systems.Customers.States
{
    public abstract class CustomerBaseState
    {
        protected CustomerAI customer;

        public CustomerBaseState(CustomerAI customer)
        {
            this.customer = customer;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void PhysicsUpdate() { }
        public virtual void Exit() { }
    }
}
