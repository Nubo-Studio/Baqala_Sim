using UnityEngine;
using Core.Events;
using Interaction;
using Systems.Items;

namespace Player.States
{
    public class FreeLookState : InteractionState
    {
        public FreeLookState(Interactor context, InteractionEvents events) : base(context, events) { }

        public override void Enter()
        {
            events.RaiseHeldStateChanged(false);
            events.RaiseItemInfoChanged(null);
        }

        public override void Update()
        {
            if (ctx.Raycast(ctx.InteractMask, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    ctx.SetTarget(interactable);
                    events.RaisePromptChanged(interactable.Prompt);
                    return;
                }
            }

            ctx.SetTarget(null);
            events.RaisePromptChanged(null);
        }

        public override void HandleInput()
        {
            if (ctx.CurrentTarget != null && ctx.CurrentTarget.CanInteract(ctx.gameObject))
            {
                // SPECIAL CASE: If it is a scannable item, scan it instead of picking it up
                if (ctx.CurrentTarget is PickupableItem item && item.IsScannable)
                {
                    item.Interact(ctx.gameObject);
                    return;
                }

                if (ctx.CurrentTarget is IPickupable pickupable)
                {
                    pickupable.Pickup(ctx.HoldPoint, ctx.gameObject);
                    ctx.SetHeldItem(pickupable);

                    if (pickupable is MonoBehaviour mono && mono.TryGetComponent(out PickupableItem pItem))
                    {
                        ctx.Ghost.CreateGhost(mono.gameObject);
                    }

                    ctx.SwitchState(new HoldingState(ctx, events));
                }
                else
                {
                    ctx.CurrentTarget.Interact(ctx.gameObject);
                }
            }
        }

        public override void Exit() { }
    }
}
