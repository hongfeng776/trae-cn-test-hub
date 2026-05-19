using UnityEngine;
using ForestMessenger.Dialogue;

namespace ForestMessenger.NPC
{
    public class NPCInteractable : MonoBehaviour
    {
        [Header("NPC设置")]
        [SerializeField] private string npcId = "default_npc";
        [SerializeField] private DialogueData defaultDialogue;
        [SerializeField] private bool canInteract = true;
        [SerializeField] private bool oneTimeOnly = false;

        [Header("交互触发")]
        [SerializeField] private float interactionRadius = 3f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private bool useTriggerCollision = true;

        [Header("视觉提示")]
        [SerializeField] private GameObject interactionIndicator;
        [SerializeField] private float indicatorBobHeight = 0.3f;
        [SerializeField] private float indicatorBobSpeed = 2f;

        [Header("对话设置")]
        [SerializeField] private bool autoStartDialogue = true;
        [SerializeField] private float autoStartDelay = 0.5f;

        private bool isPlayerInRange = false;
        private bool hasInteracted = false;
        private float autoStartTimer = 0f;
        private Transform playerTransform;
        private Vector3 indicatorStartPosition;
        private Collider2D npcCollider;

        public delegate void PlayerEnteredRange();
        public event PlayerEnteredRange OnPlayerEnteredRange;

        public delegate void PlayerExitedRange();
        public event PlayerExitedRange OnPlayerExitedRange;

        public delegate void DialogueStarted();
        public event DialogueStarted OnDialogueStarted;

        public bool IsPlayerInRange => isPlayerInRange;
        public bool CanInteract => canInteract && !hasInteracted;
        public string NPCId => npcId;

        private void Awake()
        {
            npcCollider = GetComponent<Collider2D>();

            if (npcCollider == null)
            {
                npcCollider = gameObject.AddComponent<CircleCollider2D>();
                CircleCollider2D circle = npcCollider as CircleCollider2D;
                circle.radius = interactionRadius;
                circle.isTrigger = true;
                Debug.Log($"【NPC交互】{npcId} 自动创建碰撞体");
            }

            if (playerLayer == 0)
            {
                playerLayer = LayerMask.GetMask("Default");
                Debug.LogWarning($"【NPC交互】{npcId} PlayerLayer未设置，使用Default层");
            }
        }

        private void Start()
        {
            if (interactionIndicator != null)
            {
                indicatorStartPosition = interactionIndicator.transform.localPosition;
                interactionIndicator.SetActive(false);
            }

            Debug.Log($"【NPC交互】{npcId} 初始化完成，交互半径：{interactionRadius}");
        }

        private void Update()
        {
            if (!canInteract) return;

            if (!useTriggerCollision)
            {
                CheckPlayerInRange();
            }

            UpdateInteractionIndicator();
            HandleAutoStartDialogue();
            HandleManualInteraction();
        }

        private void CheckPlayerInRange()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius, playerLayer);
            bool playerFound = false;

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    playerFound = true;
                    if (playerTransform == null)
                    {
                        playerTransform = collider.transform;
                        Debug.Log($"【NPC交互】{npcId} 检测到玩家进入范围");
                    }
                    break;
                }
            }

            if (playerFound && !isPlayerInRange)
            {
                PlayerEnteredRangeHandler();
            }
            else if (!playerFound && isPlayerInRange)
            {
                PlayerExitedRangeHandler();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!useTriggerCollision) return;
            if (!canInteract) return;

            if (other.CompareTag("Player"))
            {
                playerTransform = other.transform;
                Debug.Log($"【NPC交互】{npcId} 触发检测：玩家进入范围");
                PlayerEnteredRangeHandler();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!useTriggerCollision) return;

            if (other.CompareTag("Player"))
            {
                Debug.Log($"【NPC交互】{npcId} 触发检测：玩家离开范围");
                PlayerExitedRangeHandler();
            }
        }

        private void PlayerEnteredRangeHandler()
        {
            isPlayerInRange = true;
            autoStartTimer = 0f;

            ShowInteractionIndicator(true);
            OnPlayerEnteredRange?.Invoke();
        }

        private void PlayerExitedRangeHandler()
        {
            isPlayerInRange = false;
            playerTransform = null;
            autoStartTimer = 0f;

            ShowInteractionIndicator(false);
            OnPlayerExitedRange?.Invoke();
        }

        private void HandleAutoStartDialogue()
        {
            if (!autoStartDialogue || !isPlayerInRange) return;
            if (DialogueManager.Instance == null)
            {
                Debug.LogError($"【NPC交互】{npcId} DialogueManager不存在！");
                return;
            }
            if (DialogueManager.Instance.IsDialogueActive) return;
            if (oneTimeOnly && hasInteracted) return;

            autoStartTimer += Time.deltaTime;

            if (autoStartTimer >= autoStartDelay)
            {
                StartInteraction();
            }
        }

        private void HandleManualInteraction()
        {
            if (autoStartDialogue) return;
            if (!isPlayerInRange) return;
            if (DialogueManager.Instance == null) return;
            if (DialogueManager.Instance.IsDialogueActive) return;
            if (oneTimeOnly && hasInteracted) return;

            if (Input.GetKeyDown(interactionKey))
            {
                StartInteraction();
            }
        }

        public void StartInteraction()
        {
            if (!CanInteract) return;

            if (defaultDialogue == null)
            {
                Debug.LogError($"【NPC交互】{npcId} 没有对话数据！请在Inspector中设置DefaultDialogue");
                return;
            }

            Debug.Log($"【NPC交互】{npcId} 开始对话：{defaultDialogue.npcName}");

            hasInteracted = oneTimeOnly;
            autoStartTimer = 0f;

            OnDialogueStarted?.Invoke();

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(defaultDialogue);
            }
        }

        public void SetDialogue(DialogueData newDialogue)
        {
            defaultDialogue = newDialogue;
            Debug.Log($"【NPC交互】{npcId} 对话数据已更新");
        }

        public void ResetInteraction()
        {
            hasInteracted = false;
            autoStartTimer = 0f;
        }

        public void EnableInteraction(bool enable)
        {
            canInteract = enable;

            if (!enable)
            {
                ShowInteractionIndicator(false);
            }
        }

        private void ShowInteractionIndicator(bool show)
        {
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(show && CanInteract);
            }
        }

        private void UpdateInteractionIndicator()
        {
            if (interactionIndicator == null || !interactionIndicator.activeSelf) return;

            float bobOffset = Mathf.Sin(Time.time * indicatorBobSpeed) * indicatorBobHeight;
            interactionIndicator.transform.localPosition = indicatorStartPosition + new Vector3(0f, bobOffset, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);

            if (isPlayerInRange)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, interactionRadius);
            }
        }
    }
}
