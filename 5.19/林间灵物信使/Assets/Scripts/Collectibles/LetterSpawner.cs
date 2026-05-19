using UnityEngine;
using System.Collections.Generic;
using ForestMessenger.Collectibles;

namespace ForestMessenger.Tools
{
    public class LetterSpawner : MonoBehaviour
    {
        [Header("信件设置")]
        [SerializeField] private List<LetterData> letterPool = new List<LetterData>();
        [SerializeField] private GameObject letterPrefab;
        [SerializeField] private bool randomizeLetters = true;

        [Header("生成设置")]
        [SerializeField] private int letterCount = 10;
        [SerializeField] private float spawnRadius = 15f;
        [SerializeField] private Vector2 spawnCenter = Vector2.zero;
        [SerializeField] private float minY = 0f;
        [SerializeField] private float maxY = 3f;
        [SerializeField] private bool spawnOnStart = true;

        [Header("颜色设置")]
        [SerializeField] private bool useDifferentColors = true;
        [SerializeField] private List<Color> letterColors = new List<Color>
        {
            new Color(1f, 0.95f, 0.8f),
            new Color(1f, 0.85f, 0.6f),
            new Color(0.9f, 1f, 0.85f),
            new Color(0.85f, 0.95f, 1f),
            new Color(1f, 0.85f, 0.9f),
            new Color(0.95f, 0.85f, 1f)
        };

        private List<GameObject> spawnedLetters = new List<GameObject>();

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnLetters();
            }
        }

        [ContextMenu("生成信件")]
        public void SpawnLetters()
        {
            ClearLetters();

            for (int i = 0; i < letterCount; i++)
            {
                SpawnSingleLetter(i);
            }

            Debug.Log($"【信件系统】已生成 {letterCount} 封信件");
        }

        private void SpawnSingleLetter(int index)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject letterObj;

            if (letterPrefab != null)
            {
                letterObj = Instantiate(letterPrefab, spawnPos, Quaternion.identity, transform);
            }
            else
            {
                letterObj = new GameObject($"Letter_{index:00}");
                letterObj.transform.SetParent(transform);
                letterObj.transform.position = spawnPos;
            }

            LetterPickup pickup = letterObj.GetComponent<LetterPickup>();
            if (pickup == null)
            {
                pickup = letterObj.AddComponent<LetterPickup>();
            }

            if (letterPool.Count > 0)
            {
                if (randomizeLetters)
                {
                    pickup.SetValue("_letterData", letterPool[Random.Range(0, letterPool.Count)]);
                }
                else
                {
                    pickup.SetValue("_letterData", letterPool[index % letterPool.Count]);
                }
            }

            if (useDifferentColors && letterColors.Count > 0)
            {
                SpriteRenderer renderer = letterObj.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = letterColors[index % letterColors.Count];
                }
            }

            spawnedLetters.Add(letterObj);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float distance = Random.Range(0f, spawnRadius);
            float x = spawnCenter.x + Mathf.Cos(angle) * distance;
            float y = Random.Range(minY, maxY);

            return new Vector3(x, y, 0f);
        }

        [ContextMenu("清除所有信件")]
        public void ClearLetters()
        {
            foreach (var letter in spawnedLetters)
            {
                if (letter != null)
                {
                    DestroyImmediate(letter);
                }
            }
            spawnedLetters.Clear();

            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public void AddLetterToPool(LetterData letterData)
        {
            if (!letterPool.Contains(letterData))
            {
                letterPool.Add(letterData);
            }
        }

        public void RemoveLetterFromPool(LetterData letterData)
        {
            letterPool.Remove(letterData);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
        }
    }

    public static class LetterPickupExtensions
    {
        public static void SetValue(this LetterPickup pickup, string fieldName, LetterData data)
        {
            var field = typeof(LetterPickup).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(pickup, data);
        }
    }
}
