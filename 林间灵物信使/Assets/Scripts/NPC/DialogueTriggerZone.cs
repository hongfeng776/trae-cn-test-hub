using UnityEngine;
using ForestMessenger.Dialogue;

namespace ForestMessenger.NPC
{
    [RequireComponent(typeof(Collider2D))]
    public class DialogueTriggerZone : MonoBehaviour
    {
        [Header("触发设置")]
        [SerializeField] private DialogueData dialogueToTrigger;
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private bool disableAfterTrigger = true;
        [SerializeField] private float delayBeforeTrigger = 0f;

        [Header("碰撞检测")]
        [SerializeField] private string playerTag = "Player";

        private bool hasTriggered = false;
        private Collider2D triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();

            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered && triggerOnce) return;
            if (!other.CompareTag(playerTag)) return;
            if (DialogueManager.Instance.IsDialogueActive) return;

            TriggerDialogue();
        }

        private void TriggerDialogue()
        {
            if (dialogueToTrigger == null)
            {
                Debug.LogWarning("触发区域没有设置对话数据！");
                return;
            }

            hasTriggered = true;

            if (delayBeforeTrigger > 0f)
            {
                Invoke(nameof(StartDelayedDialogue), delayBeforeTrigger);
            }
            else
            {
                DialogueManager.Instance.StartDialogue(dialogueToTrigger);
            }

            if (disableAfterTrigger)
            {
                gameObject.SetActive(false);
            }
        }

        private void StartDelayedDialogue()
        {
            DialogueManager.Instance.StartDialogue(dialogueToTrigger);
        }

        public void ResetTrigger()
        {
            hasTriggered = false;
            gameObject.SetActive(true);
        }

        public void SetDialogue(DialogueData newDialogue)
        {
            dialogueToTrigger = newDialogue;
        }
    }
}
