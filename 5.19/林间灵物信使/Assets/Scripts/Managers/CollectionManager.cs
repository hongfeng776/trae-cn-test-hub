using UnityEngine;
using System.Collections.Generic;
using ForestMessenger.Core;
using ForestMessenger.Collectibles;

namespace ForestMessenger.Managers
{
    public class CollectionManager : Singleton<CollectionManager>
    {
        [Header("收集设置")]
        [SerializeField] private bool saveProgress = true;
        [SerializeField] private string saveKeyPrefix = "Forest_Letter_";

        [Header("收集统计")]
        [SerializeField] private int totalLettersCollected = 0;
        [SerializeField] private int totalScore = 0;

        [Header("收集的信件")]
        [SerializeField] private List<CollectedLetter> collectedLetters = new List<CollectedLetter>();

        [Header("调试模式")]
        [SerializeField] private bool debugMode = true;

        public delegate void LetterCollected(LetterData letter, LetterPickup pickup);
        public event LetterCollected OnLetterCollected;

        public delegate void CollectionUpdated();
        public event CollectionUpdated OnCollectionUpdated;

        public int TotalLettersCollected => totalLettersCollected;
        public int TotalScore => totalScore;
        public List<CollectedLetter> CollectedLetters => collectedLetters;

        [System.Serializable]
        public class CollectedLetter
        {
            public string letterId;
            public string letterName;
            public LetterType letterType;
            public int collectCount;
            public int totalScore;
            public System.DateTime firstCollectedTime;
            public System.DateTime lastCollectedTime;

            public CollectedLetter(LetterData data)
            {
                letterId = data.letterId;
                letterName = data.letterName;
                letterType = data.letterType;
                collectCount = 1;
                totalScore = data.scoreValue;
                firstCollectedTime = System.DateTime.Now;
                lastCollectedTime = System.DateTime.Now;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (debugMode)
            {
                Debug.Log("【收集管理器】初始化开始...");
                Debug.Log($"【收集管理器】当前实例：{Instance != null}");
            }

            LoadProgress();

            if (debugMode)
            {
                Debug.Log($"【收集管理器】加载完成！已收集：{totalLettersCollected} 封，总分：{totalScore}");
            }
        }

        private void Start()
        {
            if (debugMode)
            {
                Debug.Log("【收集管理器】触发初始UI更新...");
            }

            OnCollectionUpdated?.Invoke();
        }

        public void AddLetter(LetterData letterData, LetterPickup pickup)
        {
            if (letterData == null)
            {
                Debug.LogError("【收集管理器】严重错误！尝试添加空信件数据！");
                return;
            }

            if (debugMode)
            {
                Debug.Log($"【收集管理器】收到信件：{letterData.letterName} (ID: {letterData.letterId})");
            }

            CollectedLetter existing = FindLetter(letterData.letterId);

            if (existing != null)
            {
                if (!letterData.canCollectMultiple)
                {
                    if (debugMode)
                    {
                        Debug.LogWarning($"【收集管理器】{letterData.letterName} 已收集过，且不允许重复收集！");
                    }
                    return;
                }

                existing.collectCount++;
                existing.totalScore += letterData.scoreValue;
                existing.lastCollectedTime = System.DateTime.Now;

                if (debugMode)
                {
                    Debug.Log($"【收集管理器】重复收集！现在共收集 {existing.collectCount} 次");
                }
            }
            else
            {
                collectedLetters.Add(new CollectedLetter(letterData));
                totalLettersCollected++;

                if (debugMode)
                {
                    Debug.Log($"【收集管理器】新信件！总收集数变为：{totalLettersCollected}");
                }
            }

            totalScore += letterData.scoreValue;

            if (debugMode)
            {
                Debug.Log($"【收集管理器】✅ 收集成功！总计：{totalLettersCollected} 封，总分：{totalScore}");
            }

            if (debugMode)
            {
                Debug.Log($"【收集管理器】触发收集事件！OnLetterCollected 订阅者：{(OnLetterCollected != null ? OnLetterCollected.GetInvocationList().Length : 0)}");
            }

            OnLetterCollected?.Invoke(letterData, pickup);

            if (debugMode)
            {
                Debug.Log($"【收集管理器】触发更新事件！OnCollectionUpdated 订阅者：{(OnCollectionUpdated != null ? OnCollectionUpdated.GetInvocationList().Length : 0)}");
            }

            OnCollectionUpdated?.Invoke();

            if (saveProgress)
            {
                SaveProgress();
            }
        }

        public bool HasCollectedLetter(string letterId)
        {
            return FindLetter(letterId) != null;
        }

        public bool HasCollectedLetter(LetterData letterData)
        {
            if (letterData == null) return false;
            return HasCollectedLetter(letterData.letterId);
        }

        public int GetLetterCollectCount(string letterId)
        {
            CollectedLetter letter = FindLetter(letterId);
            return letter != null ? letter.collectCount : 0;
        }

        public int GetLetterCollectCount(LetterData letterData)
        {
            if (letterData == null) return 0;
            return GetLetterCollectCount(letterData.letterId);
        }

        public int GetLetterTypeCount(LetterType type)
        {
            int count = 0;
            foreach (var letter in collectedLetters)
            {
                if (letter.letterType == type)
                {
                    count++;
                }
            }
            return count;
        }

        private CollectedLetter FindLetter(string letterId)
        {
            foreach (var letter in collectedLetters)
            {
                if (letter.letterId == letterId)
                {
                    return letter;
                }
            }
            return null;
        }

        public void ResetCollection()
        {
            collectedLetters.Clear();
            totalLettersCollected = 0;
            totalScore = 0;

            if (debugMode)
            {
                Debug.Log("【收集管理器】收集进度已重置！触发UI更新...");
            }

            OnCollectionUpdated?.Invoke();

            if (saveProgress)
            {
                ClearSavedProgress();
            }
        }

        private void SaveProgress()
        {
            if (!saveProgress) return;

            PlayerPrefs.SetInt(saveKeyPrefix + "TotalCount", totalLettersCollected);
            PlayerPrefs.SetInt(saveKeyPrefix + "TotalScore", totalScore);
            PlayerPrefs.SetInt(saveKeyPrefix + "LetterCount", collectedLetters.Count);

            for (int i = 0; i < collectedLetters.Count; i++)
            {
                var letter = collectedLetters[i];
                PlayerPrefs.SetString($"{saveKeyPrefix}Letter_{i}_Id", letter.letterId);
                PlayerPrefs.SetString($"{saveKeyPrefix}Letter_{i}_Name", letter.letterName);
                PlayerPrefs.SetInt($"{saveKeyPrefix}Letter_{i}_Type", (int)letter.letterType);
                PlayerPrefs.SetInt($"{saveKeyPrefix}Letter_{i}_Count", letter.collectCount);
                PlayerPrefs.SetInt($"{saveKeyPrefix}Letter_{i}_Score", letter.totalScore);
            }

            PlayerPrefs.Save();

            if (debugMode)
            {
                Debug.Log("【收集管理器】进度已保存！");
            }
        }

        private void LoadProgress()
        {
            if (!saveProgress) return;

            totalLettersCollected = PlayerPrefs.GetInt(saveKeyPrefix + "TotalCount", 0);
            totalScore = PlayerPrefs.GetInt(saveKeyPrefix + "TotalScore", 0);
            int letterCount = PlayerPrefs.GetInt(saveKeyPrefix + "LetterCount", 0);

            collectedLetters.Clear();

            for (int i = 0; i < letterCount; i++)
            {
                string letterId = PlayerPrefs.GetString($"{saveKeyPrefix}Letter_{i}_Id", "");
                if (string.IsNullOrEmpty(letterId)) continue;

                string letterName = PlayerPrefs.GetString($"{saveKeyPrefix}Letter_{i}_Name", "未知信件");
                LetterType letterType = (LetterType)PlayerPrefs.GetInt($"{saveKeyPrefix}Letter_{i}_Type", 0);
                int collectCount = PlayerPrefs.GetInt($"{saveKeyPrefix}Letter_{i}_Count", 1);
                int score = PlayerPrefs.GetInt($"{saveKeyPrefix}Letter_{i}_Score", 0);

                CollectedLetter letter = new CollectedLetter
                (
                    ScriptableObject.CreateInstance<LetterData>()
                );
                letter.letterId = letterId;
                letter.letterName = letterName;
                letter.letterType = letterType;
                letter.collectCount = collectCount;
                letter.totalScore = score;

                collectedLetters.Add(letter);
            }
        }

        private void ClearSavedProgress()
        {
            int letterCount = PlayerPrefs.GetInt(saveKeyPrefix + "LetterCount", 0);
            for (int i = 0; i < letterCount; i++)
            {
                PlayerPrefs.DeleteKey($"{saveKeyPrefix}Letter_{i}_Id");
                PlayerPrefs.DeleteKey($"{saveKeyPrefix}Letter_{i}_Name");
                PlayerPrefs.DeleteKey($"{saveKeyPrefix}Letter_{i}_Type");
                PlayerPrefs.DeleteKey($"{saveKeyPrefix}Letter_{i}_Count");
                PlayerPrefs.DeleteKey($"{saveKeyPrefix}Letter_{i}_Score");
            }

            PlayerPrefs.DeleteKey(saveKeyPrefix + "TotalCount");
            PlayerPrefs.DeleteKey(saveKeyPrefix + "TotalScore");
            PlayerPrefs.DeleteKey(saveKeyPrefix + "LetterCount");
            PlayerPrefs.Save();

            if (debugMode)
            {
                Debug.Log("【收集管理器】保存的进度已清除！");
            }
        }

        private void OnDestroy()
        {
            if (saveProgress)
            {
                SaveProgress();
            }
        }

        private void OnApplicationQuit()
        {
            if (saveProgress)
            {
                SaveProgress();
            }
        }
    }
}
