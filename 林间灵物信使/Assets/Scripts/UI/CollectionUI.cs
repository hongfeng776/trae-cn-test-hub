using UnityEngine;
using UnityEngine.UI;
using ForestMessenger.Managers;
using ForestMessenger.Collectibles;

namespace ForestMessenger.UI
{
    public class CollectionUI : MonoBehaviour
    {
        [Header("主UI设置")]
        [SerializeField] private bool autoCreateUI = true;
        [SerializeField] private Vector2 uiPosition = new Vector2(-20, -20);
        [SerializeField] private float uiScale = 1f;
        [SerializeField] private TextAnchor uiAnchor = TextAnchor.UpperRight;

        [Header("UI组件")]
        [SerializeField] private Text letterCountText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Image letterIcon;
        [SerializeField] private GameObject panelBackground;

        [Header("动画设置")]
        [SerializeField] private bool useCollectAnimation = true;
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private float pulseScale = 1.3f;

        [Header("显示设置")]
        [SerializeField] private bool showScore = true;
        [SerializeField] private bool showIcon = true;

        [Header("调试模式")]
        [SerializeField] private bool debugMode = true;

        private RectTransform panelRect;
        private Vector3 originalScale;
        private bool isAnimating = false;
        private bool isInitialized = false;

        private void Awake()
        {
            if (debugMode) Debug.Log("【收集UI】Awake 开始...");

            if (autoCreateUI)
            {
                CreateDefaultUI();
            }

            panelRect = GetComponent<RectTransform>();
            if (panelRect == null)
            {
                panelRect = gameObject.AddComponent<RectTransform>();
            }

            originalScale = transform.localScale;
            isInitialized = true;

            if (debugMode) Debug.Log("【收集UI】Awake 完成！");
        }

        private void Start()
        {
            if (debugMode) Debug.Log("【收集UI】Start 开始...");

            SubscribeToEvents();
            UpdateCollectionUI();

            if (debugMode)
            {
                Debug.Log($"【收集UI】UI已激活！当前数值：信件数={CollectionManager.Instance?.TotalLettersCollected ?? 0}, 分数={CollectionManager.Instance?.TotalScore ?? 0}");
            }
        }

        private void SubscribeToEvents()
        {
            if (CollectionManager.Instance == null)
            {
                Debug.LogError("【收集UI】严重错误！CollectionManager不存在！请在场景中添加CollectionManager组件！");
                return;
            }

            CollectionManager.Instance.OnCollectionUpdated -= UpdateCollectionUI;
            CollectionManager.Instance.OnLetterCollected -= OnLetterCollected;

            CollectionManager.Instance.OnCollectionUpdated += UpdateCollectionUI;
            CollectionManager.Instance.OnLetterCollected += OnLetterCollected;

            if (debugMode)
            {
                Debug.Log($"【收集UI】已订阅CollectionManager事件！");
            }
        }

        private void CreateDefaultUI()
        {
            if (debugMode) Debug.Log("【收集UI】开始自动创建UI...");

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                if (debugMode) Debug.Log("【收集UI】Canvas不存在，创建新Canvas...");

                GameObject canvasObj = new GameObject("CollectionCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            transform.SetParent(canvas.transform, false);

            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = gameObject.AddComponent<RectTransform>();
            }

            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = uiPosition;
            rect.sizeDelta = new Vector2(200, 70);

            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.1f, 0.05f, 0.85f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(-10, -10);
            bgRect.offsetMax = new Vector2(10, 10);
            panelBackground = bgObj;

            if (showIcon)
            {
                GameObject iconObj = new GameObject("LetterIcon");
                iconObj.transform.SetParent(transform, false);
                Image iconImage = iconObj.AddComponent<Image>();
                iconImage.color = new Color(1f, 0.9f, 0.7f);

                Texture2D tex = new Texture2D(32, 32);
                Color[] colors = new Color[32 * 32];
                for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                iconImage.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);

                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.05f, 0.5f);
                iconRect.anchorMax = new Vector2(0.05f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = new Vector2(35, 35);
                letterIcon = iconImage;
            }

            GameObject countObj = new GameObject("LetterCount");
            countObj.transform.SetParent(transform, false);
            Text countText = countObj.AddComponent<Text>();
            countText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            countText.fontSize = 32;
            countText.color = Color.white;
            countText.alignment = TextAnchor.MiddleLeft;
            countText.fontStyle = FontStyle.Bold;
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.28f, 0.55f);
            countRect.anchorMax = new Vector2(0.28f, 0.55f);
            countRect.pivot = new Vector2(0.5f, 0.5f);
            countRect.anchoredPosition = Vector2.zero;
            countRect.sizeDelta = new Vector2(100, 40);
            letterCountText = countText;

            if (showScore)
            {
                GameObject scoreObj = new GameObject("Score");
                scoreObj.transform.SetParent(transform, false);
                Text scoreTextComp = scoreObj.AddComponent<Text>();
                scoreTextComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                scoreTextComp.fontSize = 18;
                scoreTextComp.color = new Color(1f, 0.85f, 0.5f);
                scoreTextComp.alignment = TextAnchor.MiddleLeft;
                RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
                scoreRect.anchorMin = new Vector2(0.28f, 0.25f);
                scoreRect.anchorMax = new Vector2(0.28f, 0.25f);
                scoreRect.pivot = new Vector2(0.5f, 0.5f);
                scoreRect.anchoredPosition = Vector2.zero;
                scoreRect.sizeDelta = new Vector2(100, 25);
                scoreText = scoreTextComp;
            }

            transform.localScale = Vector3.one * uiScale;

            if (debugMode)
            {
                Debug.Log("【收集UI】✅ UI创建完成！");
                Debug.Log($"【收集UI】LetterCountText: {(letterCountText != null ? "已设置" : "缺失！")}");
                Debug.Log($"【收集UI】ScoreText: {(scoreText != null ? "已设置" : "缺失！")}");
            }
        }

        public void UpdateCollectionUI()
        {
            if (!isInitialized)
            {
                if (debugMode) Debug.LogWarning("【收集UI】UI尚未初始化，跳过更新！");
                return;
            }

            if (CollectionManager.Instance == null)
            {
                Debug.LogError("【收集UI】CollectionManager不存在！无法更新UI！");
                return;
            }

            int count = CollectionManager.Instance.TotalLettersCollected;
            int score = CollectionManager.Instance.TotalScore;

            if (letterCountText != null)
            {
                letterCountText.text = $"× {count}";

                if (debugMode)
                {
                    Debug.Log($"【收集UI】信件数更新为：{count}");
                }
            }
            else
            {
                Debug.LogError("【收集UI】letterCountText为空！无法显示信件数！");
            }

            if (showScore && scoreText != null)
            {
                scoreText.text = $"{score} 分";

                if (debugMode)
                {
                    Debug.Log($"【收集UI】分数更新为：{score}");
                }
            }
        }

        private void OnLetterCollected(LetterData letter, LetterPickup pickup)
        {
            if (debugMode)
            {
                Debug.Log($"【收集UI】收到收集事件！信件：{letter?.letterName}");
            }

            if (useCollectAnimation && !isAnimating)
            {
                StartCoroutine(CollectAnimation());
            }
        }

        private System.Collections.IEnumerator CollectAnimation()
        {
            isAnimating = true;

            if (debugMode) Debug.Log("【收集UI】开始播放收集动画！");

            float elapsed = 0f;
            Vector3 targetScale = originalScale * pulseScale;

            while (elapsed < animationDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration * 0.5f);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < animationDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration * 0.5f);
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
            isAnimating = false;

            if (debugMode) Debug.Log("【收集UI】收集动画播放完成！");
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
            if (debugMode) Debug.Log("【收集UI】显示UI！");
        }

        public void HideUI()
        {
            gameObject.SetActive(false);
            if (debugMode) Debug.Log("【收集UI】隐藏UI！");
        }

        private void OnDestroy()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnCollectionUpdated -= UpdateCollectionUI;
                CollectionManager.Instance.OnLetterCollected -= OnLetterCollected;

                if (debugMode) Debug.Log("【收集UI】已取消事件订阅！");
            }
        }
    }
}
