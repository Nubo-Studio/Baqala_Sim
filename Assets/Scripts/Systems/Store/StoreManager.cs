using System.Collections.Generic;
using UnityEngine;

namespace Systems.Store
{
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager Instance { get; private set; }

        [Header("Store Locations")]
        [SerializeField] private List<Shelf> shelves = new List<Shelf>();
        [SerializeField] private List<Transform> checkoutPoints = new List<Transform>();
        [SerializeField] private Transform exitPoint;

        public IReadOnlyList<Shelf> Shelves => shelves;
        public IReadOnlyList<Transform> CheckoutPoints => checkoutPoints;
        public Transform ExitPoint => exitPoint;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Auto-discover shelves if the list is empty
            if (shelves.Count == 0)
            {
                shelves.AddRange(FindObjectsByType<Shelf>(FindObjectsSortMode.None));
            }
        }

        public Shelf GetRandomShelf()
        {
            if (shelves == null || shelves.Count == 0) return null;
            return shelves[Random.Range(0, shelves.Count)];
        }

        public Transform GetRandomCheckoutPoint()
        {
            if (checkoutPoints == null || checkoutPoints.Count == 0) return null;
            return checkoutPoints[Random.Range(0, checkoutPoints.Count)];
        }
    }
}
