using System;
using UnityEngine;
using Systems.Items;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Baqala/Events/Interaction Events")]
    public class InteractionEvents : ScriptableObject
    {
        // Invoked when the player hovers over something interactable (e.g. "Pickup Milk")
        public event Action<string> OnPromptChanged;
        
        // Invoked when the player holds an item (to show "Drop/Throw" hints)
        public event Action<bool> OnHeldStateChanged;
        
        // Invoked to show/hide the Item Info Card (Name, Price)
        public event Action<ItemData> OnItemInfoChanged;

        public void RaisePromptChanged(string prompt) => OnPromptChanged?.Invoke(prompt);
        public void RaiseHeldStateChanged(bool isHolding) => OnHeldStateChanged?.Invoke(isHolding);
        public void RaiseItemInfoChanged(ItemData data) => OnItemInfoChanged?.Invoke(data);
    }
}