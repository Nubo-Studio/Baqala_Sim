using System;
using UnityEngine;
using Systems.Items;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Baqala/Events/Interaction Events")]
    public class InteractionEvents : ScriptableObject
    {
        public event Action<string> OnPromptChanged;
        
        public event Action<bool> OnHeldStateChanged;
        
        public event Action<ItemData> OnItemInfoChanged;

        public event Action<string> OnHoldTextChanged;

        public void RaisePromptChanged(string prompt) => OnPromptChanged?.Invoke(prompt);
        public void RaiseHeldStateChanged(bool isHolding) => OnHeldStateChanged?.Invoke(isHolding);
        public void RaiseItemInfoChanged(ItemData data) => OnItemInfoChanged?.Invoke(data);
        public void RaiseHoldTextChanged(string text) => OnHoldTextChanged?.Invoke(text);
    }
}