using UnityEngine;
using Core.Events;
using Interaction;

namespace Player.States
{
    public class MovingShelfState : InteractionState
    {
        private readonly IMovable movableShelf;
        private float pendingYRotation = 0f;
        private const float RotationStep = 30f;
        private const float gridSize = 0.5f;

        public MovingShelfState(Interactor context, InteractionEvents events, IMovable shelf)
            : base(context, events)
        {
            movableShelf = shelf;
        }

        public override void Enter()
        {
            events.RaisePromptChanged(null);
            events.RaiseHeldStateChanged(true);
            events.RaiseHoldTextChanged("إضغط E للوضع\nإضغط Q للإلغاء\nZ دوران يميناً \n X دوران يساراً");
            movableShelf.StartMoving(ctx.gameObject);
            ctx.Ghost.CreateGhost(movableShelf.Transform.gameObject);
            ctx.InputReader.OnRotatePressed += HandleRotate;
            if (GridManager.Instance != null)
            {
                GridManager.Instance.ToggleGrid(true);
            }
        }

        private void HandleRotate(float direction)
        {
            pendingYRotation += direction * RotationStep;
        }

        public override void Update()
        {
            LayerMask placementMask = movableShelf.PlacementMask;
            float maxDistance = movableShelf.MaxPlacementDistance;

            if (Physics.Raycast(ctx.PlayerCamera.transform.position, ctx.PlayerCamera.transform.forward, out RaycastHit hit, maxDistance, placementMask))
            {
                if (movableShelf is MonoBehaviour mono && mono.TryGetComponent(out Systems.Store.MovableShelf shelf))
                {
                    bool valid = shelf.ValidatePlacement(hit.point, hit.normal, hit.collider.gameObject.layer, out Vector3 pos, out Quaternion rot);

                    pos = SnapToGrid(pos, gridSize);
                    Quaternion rotatedRot = rot * Quaternion.Euler(0f, pendingYRotation, 0f);

                    if (pos != Vector3.zero)
                        ctx.Ghost.UpdateGhost(pos, rotatedRot, valid, true);
                    else
                        ctx.Ghost.UpdateGhost(hit.point, rotatedRot, false, true);
                }
                else
                {
                    ctx.Ghost.UpdateGhost(hit.point, Quaternion.LookRotation(hit.normal), false, true);
                }
            }
            else
            {
                ctx.Ghost.UpdateGhost(Vector3.zero, Quaternion.identity, false, false);
            }
        }

        public override void HandleInput()
        {
            LayerMask placementMask = movableShelf.PlacementMask;
            float maxDistance = movableShelf.MaxPlacementDistance;

            if (Physics.Raycast(ctx.PlayerCamera.transform.position, ctx.PlayerCamera.transform.forward, out RaycastHit hit, maxDistance, placementMask))
            {
                if (movableShelf is MonoBehaviour mono && mono.TryGetComponent(out Systems.Store.MovableShelf shelf))
                {
                    if (shelf.ValidatePlacement(hit.point, hit.normal, hit.collider.gameObject.layer, out Vector3 pos, out Quaternion rot))
                    {
                        pos = SnapToGrid(pos, gridSize);
                        Quaternion rotatedRot = rot * Quaternion.Euler(0f, pendingYRotation, 0f);
                        movableShelf.MoveTo(pos, rotatedRot);
                        ExitState();
                        return;
                    }
                }
            }

            CancelMovement();
        }

        public void HandleCancel()
        {
            CancelMovement();
        }

        private void CancelMovement()
        {
            movableShelf.CancelMoving();
            ExitState();
        }

        private Vector3 SnapToGrid(Vector3 position, float gridSize)
        {
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                position.y,
                Mathf.Round(position.z / gridSize) * gridSize
            );
        }


        private void ExitState()
        {
            ctx.Ghost.Clear();
            ctx.SwitchState(new FreeLookState(ctx, events));
        }

        public override void Exit()
        {
            ctx.InputReader.OnRotatePressed -= HandleRotate;
            events.RaiseHeldStateChanged(false);
            events.RaiseHoldTextChanged(null);
            if (GridManager.Instance != null)
            {
                GridManager.Instance.ToggleGrid(false);
            }
        }
    }
}
