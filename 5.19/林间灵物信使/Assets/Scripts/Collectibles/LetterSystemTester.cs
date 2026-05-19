using UnityEngine;
using ForestMessenger.Core;

namespace ForestMessenger.Collectibles
{
    [ExecuteInEditMode]
    public class LetterSystemTester : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField] private bool autoSetupManagers = true;
        [SerializeField] private bool createTestLetters = true;
        [SerializeField] private int testLetterCount = 5;

        [Header("UI设置")]
        [SerializeField] private bool autoCreateUI = true;

        [Header("调试")]
        [SerializeField] private bool showDebugInfo = true;

        private void Start()
        {
            if (Application.isPlaying && autoSetupManagers)
            {
                SetupManagers();
            }
        }

        [ContextMenu("🔧 一键设置信件系统")]
        public void SetupLetterSystem()
        {
            Debug.Log("📨 开始设置信件收集系统...");

            SetupManagers();
            CreateTestLetters();

            if (autoCreateUI)
            {
                CreateCollectionUI();
            }

            Debug.Log("✅ 信件收集系统设置完成！");
        }

        private void SetupManagers()
        {
            if (InventoryManager.Instance == null)
            {
                GameObject inventoryObj = new GameObject("InventoryManager");
                inventoryObj.AddComponent<InventoryManager>();
                Debug.Log("【测试】已创建 InventoryManager");
            }

            if (DialogueManager.Instance == null)
            {
                GameObject dialogueObj = new GameObject("DialogueManager");
                dialogueObj.AddComponent<DialogueManager>();
                Debug.Log("【测试】已创建 DialogueManager");
            }
        }

        private void CreateTestLetters()
        {
            if (!createTestLetters) return;

            LetterData[] testLetters = CreateTestLetterDatas();

            for (int i = 0; i < testLetterCount; i++)
            {
                CreateLetterPickup(testLetters[i % testLetters.Length], new Vector3(Random.Range(-8f, 8f), Random.Range(-3f, 3f), 0f));
            }

            Debug.Log($"【测试】已创建 {testLetterCount} 封测试信件");
        }

        private LetterData[] CreateTestLetterDatas()
        {
            LetterData[] letters = new LetterData[4];

            letters[0] = ScriptableObject.CreateInstance<LetterData>();
            letters[0].letterId = "letter_normal_01";
            letters[0].letterName = "普通信件";
            letters[0].letterType = LetterType.Normal;
            letters[0].letterColor = new Color(1f, 0.95f, 0.8f);
            letters[0].points = 10;
            letters[0].description = "一封来自森林小精灵的普通信件。";
            letters[0].letterContent = "亲爱的旅行者：\n\n欢迎来到神秘森林！\n这里住着许多可爱的小动物，希望你能喜欢这里！\n\n森林小精灵 敬上";

            letters[1] = ScriptableObject.CreateInstance<LetterData>();
            letters[1].letterId = "letter_rare_01";
            letters[1].letterName = "稀有信件";
            letters[1].letterType = LetterType.Rare;
            letters[1].letterColor = new Color(0.4f, 0.7f, 1f);
            letters[1].points = 25;
            letters[1].description = "一封带有蓝色封蜡的稀有信件！";
            letters[1].letterContent = "致勇敢的探险家：\n\n恭喜你找到了这封稀有信件！\n只有真正的探索者才能发现它。\n继续努力，森林中还有更多秘密等你发现！\n\n森林守护者 敬上";

            letters[2] = ScriptableObject.CreateInstance<LetterData>();
            letters[2].letterId = "letter_legendary_01";
            letters[2].letterName = "传说信件";
            letters[2].letterType = LetterType.Legendary;
            letters[2].letterColor = new Color(1f, 0.84f, 0f);
            letters[2].points = 50;
            letters[2].description = "闪闪发光的传说信件！";
            letters[2].letterContent = "✨ 森林传说 ✨\n\n很久以前，这片森林由一位古老的树精守护...\n传说收集齐所有信件的人，将获得森林的祝福！\n\n你已经迈出了第一步，继续前进吧！\n\n古老树精 敬上";

            letters[3] = ScriptableObject.CreateInstance<LetterData>();
            letters[3].letterId = "letter_quest_01";
            letters[3].letterName = "任务信件";
            letters[3].letterType = LetterType.Quest;
            letters[3].letterColor = new Color(0.8f, 0.4f, 1f);
            letters[3].points = 15;
            letters[3].description = "来自森林居民的求助信。";
            letters[3].letterContent = "求助！！！\n\n我的橡果都不见了！\n如果你能帮我找到它们，我将赠送你珍藏的金色橡子作为感谢！\n\n小松鼠 敬上";

            return letters;
        }

        private void CreateLetterPickup(LetterData data, Vector3 position)
        {
            GameObject letterObj = new GameObject($"Letter_{data.letterName}");
            letterObj.transform.position = position;
            letterObj.transform.parent = transform;

            SpriteRenderer renderer = letterObj.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(32, 32);
            Color[] colors = tex.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = data.letterColor;
            }
            tex.SetPixels(colors);
            tex.Apply();
            renderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);

            BoxCollider2D collider = letterObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.8f, 0.8f);

            LetterPickup pickup = letterObj.AddComponent<LetterPickup>();
            pickup.SetLetterData(data);
        }

        private void CreateCollectionUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<GraphicRaycaster>();

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            GameObject uiObj = new GameObject("LetterCollectionUI");
            uiObj.transform.SetParent(canvas.transform, false);

            RectTransform rect = uiObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.95f);
            rect.anchorMax = new Vector2(0.2f, 0.99f);
            rect.sizeDelta = Vector2.zero;

            Image panelImage = uiObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(uiObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.05f, 0.1f);
            iconRect.anchorMax = new Vector2(0.15f, 0.9f);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.yellow;

            GameObject countObj = new GameObject("LetterCount");
            countObj.transform.SetParent(uiObj.transform, false);
            RectTransform countRect = countObj.AddComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.2f, 0.1f);
            countRect.anchorMax = new Vector2(0.6f, 0.9f);
            Text countText = countObj.AddComponent<Text>();
            countText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            countText.fontSize = 24;
            countText.color = Color.white;
            countText.alignment = TextAnchor.MiddleLeft;

            GameObject pointsObj = new GameObject("Points");
            pointsObj.transform.SetParent(uiObj.transform, false);
            RectTransform pointsRect = pointsObj.AddComponent<RectTransform>();
            pointsRect.anchorMin = new Vector2(0.65f, 0.1f);
            pointsRect.anchorMax = new Vector2(0.95f, 0.9f);
            Text pointsText = pointsObj.AddComponent<Text>();
            pointsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            pointsText.fontSize = 20;
            pointsText.color = Color.green;
            pointsText.alignment = TextAnchor.MiddleRight;

            LetterCollectionUI ui = uiObj.AddComponent<LetterCollectionUI>();

            Debug.Log("【测试】已创建收集统计UI");
        }

        [ContextMenu("🧪 测试收集信件")]
        public void TestCollectLetter()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("【测试】请先运行游戏！");
                return;
            }

            if (InventoryManager.Instance == null)
            {
                Debug.LogError("【测试】背包管理器不存在！");
                return;
            }

            LetterData testLetter = ScriptableObject.CreateInstance<LetterData>();
            testLetter.letterId = "test_letter_" + Random.Range(1000, 9999);
            testLetter.letterName = "测试信件";
            testLetter.points = Random.Range(10, 50);

            bool success = InventoryManager.Instance.CollectLetter(testLetter);
            Debug.Log(success ? $"【测试】成功收集信件！当前：{InventoryManager.Instance.TotalLettersCollected}封" : "【测试】收集失败！");
        }

        [ContextMenu("🗑️ 清空背包")]
        public void ClearInventory()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ClearInventory();
                Debug.Log("【测试】背包已清空！");
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 120, 200, 120));
            GUILayout.Label("📨 信件系统测试", GUILayout.Height(25));

            if (InventoryManager.Instance != null)
            {
                GUILayout.Label($"已收集: {InventoryManager.Instance.TotalLettersCollected} 封");
                GUILayout.Label($"总分: {InventoryManager.Instance.TotalPoints}");

                if (GUILayout.Button("🧪 测试收集", GUILayout.Height(25)))
                {
                    TestCollectLetter();
                }

                if (GUILayout.Button("🗑️ 清空背包", GUILayout.Height(25)))
                {
                    ClearInventory();
                }
            }
            else
            {
                GUILayout.Label("背包管理器未初始化");
            }

            GUILayout.EndArea();
        }
    }
}
