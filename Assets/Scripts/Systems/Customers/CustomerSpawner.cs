using System.Collections;
using UnityEngine;

namespace Systems.Customers
{
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CustomerAI customerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform storeEntrance;
        
        [Header("Timing")]
        [SerializeField] private float spawnInterval = 10f;

        private void Start()
        {
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                SpawnCustomer();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnCustomer()
        {
            if (customerPrefab == null || spawnPoint == null) return;

            CustomerAI newCustomer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
            newCustomer.Initialize(storeEntrance);
        }
    }
}
