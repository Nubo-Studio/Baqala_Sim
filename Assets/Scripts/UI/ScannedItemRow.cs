using UnityEngine;
using RTLTMPro;

namespace UI
{
    public class ScannedItemRow : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro nameText;
        [SerializeField] private RTLTextMeshPro countText;
        [SerializeField] private RTLTextMeshPro priceText;
        [SerializeField] private RTLTextMeshPro totalText;

        public void Setup(string name, int count, float price, float total)
        {
            if (nameText) nameText.text = name;
            UpdateValues(count, price, total);
        }

        public void UpdateValues(int count, float price, float total)
        {
            if (countText) countText.text = count.ToString();
            if (priceText) priceText.text = price.ToString("0.00");
            if (totalText) totalText.text = total.ToString("0.00");
        }
    }
}
