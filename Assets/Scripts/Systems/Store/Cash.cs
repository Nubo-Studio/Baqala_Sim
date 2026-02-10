using UnityEngine;
using Interaction;

namespace Systems.Store
{
    public class Cash : MonoBehaviour, IInteractable
    {
        [SerializeField] private float amount = 0f;
        [SerializeField] private string prompt = "???? E ???? ?????";

        public float Amount => amount;
        public string Prompt => prompt;

        public void Setup(float value)
        {
            amount = value;
        }

        public bool CanInteract(GameObject interactor) => true;

        public void Interact(GameObject interactor)
        {
            // Logic to take cash handled by Interactor or Cashier System
            // For now, we just destroy it to simulate taking it
            Debug.Log($"Collected {amount} Riyals.");
            
            // Notify UI or Game Manager here if needed
            StoreManager.Instance.CheckoutCounter.ProcessPayment(amount);
            
            Destroy(gameObject);
        }
    }
}
