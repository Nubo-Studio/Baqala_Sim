using UnityEngine;
using Core.Events;
using Interaction;
using Player.States;

namespace Player
{
    [DisallowMultipleComponent]
    public class Interactor : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InteractionEvents events;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform holdPoint;
        
        [Header("Settings")]
        [SerializeField] private float range = 4f;
        [SerializeField] private LayerMask interactMask;
        [SerializeField] private float throwForce = 6f;

        [Header("Visuals")]
        [SerializeField] private Material validGhostMat;
        [SerializeField] private Material invalidGhostMat;

        private InteractionState currentState;
        private PlacementGhost ghost;

        // Public Accessors for States
        public Camera PlayerCamera => playerCamera;
        public Transform HoldPoint => holdPoint;
        public LayerMask InteractMask => interactMask;
        public float ThrowForce => throwForce;
        public PlacementGhost Ghost => ghost;
        public IPickupable HeldItem { get; private set; }
        public IInteractable CurrentTarget { get; private set; }

        private void Awake()
        {
            ghost = GetComponent<PlacementGhost>();
            if (ghost == null) ghost = gameObject.AddComponent<PlacementGhost>();
        }

        private void Start()
        {
            if (playerCamera == null) playerCamera = Camera.main;
            ghost.Initialize(validGhostMat, invalidGhostMat);

            if (inputReader != null)
            {
                inputReader.OnInteractPressed += OnInteract;
                inputReader.OnThrowPressed += OnThrow;
            }

            SwitchState(new FreeLookState(this, events));
        }

        private void OnDestroy()
        {
            if (inputReader != null)
            {
                inputReader.OnInteractPressed -= OnInteract;
                inputReader.OnThrowPressed -= OnThrow;
            }
        }

        private void Update()
        {
            currentState?.Update();
        }

        private void OnInteract() => currentState?.HandleInput();
        
        private void OnThrow()
        {
            if (currentState is HoldingState holdingState)
            {
                holdingState.HandleThrow();
            }
            else if (currentState is MovingShelfState movingShelfState)
            {
                movingShelfState.HandleCancel();
            }
        }

        public void SwitchState(InteractionState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        // Helper methods for States
        public bool Raycast(LayerMask mask, out RaycastHit hit)
        {
            return Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range, mask);
        }

        public void SetHeldItem(IPickupable item) => HeldItem = item;
        public void SetTarget(IInteractable target) => CurrentTarget = target;
    }
}