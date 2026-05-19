using UnityEngine;
using ForestMessenger.Managers;
using ForestMessenger.Collectibles;
using ForestMessenger.UI;

namespace ForestMessenger.Tools
{
    public class LetterSystemTester : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private int testLetterCount = 5;
        [SerializeField] private float spawnRadius = 3f;

        [Header("状态显示")]
        [SerializeField] private bool showDebugGUI = true;

        private GameObject testLetterPrefab;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupTestEnvironment();
            }
        }

        [ContextMenu("🔧 一键设置测试环境")]
        public void SetupTestEnvironment()
        {
            Debug.Log("【信件测试器】开始设置测试环境...");

            SetupCollectionManager();
            SetupCollectionUI();
            CreateTestLetters();

            Debug.Log("【信件测试器】✅ 测试环境设置完成！");
            Debug.Log("【信件测试器】👉 运行游戏，移动玩家靠近信件即可测试！");
        }

        private void SetupCollectionManager()
        {
            CollectionManager existing = FindObjectOfType<CollectionManager>();
            if (existing == null)
            {
                GameObject managerObj = new GameObject("CollectionManager");
                managerObj.AddComponent<CollectionManager>();
                Debug.Log("【信件测试器】✅ 创建CollectionManager");
            }
            else
            {
                Debug.Log("【信件测试器】✅ CollectionManager已存在");
            }
        }

        private void SetupCollectionUI()
        {
            CollectionUI existing = FindObjectOfType<CollectionUI>();
            if (existing == null)
            {
                GameObject uiObj = new GameObject("CollectionUI");
                uiObj.AddComponent<CollectionUI>();
                Debug.Log("【信件测试器】✅ 创建CollectionUI");
            }
            else
            {
                Debug.Log("【信件测试器】✅ CollectionUI已存在");
            }
        }

        private void CreateTestLetters()
        {
            for (int i = 0; i < testLetterCount; i++)
            {
                CreateSingleTestLetter(i);
            }
        }

        private void CreateSingleTestLetter(int index)
        {
            GameObject letterObj = new GameObject($"Letter_{index:00}");

            float angle = Random.Range(0f, 2f * Mathf.PI);
            float distance = Random.Range(0.5f, spawnRadius);
            float x = Mathf.Cos(angle) * distance;
            float y = Random.Range(0f, 1.5f);

            letterObj.transform.position = new Vector3(x, y, 0f);

            LetterPickup pickup = letterObj.AddComponent<LetterPickup>();

            LetterData letterData = ScriptableObject.CreateInstance<LetterData>();
            letterData.letterId = $"test_letter_{System.Guid.NewGuid().ToString("N").Substring(0, 8)}";
            letterData.letterName = $"森林信件 {index + 1}";
            letterData.description = "一封神秘的森林信件";
            letterData.letterType = (LetterType)Random.Range(0, 3);
            letterData.scoreValue = Random.Range(10, 51);
            letterData.letterColor = GetRandomLetterColor();
            letterData.canCollectMultiple = false;
            letterData.destroyOnCollect = true;

            pickup.SetValue("_letterData", letterData);

            SpriteRenderer renderer = letterObj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = letterData.letterColor;
            }

            Debug.Log($"【信件测试器】✅ 创建测试信件：{letterData.letterName}");
        }

        private Color GetRandomLetterColor()
        {
            Color[] colors = new Color[]
            {
                new Color(1f, 0.95f, 0.8f),
                new Color(1f, 0.85f, 0.6f),
                new Color(0.9f, 1f, 0.85f),
                new Color(0.85f, 0.95f, 1f),
                new Color(1f, 0.85f, 0.9f)
            };
            return colors[Random.Range(0, colors.Length)];
        }

        [ContextMenu("🧪 模拟收集信件")]
        public void SimulateLetterCollection()
        {
            if (CollectionManager.Instance == null)
            {
                Debug.LogError("【信件测试器】CollectionManager不存在！请先设置测试环境！");
                return;
            }

            LetterData testLetter = ScriptableObject.CreateInstance<LetterData>();
            testLetter.letterId = "simulated_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
            testLetter.letterName = "模拟信件";
            testLetter.scoreValue = 25;

            CollectionManager.Instance.AddLetter(testLetter, null);

            Debug.Log("【信件测试器】✅ 模拟信件收集成功！");
        }

        [ContextMenu("🔄 重置收集进度")]
        public void ResetCollectionProgress()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.ResetCollection();
                Debug.Log("【信件测试器】✅ 收集进度已重置！");
            }
        }

        private void OnGUI()
        {
            if (!showDebugGUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 250, 300), "信件系统测试面板", GUI.skin.window);

            GUILayout.Space(20);

            if (CollectionManager.Instance != null)
            {
                GUILayout.Label($"📬 已收集信件: {CollectionManager.Instance.TotalLettersCollected}");
                GUILayout.Label($"⭐ 总分数: {CollectionManager.Instance.TotalScore}");
            }
            else
            {
                GUILayout.Label("❌ CollectionManager 不存在");
            }

            GUILayout.Space(10);
            GUILayout.Label("--- 测试功能 ---");

            if (GUILayout.Button("🧪 模拟收集信件", GUILayout.Height(30)))
            {
                SimulateLetterCollection();
            }

            if (GUILayout.Button("🔄 重置收集进度", GUILayout.Height(30)))
            {
                ResetCollectionProgress();
            }

            if (!Application.isPlaying && GUILayout.Button("🔧 设置测试环境", GUILayout.Height(30)))
            {
                SetupTestEnvironment();
            }

            GUILayout.Space(10);
            GUILayout.Label("--- 操作说明 ---");
            GUILayout.Label("1. 确保玩家Tag为 'Player'");
            GUILayout.Label("2. 移动玩家靠近信件");
            GUILayout.Label("3. 查看Console调试日志");

            GUILayout.EndArea();
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
