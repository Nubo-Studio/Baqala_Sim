using UnityEngine;

namespace Interaction
{
    public interface IMovable : IInteractable
    {
        Transform Transform { get; }
        LayerMask PlacementMask { get; }
        float MaxPlacementDistance { get; }
        void StartMoving(GameObject interactor);
        void MoveTo(Vector3 position, Quaternion rotation);
        void CancelMoving();
    }
}
