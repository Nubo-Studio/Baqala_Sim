using UnityEngine;

namespace Systems.Items
{
    [CreateAssetMenu(fileName = "New Item Data", menuName = "Baqala/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Product Name in Arabic.")]
        public string itemNameArabic = "اسم المنتج";
        
        [Header("Pricing")]
        [Tooltip("The price at which the product is sold to customers.")]
        public float sellPrice = 0.0f;
        
        [Tooltip("The price paid to the supplier for this item.")]
        public float costPrice = 0.0f;

        [Header("Classification")]
        [Tooltip("Product category (e.g., Dairy, Snacks).")]
        public string category = "General";

        [Header("Visuals")]
        [Tooltip("Icon representing the product in UI.")]
        public Sprite icon;
    }
}