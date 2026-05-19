using UnityEngine;
using System.Collections.Generic;

namespace ForestMessenger.Collectibles
{
    public class LetterGenerator : MonoBehaviour
    {
        [Header("生成设置")]
        [SerializeField] private GameObject letterPrefab;
        [SerializeField] private List<LetterData> letterDatas = new List<LetterData>();
        [SerializeField] private int letterCount = 10;
        [SerializeField] private bool generateOnStart = true;

        [Header("生成范围")]
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-20f, -5f);
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(20f, 5f);
        [SerializeField] private float minDistanceBetweenLetters = 2f;
        [SerializeField] private LayerMask obstacleLayers;

        [Header("稀有度设置")]
        [Range(0f, 1f)]
        [SerializeField] private float normalLetterChance = 0.6f;
        [Range(0f, 1f)]
        [SerializeField] private float rareLetterChance = 0.3f;
        [Range(0f, 1f)]
        [SerializeField] private float legendaryLetterChance = 0.1f;

        [Header("视觉")]
        [SerializeField] private bool showGizmos = true;

        private List<Vector3> spawnPositions = new List<Vector3>();
        private List<GameObject> spawnedLetters = new List<GameObject>();

        public int SpawnedCount => spawnedLetters.Count;

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateLetters();
            }
        }

        [ContextMenu("生成信件")]
        public void GenerateLetters()
        {
            ClearLetters();

            Debug.Log($"📨 开始生成 {letterCount} 封信件...");

            for (int i = 0; i < letterCount; i++)
            {
                Vector3 position = GetValidSpawnPosition();
                if (position != Vector3.zero)
                {
                    SpawnLetter(position);
                    spawnPositions.Add(position);
                }
            }

            Debug.Log($"✅ 信件生成完成！共生成 {spawnedLetters.Count} 封");
        }

        [ContextMenu("清除所有信件")]
        public void ClearLetters()
        {
            foreach (GameObject letter in spawnedLetters)
            {
                if (letter != null)
                {
                    DestroyImmediate(letter);
                }
            }

            spawnedLetters.Clear();
            spawnPositions.Clear();
        }

        private Vector3 GetValidSpawnPosition()
        {
            int maxAttempts = 50;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                    0f
                );

                if (IsValidPosition(randomPos))
                {
                    return randomPos;
                }
            }

            Debug.LogWarning("【信件生成】找不到有效生成位置！");
            return Vector3.zero;
        }

        private bool IsValidPosition(Vector3 position)
        {
            foreach (Vector3 existingPos in spawnPositions)
            {
                if (Vector3.Distance(position, existingPos) < minDistanceBetweenLetters)
                {
                    return false;
                }
            }

            Collider2D hit = Physics2D.OverlapCircle(position, 0.5f, obstacleLayers);
            if (hit != null)
            {
                return false;
            }

            return true;
        }

        private void SpawnLetter(Vector3 position)
        {
            GameObject letterObj;

            if (letterPrefab != null)
            {
                letterObj = Instantiate(letterPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                letterObj = CreateDefaultLetterObject(position);
            }

            LetterPickup letterPickup = letterObj.GetComponent<LetterPickup>();
            if (letterPickup != null && letterDatas.Count > 0)
            {
                LetterData randomLetter = GetRandomLetterData();
                letterPickup.SetLetterData(randomLetter);
            }

            spawnedLetters.Add(letterObj);
        }

        private GameObject CreateDefaultLetterObject(Vector3 position)
        {
            GameObject letterObj = new GameObject("Letter");
            letterObj.transform.position = position;
            letterObj.transform.parent = transform;

            SpriteRenderer renderer = letterObj.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(32, 32);
            Color[] colors = tex.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(1f, 0.95f, 0.8f);
            }
            tex.SetPixels(colors);
            tex.Apply();
            renderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);

            BoxCollider2D collider = letterObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.8f, 0.8f);

            letterObj.AddComponent<LetterPickup>();

            return letterObj;
        }

        private LetterData GetRandomLetterData()
        {
            float randomValue = Random.value;
            LetterType targetType;

            if (randomValue < legendaryLetterChance)
            {
                targetType = LetterType.Legendary;
            }
            else if (randomValue < legendaryLetterChance + rareLetterChance)
            {
                targetType = LetterType.Rare;
            }
            else
            {
                targetType = LetterType.Normal;
            }

            List<LetterData> matchingLetters = letterDatas.FindAll(l => l.letterType == targetType);

            if (matchingLetters.Count > 0)
            {
                return matchingLetters[Random.Range(0, matchingLetters.Count)];
            }

            if (letterDatas.Count > 0)
            {
                return letterDatas[Random.Range(0, letterDatas.Count)];
            }

            return null;
        }

        public void AddLetterData(LetterData data)
        {
            if (!letterDatas.Contains(data))
            {
                letterDatas.Add(data);
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Vector3 center = (spawnAreaMin + spawnAreaMax) * 0.5f;
            Vector3 size = spawnAreaMax - spawnAreaMin;

            Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.3f);
            Gizmos.DrawCube(center, size);

            Gizmos.color = new Color(0.2f, 0.8f, 0.4f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
