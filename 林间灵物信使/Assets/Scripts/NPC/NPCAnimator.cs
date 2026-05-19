using UnityEngine;
using ForestMessenger.Dialogue;

namespace ForestMessenger.NPC
{
    [RequireComponent(typeof(Animator))]
    public class NPCAnimator : MonoBehaviour
    {
        [Header("动画参数")]
        [SerializeField] private string isIdleParam = "IsIdle";
        [SerializeField] private string isTalkingParam = "IsTalking";
        [SerializeField] private string playerNearParam = "PlayerNear";

        private Animator animator;
        private NPCInteractable npcInteractable;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            npcInteractable = GetComponent<NPCInteractable>();
        }

        private void Start()
        {
            if (npcInteractable != null)
            {
                npcInteractable.OnPlayerEnteredRange += OnPlayerEnteredRange;
                npcInteractable.OnPlayerExitedRange += OnPlayerExitedRange;
                npcInteractable.OnDialogueStarted += OnDialogueStarted;
            }

            DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        }

        private void OnPlayerEnteredRange()
        {
            SetBool(playerNearParam, true);
        }

        private void OnPlayerExitedRange()
        {
            SetBool(playerNearParam, false);
        }

        private void OnDialogueStarted()
        {
            SetBool(isTalkingParam, true);
            SetBool(isIdleParam, false);
        }

        private void OnDialogueEnded()
        {
            SetBool(isTalkingParam, false);
            SetBool(isIdleParam, true);
        }

        private void SetBool(string paramName, bool value)
        {
            if (animator != null && !string.IsNullOrEmpty(paramName))
            {
                animator.SetBool(paramName, value);
            }
        }

        private void OnDestroy()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
            }

            if (npcInteractable != null)
            {
                npcInteractable.OnPlayerEnteredRange -= OnPlayerEnteredRange;
                npcInteractable.OnPlayerExitedRange -= OnPlayerExitedRange;
                npcInteractable.OnDialogueStarted -= OnDialogueStarted;
            }
        }
    }
}
