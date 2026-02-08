using UnityEngine;

namespace Interaction
{
    public interface IPickupable : IInteractable
    {
        bool IsHeld { get; }
        void Pickup(Transform holdPoint, GameObject interactor);
        void Drop(Vector3 throwVelocity);
    }
}
