using UnityEngine;
using UnityEngine.AI;
using Systems.Customers.States;

namespace Systems.Customers
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CustomerAI : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Where the customer goes immediately after spawning.")]
        [SerializeField] private Transform storeEntrance;

        [Header("Debug")]
        [SerializeField] private string currentStateName;

        private NavMeshAgent agent;
        private CustomerBaseState currentState;

        // Public Accessors
        public NavMeshAgent Agent => agent;
        public Transform StoreEntrance => storeEntrance;

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
            // Start in the Entering State
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
    }
}
