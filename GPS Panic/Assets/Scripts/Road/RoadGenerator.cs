using UnityEngine;
using System.Collections.Generic;

namespace GPSPanic.Road
{
    public class RoadGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject[] chunkPrefabs;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private int initialChunks = 5;
        [SerializeField] private float chunkLength = 20f;
        [SerializeField] private float spawnThreshold = 50f;

        private List<GameObject> activeChunks = new List<GameObject>();
        private float nextSpawnY = 0f;

        private void Start()
        {
            for (int i = 0; i < initialChunks; i++)
            {
                SpawnChunk();
            }
        }

        private void Update()
        {
            if (playerTransform != null && playerTransform.position.y + spawnThreshold > nextSpawnY)
            {
                SpawnChunk();
                RemoveOldChunk();
            }
        }

        private void SpawnChunk()
        {
            if (chunkPrefabs == null || chunkPrefabs.Length == 0) return;

            GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
            GameObject chunk = Instantiate(prefab, new Vector3(0, nextSpawnY, 0), Quaternion.identity, transform);
            activeChunks.Add(chunk);
            nextSpawnY += chunkLength;
        }

        private void RemoveOldChunk()
        {
            if (activeChunks.Count > initialChunks + 2)
            {
                GameObject oldChunk = activeChunks[0];
                activeChunks.RemoveAt(0);
                Destroy(oldChunk);
            }
        }
    }
}
