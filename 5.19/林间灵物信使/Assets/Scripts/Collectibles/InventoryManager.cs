using UnityEngine;
using System.Collections.Generic;
using ForestMessenger.Core;

namespace ForestMessenger.Collectibles
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        [Header("背包设置")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private string saveKey = "Forest_Letters";

        [Header("收集统计")]
        [SerializeField] private int totalLettersCollected;
        [SerializeField] private int totalPoints;
        [SerializeField] private List<LetterData> collectedLetters = new List<LetterData>();

        public delegate void LetterCollected(LetterData letter, int newTotal);
        public event LetterCollected OnLetterCollected;

        public delegate void InventoryLoaded();
        public event InventoryLoaded OnInventoryLoaded;

        public int TotalLettersCollected => totalLettersCollected;
        public int TotalPoints => totalPoints;
        public int UniqueLettersCollected => collectedLetters.Count;
        public List<LetterData> CollectedLetters => new List<LetterData>(collectedLetters);

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            Debug.Log("📦 背包管理器初始化...");
        }

        private void Start()
        {
            LoadInventory();
        }

        public bool CollectLetter(LetterData letter)
        {
            if (letter == null)
            {
                Debug.LogWarning("【背包】信件数据为空！");
                return false;
            }

            if (!letter.canCollectMultiple && HasLetter(letter))
            {
                Debug.Log($"【背包】已收集过此信件：{letter.letterName}");
                return false;
            }

            collectedLetters.Add(letter);
            totalLettersCollected++;
            totalPoints += letter.points;

            Debug.Log($"📨 收集信件：{letter.letterName} (+{letter.points}分) | 总计：{totalLettersCollected}封");

            if (enableAutoSave)
            {
                SaveInventory();
            }

            OnLetterCollected?.Invoke(letter, totalLettersCollected);
            PlayCollectEffect(letter);

            return true;
        }

        public bool HasLetter(LetterData letter)
        {
            if (letter == null) return false;
            return collectedLetters.Exists(l => l.letterId == letter.letterId);
        }

        public bool HasLetter(string letterId)
        {
            return collectedLetters.Exists(l => l.letterId == letterId);
        }

        public int GetLetterCount(LetterData letter)
        {
            if (letter == null) return 0;
            return collectedLetters.FindAll(l => l.letterId == letter.letterId).Count;
        }

        public int GetLetterCountByType(LetterType type)
        {
            return collectedLetters.FindAll(l => l.letterType == type).Count;
        }

        public void RemoveLetter(LetterData letter)
        {
            if (collectedLetters.Contains(letter))
            {
                collectedLetters.Remove(letter);
                totalLettersCollected--;
                totalPoints -= letter.points;

                Debug.Log($"【背包】移除信件：{letter.letterName}");

                if (enableAutoSave)
                {
                    SaveInventory();
                }
            }
        }

        public void ClearInventory()
        {
            collectedLetters.Clear();
            totalLettersCollected = 0;
            totalPoints = 0;

            Debug.Log("【背包】背包已清空");

            if (enableAutoSave)
            {
                PlayerPrefs.DeleteKey(saveKey);
                PlayerPrefs.Save();
            }
        }

        public void SaveInventory()
        {
            List<string> letterIds = new List<string>();
            foreach (LetterData letter in collectedLetters)
            {
                letterIds.Add(letter.letterId);
            }

            string json = JsonUtility.ToJson(new SaveData { letterIds = letterIds, totalPoints = totalPoints });
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.SetInt(saveKey + "_Count", totalLettersCollected);
            PlayerPrefs.Save();

            Debug.Log($"💾 背包已保存：{totalLettersCollected}封信件");
        }

        public void LoadInventory()
        {
            if (!PlayerPrefs.HasKey(saveKey))
            {
                Debug.Log("【背包】没有找到存档，新建背包");
                OnInventoryLoaded?.Invoke();
                return;
            }

            string json = PlayerPrefs.GetString(saveKey);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            totalLettersCollected = PlayerPrefs.GetInt(saveKey + "_Count", 0);
            totalPoints = saveData.totalPoints;

            Debug.Log($"📂 背包已加载：{totalLettersCollected}封信件，{totalPoints}分");
            OnInventoryLoaded?.Invoke();
        }

        private void PlayCollectEffect(LetterData letter)
        {
            if (letter.collectSound != null)
            {
                AudioSource.PlayClipAtPoint(letter.collectSound, Camera.main.transform.position);
            }
        }

        [System.Serializable]
        private class SaveData
        {
            public List<string> letterIds;
            public int totalPoints;
        }
    }
}
