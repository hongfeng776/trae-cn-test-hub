using UnityEngine;
using System.Collections.Generic;

namespace ForestMessenger.Environment
{
    public class ForestGenerator : MonoBehaviour
    {
        [Header("地形设置")]
        [SerializeField] private GameObject groundPrefab;
        [SerializeField] private int groundWidth = 50;
        [SerializeField] private int groundHeight = 1;

        [Header("植被设置")]
        [SerializeField] private List<GameObject> treePrefabs;
        [SerializeField] private List<GameObject> bushPrefabs;
        [SerializeField] private List<GameObject> grassPrefabs;
        [SerializeField] private int treeCount = 20;
        [SerializeField] private int bushCount = 30;
        [SerializeField] private int grassCount = 50;

        [Header("生成范围")]
        [SerializeField] private float spawnRangeX = 45f;
        [SerializeField] private float spawnRangeY = 0f;

        private Transform groundParent;
        private Transform vegetationParent;

        private void Awake()
        {
            CreateParentObjects();
        }

        private void Start()
        {
            GenerateGround();
            GenerateVegetation();
        }

        private void CreateParentObjects()
        {
            groundParent = new GameObject("Ground").transform;
            groundParent.SetParent(transform);

            vegetationParent = new GameObject("Vegetation").transform;
            vegetationParent.SetParent(transform);
        }

        private void GenerateGround()
        {
            if (groundPrefab == null)
            {
                CreateDefaultGround();
                return;
            }

            for (int x = 0; x < groundWidth; x++)
            {
                Vector3 position = new Vector3(x - groundWidth / 2f, 0f, 0f);
                Instantiate(groundPrefab, position, Quaternion.identity, groundParent);
            }
        }

        private void CreateDefaultGround()
        {
            GameObject ground = new GameObject("GroundPlatform");
            ground.transform.SetParent(groundParent);
            ground.transform.position = Vector3.zero;

            SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>();
            renderer.color = new Color(0.4f, 0.26f, 0.13f);

            BoxCollider2D collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(groundWidth, groundHeight);
            collider.offset = new Vector2(0f, 0f);

            ground.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        private void GenerateVegetation()
        {
            for (int i = 0; i < treeCount; i++)
            {
                SpawnRandomVegetation(treePrefabs, 2f, 5f);
            }

            for (int i = 0; i < bushCount; i++)
            {
                SpawnRandomVegetation(bushPrefabs, 0.5f, 1f);
            }

            for (int i = 0; i < grassCount; i++)
            {
                SpawnRandomVegetation(grassPrefabs, 0.2f, 0.5f);
            }
        }

        private void SpawnRandomVegetation(List<GameObject> prefabs, float minScale, float maxScale)
        {
            if (prefabs == null || prefabs.Count == 0) return;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            Vector3 position = new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                spawnRangeY + Random.Range(-0.5f, 0.5f),
                Random.Range(-1f, 1f)
            );

            float scale = Random.Range(minScale, maxScale);
            GameObject instance = Instantiate(prefab, position, Quaternion.identity, vegetationParent);
            instance.transform.localScale = Vector3.one * scale;
        }

        public void ClearForest()
        {
            if (groundParent != null)
            {
                foreach (Transform child in groundParent)
                {
                    Destroy(child.gameObject);
                }
            }

            if (vegetationParent != null)
            {
                foreach (Transform child in vegetationParent)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
