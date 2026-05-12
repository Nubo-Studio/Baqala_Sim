using UnityEngine;
using Core.Events;
using Interaction;

namespace Player.States
{
    public class MovingObjectState : InteractionState
    {
        private readonly IMovable movableObject;
        private float pendingYRotation = 0f;
        private const float RotationStep = 30f;
        private const float gridSize = 0.5f;

        public MovingObjectState(Interactor context, InteractionEvents events, IMovable obj)
            : base(context, events)
        {
            movableObject = obj;
        }

        public override void Enter()
        {
            events.RaisePromptChanged(null);
            events.RaiseHeldStateChanged(true);
            events.RaiseHoldTextChanged("إضغط E للوضع\nإضغط Q للإلغاء\nZ دوران يميناً \n X دوران يساراً");
            movableObject.StartMoving(ctx.gameObject);
            ctx.Ghost.CreateGhost(movableObject.Transform.gameObject);
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
            LayerMask placementMask = movableObject.PlacementMask;
            float maxDistance = movableObject.MaxPlacementDistance;

            if (Physics.Raycast(ctx.PlayerCamera.transform.position, ctx.PlayerCamera.transform.forward, out RaycastHit hit, maxDistance, placementMask))
            {
                if (movableObject is MonoBehaviour mono && mono.TryGetComponent(out Systems.Store.MovableObject obj))
                {
                    bool valid = obj.ValidatePlacement(hit.point, hit.normal, hit.collider.gameObject.layer, out Vector3 pos, out Quaternion rot);

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
            LayerMask placementMask = movableObject.PlacementMask;
            float maxDistance = movableObject.MaxPlacementDistance;

            if (Physics.Raycast(ctx.PlayerCamera.transform.position, ctx.PlayerCamera.transform.forward, out RaycastHit hit, maxDistance, placementMask))
            {
                if (movableObject is MonoBehaviour mono && mono.TryGetComponent(out Systems.Store.MovableObject obj))
                {
                    if (obj.ValidatePlacement(hit.point, hit.normal, hit.collider.gameObject.layer, out Vector3 pos, out Quaternion rot))
                    {
                        pos = SnapToGrid(pos, gridSize);
                        Quaternion rotatedRot = rot * Quaternion.Euler(0f, pendingYRotation, 0f);
                        movableObject.MoveTo(pos, rotatedRot);
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
            movableObject.CancelMoving();
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
