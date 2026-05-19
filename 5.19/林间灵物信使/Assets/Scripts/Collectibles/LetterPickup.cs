using UnityEngine;
using ForestMessenger.Managers;

namespace ForestMessenger.Collectibles
{
    public class LetterPickup : MonoBehaviour
    {
        [Header("信件配置")]
        [SerializeField] private LetterData letterData;
        [SerializeField] private bool useCustomSettings = false;

        [Header("自定义设置（可选）")]
        [SerializeField] private float customPickupRange = 1.5f;
        [SerializeField] private float customFloatSpeed = 1f;
        [SerializeField] private float customFloatHeight = 0.3f;
        [SerializeField] private Color customColor = Color.white;

        [Header("视觉效果")]
        [SerializeField] private bool useBobAnimation = true;
        [SerializeField] private bool useRotation = false;
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private bool useGlowEffect = true;

        [Header("拾取设置")]
        [SerializeField] private bool autoPickup = true;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float triggerCheckInterval = 0.1f;

        [Header("拾取特效")]
        [SerializeField] private GameObject pickupParticlePrefab;
        [SerializeField] private float collectAnimationDuration = 0.5f;

        [Header("调试模式")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private bool showGizmos = true;

        private SpriteRenderer spriteRenderer;
        private Collider2D pickupCollider;
        private Vector3 startPosition;
        private float randomOffset;
        private bool isCollected = false;
        private float spawnTime;
        private float lastCheckTime;

        public LetterData LetterData => letterData;
        public bool IsCollected => isCollected;

        private void Awake()
        {
            if (debugMode) Debug.Log($"【信件拾取】{gameObject.name} 初始化开始...");

            InitializeComponents();
            randomOffset = Random.Range(0f, 2f * Mathf.PI);

            if (debugMode) Debug.Log($"【信件拾取】{gameObject.name} 初始化完成！");
        }

        private void InitializeComponents()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                if (debugMode) Debug.LogWarning($"【信件拾取】{gameObject.name} 没有SpriteRenderer，自动添加！");
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                CreateDefaultSprite();
            }

            pickupCollider = GetComponent<Collider2D>();
            if (pickupCollider == null)
            {
                if (debugMode) Debug.LogWarning($"【信件拾取】{gameObject.name} 没有Collider2D，自动添加CircleCollider2D！");
                pickupCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            pickupCollider.isTrigger = true;

            CircleCollider2D circle = pickupCollider as CircleCollider2D;
            if (circle != null)
            {
                circle.radius = GetPickupRange();
            }

            ApplyVisualSettings();
        }

        private void Start()
        {
            startPosition = transform.position;
            spawnTime = Time.time;

            if (letterData == null)
            {
                Debug.LogError($"【信件拾取】严重错误！{gameObject.name} 没有设置LetterData！请在Inspector中配置！");
                CreateDefaultLetterData();
            }

            if (debugMode)
            {
                Debug.Log($"【信件拾取】{gameObject.name} 配置：信件={letterData?.letterName ?? "无"}, 拾取范围={GetPickupRange()}m, 自动拾取={autoPickup}");
            }
        }

        private void CreateDefaultLetterData()
        {
            LetterData defaultData = ScriptableObject.CreateInstance<LetterData>();
            defaultData.letterId = "auto_letter_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
            defaultData.letterName = "森林信件";
            defaultData.description = "一封神秘的森林信件";
            defaultData.scoreValue = 10;
            defaultData.letterColor = new Color(1f, 0.95f, 0.8f);
            letterData = defaultData;
        }

        private void Update()
        {
            if (isCollected) return;

            if (useBobAnimation)
            {
                UpdateBobAnimation();
            }

            if (useRotation)
            {
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }

            if (autoPickup && Time.time - lastCheckTime >= triggerCheckInterval)
            {
                lastCheckTime = Time.time;
                CheckAutoPickup();
            }
        }

        private void ApplyVisualSettings()
        {
            if (spriteRenderer != null)
            {
                Color color = useCustomSettings ? customColor : (letterData != null ? letterData.letterColor : Color.white);
                spriteRenderer.color = color;
                spriteRenderer.sortingOrder = 15;
                spriteRenderer.sortingLayerName = "Default";
            }
        }

        private void CreateDefaultSprite()
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    if (dist < 12f)
                    {
                        colors[y * 32 + x] = new Color(1f, 0.95f, 0.8f, 1f);
                    }
                    else
                    {
                        colors[y * 32 + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            tex.filterMode = FilterMode.Point;

            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 100f);
        }

        private void UpdateBobAnimation()
        {
            float speed = useCustomSettings ? customFloatSpeed : (letterData != null ? letterData.floatSpeed : 1f);
            float height = useCustomSettings ? customFloatHeight : (letterData != null ? letterData.floatHeight : 0.3f);
            float newY = startPosition.y + Mathf.Sin(Time.time * speed + randomOffset) * height;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        private void CheckAutoPickup()
        {
            float range = GetPickupRange();
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range);

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag(playerTag))
                {
                    if (debugMode) Debug.Log($"【信件拾取】检测到玩家进入范围：{collider.gameObject.name}");
                    TryPickup();
                    break;
                }
            }
        }

        private float GetPickupRange()
        {
            return useCustomSettings ? customPickupRange : (letterData != null ? letterData.autoPickupRange : 1.5f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (debugMode) Debug.Log($"【信件拾取】触发碰撞：{other.gameObject.name}, Tag={other.tag}, 目标Tag={playerTag}");

            if (other.CompareTag(playerTag))
            {
                if (debugMode) Debug.Log($"【信件拾取】玩家Tag匹配成功！准备拾取！");
                TryPickup();
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.CompareTag(playerTag))
            {
                TryPickup();
            }
        }

        public void TryPickup()
        {
            if (isCollected)
            {
                if (debugMode) Debug.LogWarning($"【信件拾取】{gameObject.name} 已经被收集过了！");
                return;
            }

            if (letterData != null && Time.time - spawnTime < letterData.pickupDelay)
            {
                if (debugMode) Debug.Log($"【信件拾取】拾取冷却中：{Time.time - spawnTime:F2}s / {letterData.pickupDelay}s");
                return;
            }

            CollectLetter();
        }

        private void CollectLetter()
        {
            isCollected = true;

            Debug.Log($"【信件拾取】✅ 成功收集信件：{letterData?.letterName ?? "未知信件"}！");

            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.AddLetter(letterData, this);
            }
            else
            {
                Debug.LogError($"【信件拾取】严重错误！CollectionManager不存在！请在场景中添加CollectionManager组件！");
            }

            PlayPickupEffects();

            if (pickupCollider != null)
            {
                pickupCollider.enabled = false;
            }

            if (letterData != null && letterData.destroyOnCollect)
            {
                StartCoroutine(CollectAndDestroy());
            }
        }

        private void PlayPickupEffects()
        {
            if (pickupParticlePrefab != null)
            {
                Instantiate(pickupParticlePrefab, transform.position, Quaternion.identity);
            }

            if (letterData != null && letterData.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(letterData.pickupSound, transform.position);
            }
        }

        private System.Collections.IEnumerator CollectAndDestroy()
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 startPos = transform.position;
            Vector3 targetPos = Camera.main != null ? Camera.main.transform.position : startPos;

            while (elapsed < collectAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / collectAnimationDuration;

                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                transform.position = Vector3.Lerp(startPos, targetPos, t * 0.5f);

                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f - t;
                    spriteRenderer.color = color;
                }

                yield return null;
            }

            if (debugMode) Debug.Log($"【信件拾取】{gameObject.name} 已销毁！");
            Destroy(gameObject);
        }

        public void ResetPickup()
        {
            isCollected = false;
            if (pickupCollider != null)
            {
                pickupCollider.enabled = true;
            }
            transform.localScale = Vector3.one;

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }

            if (debugMode) Debug.Log($"【信件拾取】{gameObject.name} 已重置！");
        }

        public void SetLetterData(LetterData data)
        {
            letterData = data;
            ApplyVisualSettings();

            if (debugMode) Debug.Log($"【信件拾取】{gameObject.name} 信件数据已更新为：{data?.letterName}");
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            float range = GetPickupRange();
            Gizmos.color = isCollected ? Color.gray : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);

            if (isCollected)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawIcon(transform.position, "Selected", true);
            }
        }
    }
}
