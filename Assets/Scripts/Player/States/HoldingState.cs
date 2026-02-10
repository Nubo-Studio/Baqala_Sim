using UnityEngine;
using Core.Events;
using Systems.Store;
using Systems.Items;

namespace Player.States
{
    public class HoldingState : InteractionState
    {
        public HoldingState(Interactor context, InteractionEvents events) : base(context, events) { }

        public override void Enter()
        {
            events.RaiseHeldStateChanged(true);

            if (ctx.HeldItem is MonoBehaviour mono && mono.TryGetComponent(out PickupableItem item))
            {
                events.RaiseItemInfoChanged(item.Data);
            }
        }

        public override void Update()
        {
            int mask = ctx.InteractMask & ~(1 << 7);

            if (ctx.Raycast(mask, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out Shelf shelf))
                {
                    events.RaisePromptChanged(shelf.PlacementPrompt);

                    bool valid = false;
                    Vector3 pos = hit.point;
                    Quaternion rot = Quaternion.LookRotation(hit.normal);

                    if (ctx.HeldItem is MonoBehaviour mono && mono.TryGetComponent(out PickupableItem item))
                    {
                        valid = shelf.ValidatePlacement(item, hit.point, hit.normal, out pos, out rot);
                        if (pos == Vector3.zero) pos = hit.point;
                    }

                    ctx.Ghost.UpdateGhost(pos, rot, valid, true);
                }
                else
                {
                    events.RaisePromptChanged(null);
                    ctx.Ghost.UpdateGhost(hit.point, Quaternion.LookRotation(hit.normal), false, true);
                }
            }
            else
            {
                events.RaisePromptChanged(null);
                ctx.Ghost.UpdateGhost(Vector3.zero, Quaternion.identity, false, false);
            }
        }

        public override void HandleInput()
        {
            // Attempt to Place
            int mask = ctx.InteractMask & ~(1 << 7);
            if (ctx.Raycast(mask, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out Shelf shelf))
                {
                    if (ctx.HeldItem is MonoBehaviour mono && mono.TryGetComponent(out PickupableItem item))
                    {
                        if (shelf.TryPlaceItem(item, hit.point, hit.normal))
                        {
                            ClearAndExit();
                            return;
                        }
                    }
                }
            }

            // Fallback Drop
            ctx.HeldItem.Drop(Vector3.zero);
            ClearAndExit();
        }

        public void HandleThrow()
        {
            ctx.HeldItem.Drop(ctx.PlayerCamera.transform.forward * ctx.ThrowForce);
            ClearAndExit();
        }

        private void ClearAndExit()
        {
            ctx.SetHeldItem(null);
            ctx.Ghost.Clear();
            ctx.SwitchState(new FreeLookState(ctx, events));
        }

        public override void Exit()
        {
            events.RaiseHeldStateChanged(false);
            events.RaiseItemInfoChanged(null);
            events.RaisePromptChanged(null);
        }
    }
}
