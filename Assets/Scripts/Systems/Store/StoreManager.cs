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
        [SerializeField] private float offsetPerNote = 0.02f;

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

        public bool HasAnyItems()
        {
            if (shelves == null) return false;
            foreach (var shelf in shelves)
            {
                if (shelf != null && shelf.HasItems) return true;
            }
            return false;
        }

        public void SpawnCash(float totalAmount, Transform spawnPoint)
        {
            if (cashPrefab == null || spawnPoint == null)
            {
                Debug.LogWarning("[StoreManager] Cannot spawn cash! Missing Prefab or SpawnPoint.");
                checkoutCounter.ProcessPayment(totalAmount);
                return;
            }

            // If the total is e.g. 30, and we use "10" as a base note value 
            // OR if we just want to spawn multiple objects for visual effect.
            // Let's assume each note object represents 10 units for now if it's large, 
            // or just split it into notes of max 50.
            
            float remaining = totalAmount;
            int count = 0;
            
            while (remaining > 0)
            {
                float noteValue = remaining >= 50 ? 50 : (remaining >= 20 ? 20 : (remaining >= 10 ? 10 : remaining));
                
                Vector3 spawnPos = spawnPoint.position + (Vector3.up * offsetPerNote * count);
                Cash newCash = Instantiate(cashPrefab, spawnPos, spawnPoint.rotation);
                newCash.Setup(noteValue);
                
                remaining -= noteValue;
                count++;

                // Safety break to prevent infinite loops if noteValue logic fails
                if (count > 20) break;
            }
        }
    }
}
