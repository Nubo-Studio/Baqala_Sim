using UnityEngine;
using Core.Events;
using Interaction;

namespace Player.States
{
    public class MovingShelfState : InteractionState
    {
        private readonly IMovable movableShelf;

        public MovingShelfState(Interactor context, InteractionEvents events, IMovable shelf) 
            : base(context, events)
        {
            movableShelf = shelf;
        }

        public override void Enter()
        {
            events.RaisePromptChanged(null);
            events.RaiseHeldStateChanged(true);
            events.RaiseHoldTextChanged("إضغط E للوضع\nإضغط Q للإلغاء");
            movableShelf.StartMoving(ctx.gameObject);
            ctx.Ghost.CreateGhost(movableShelf.Transform.gameObject);
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
                    
                    if (pos != Vector3.zero)
                    {
                        ctx.Ghost.UpdateGhost(pos, rot, valid, true);
                    }
                    else
                    {
                        ctx.Ghost.UpdateGhost(hit.point, Quaternion.LookRotation(hit.normal), false, true);
                    }
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
                        movableShelf.MoveTo(pos, rot);
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

        private void ExitState()
        {
            ctx.Ghost.Clear();
            ctx.SwitchState(new FreeLookState(ctx, events));
        }

        public override void Exit()
        {
            events.RaiseHeldStateChanged(false);
            events.RaiseHoldTextChanged(null);
        }
    }
}
