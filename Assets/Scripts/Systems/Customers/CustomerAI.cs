using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Systems.Customers.States;
using Systems.Items;

namespace Systems.Customers
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CustomerAI : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Where the customer goes immediately after spawning.")]
        [SerializeField] private Transform storeEntrance;

        [Header("Inventory")]
        [SerializeField] private List<PickupableItem> itemsInBasket = new List<PickupableItem>();

        [Header("Debug")]
        [SerializeField] private string currentStateName;

        private NavMeshAgent agent;
        private CustomerBaseState currentState;

        // Public Accessors
        public NavMeshAgent Agent => agent;
        public Transform StoreEntrance => storeEntrance;
        public List<PickupableItem> ItemsInBasket => itemsInBasket;

        public void Initialize(Transform entrance)
        {
            storeEntrance = entrance;
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            SwitchState(new EnteringState(this));
        }

        private void Update()
        {
            currentState?.Update();
        }

        private void FixedUpdate()
        {
            currentState?.PhysicsUpdate();
        }

        public void SwitchState(CustomerBaseState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
            
            currentStateName = currentState != null ? currentState.GetType().Name : "None";
        }

        public bool HasReachedDestination(float stoppingDistance = 0.5f)
        {
            if (agent.pathPending) return false;
            if (agent.remainingDistance > stoppingDistance) return false;
            return agent.hasPath || agent.velocity.sqrMagnitude == 0f;
        }

        public void AddItemToBasket(PickupableItem item)
        {
            if (item != null)
            {
                itemsInBasket.Add(item);
                item.gameObject.SetActive(false); // Hide it for now, pretending it is in a basket
                item.transform.SetParent(transform); // Carry it with us
            }
        }
    }
}
