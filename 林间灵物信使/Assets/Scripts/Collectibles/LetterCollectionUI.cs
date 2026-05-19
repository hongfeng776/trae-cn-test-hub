using UnityEngine;
using UnityEngine.UI;

namespace ForestMessenger.Collectibles
{
    public class LetterCollectionUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Text letterCountText;
        [SerializeField] private Text pointsText;
        [SerializeField] private Image letterIcon;
        [SerializeField] private GameObject panelRoot;

        [Header("动画设置")]
        [SerializeField] private bool enableAnimations = true;
        [SerializeField] private float collectAnimDuration = 0.5f;
        [SerializeField] private float scaleMultiplier = 1.3f;
        [SerializeField] private Color flashColor = Color.yellow;

        [Header("显示设置")]
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private string countFormat = "信件: {0}";
        [SerializeField] private string pointsFormat = "分数: {0}";

        private Vector3 originalScale;
        private Color originalColor;
        private bool isAnimating = false;
        private int lastDisplayedCount = -1;

        private void Awake()
        {
            if (letterCountText != null)
            {
                originalScale = letterCountText.transform.localScale;
                originalColor = letterCountText.color;
            }
        }

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();

            if (panelRoot != null)
            {
                panelRoot.SetActive(showOnStart);
            }
        }

        private void InitializeUI()
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("【收集UI】背包管理器不存在！");
                return;
            }

            UpdateDisplay(InventoryManager.Instance.TotalLettersCollected, InventoryManager.Instance.TotalPoints);
            Debug.Log($"📊 收集UI初始化：{InventoryManager.Instance.TotalLettersCollected}封信件");
        }

        private void SubscribeToEvents()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnLetterCollected += OnLetterCollected;
                InventoryManager.Instance.OnInventoryLoaded += OnInventoryLoaded;
            }
        }

        private void OnLetterCollected(LetterData letter, int newTotal)
        {
            UpdateDisplay(newTotal, InventoryManager.Instance.TotalPoints);

            if (enableAnimations && !isAnimating)
            {
                PlayCollectAnimation();
            }

            if (letter != null)
            {
                UpdateIconColor(letter.letterColor);
            }
        }

        private void OnInventoryLoaded()
        {
            if (InventoryManager.Instance != null)
            {
                UpdateDisplay(InventoryManager.Instance.TotalLettersCollected, InventoryManager.Instance.TotalPoints);
            }
        }

        private void UpdateDisplay(int letterCount, int points)
        {
            if (letterCountText != null)
            {
                letterCountText.text = string.Format(countFormat, letterCount);
            }

            if (pointsText != null)
            {
                pointsText.text = string.Format(pointsFormat, points);
            }

            lastDisplayedCount = letterCount;
        }

        private void UpdateIconColor(Color newColor)
        {
            if (letterIcon != null)
            {
                letterIcon.color = newColor;
            }
        }

        private void PlayCollectAnimation()
        {
            if (letterCountText == null) return;

            StopAllCoroutines();
            StartCoroutine(CollectAnimationCoroutine());
        }

        private System.Collections.IEnumerator CollectAnimationCoroutine()
        {
            isAnimating = true;
            float timer = 0f;

            while (timer < collectAnimDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / collectAnimDuration;

                float scale = 1f + Mathf.Sin(progress * Mathf.PI) * (scaleMultiplier - 1f);
                letterCountText.transform.localScale = originalScale * scale;

                if (letterIcon != null)
                {
                    letterIcon.transform.localScale = originalScale * scale;
                }

                Color currentColor = Color.Lerp(originalColor, flashColor, Mathf.Sin(progress * Mathf.PI));
                letterCountText.color = currentColor;

                yield return null;
            }

            letterCountText.transform.localScale = originalScale;
            letterCountText.color = originalColor;

            if (letterIcon != null)
            {
                letterIcon.transform.localScale = originalScale;
            }

            isAnimating = false;
        }

        public void ShowUI()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        public void HideUI()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        public void RefreshUI()
        {
            if (InventoryManager.Instance != null)
            {
                UpdateDisplay(InventoryManager.Instance.TotalLettersCollected, InventoryManager.Instance.TotalPoints);
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnLetterCollected -= OnLetterCollected;
                InventoryManager.Instance.OnInventoryLoaded -= OnInventoryLoaded;
            }
        }
    }
}
