using System.Collections.Generic;
using UnityEngine;

namespace Systems.Store
{
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager Instance { get; private set; }

        [Header("Store Locations")]
        [SerializeField] private List<Shelf> shelves = new List<Shelf>();
        [SerializeField] private CheckoutCounter checkoutCounter;
        [SerializeField] private Transform exitPoint;
        
        [Header("Prefabs")]
        [SerializeField] private Cash cashPrefab;

        public IReadOnlyList<Shelf> Shelves => shelves;
        public CheckoutCounter CheckoutCounter => checkoutCounter;
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

            if (shelves.Count == 0)
            {
                shelves.AddRange(FindObjectsByType<Shelf>(FindObjectsSortMode.None));
            }

            if (checkoutCounter == null)
                checkoutCounter = FindAnyObjectByType<CheckoutCounter>();
        }

        public Shelf GetRandomShelf()
        {
            if (shelves == null || shelves.Count == 0) return null;
            return shelves[Random.Range(0, shelves.Count)];
        }

        public void SpawnCash(float amount, Transform spawnPoint)
        {
            if (cashPrefab != null && spawnPoint != null)
            {
                Cash newCash = Instantiate(cashPrefab, spawnPoint.position, spawnPoint.rotation);
                newCash.Setup(amount);
            }
            else
            {
                Debug.LogWarning("[StoreManager] Cannot spawn cash! Missing Prefab or SpawnPoint.");
                // Fallback: auto-process
                checkoutCounter.ProcessPayment(amount);
            }
        }
    }
}
