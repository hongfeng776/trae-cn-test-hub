using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ForestMessenger.Core;
using ForestMessenger.Managers;

namespace ForestMessenger.Dialogue
{
    public class DialogueManager : Singleton<DialogueManager>
    {
        [Header("UI组件")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Image npcPortrait;
        [SerializeField] private Text npcNameText;
        [SerializeField] private Text dialogueText;
        [SerializeField] private GameObject continueIndicator;

        [Header("选项按钮")]
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("对话设置")]
        [SerializeField] private KeyCode nextDialogueKey = KeyCode.Space;
        [SerializeField] private Color defaultTextColor = Color.white;
        [SerializeField] private bool autoCreateUI = true;

        private DialogueData currentDialogue;
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private bool isDialogueActive = false;
        private Coroutine typingCoroutine;

        public delegate void DialogueStarted();
        public event DialogueStarted OnDialogueStarted;

        public delegate void DialogueEnded();
        public event DialogueEnded OnDialogueEnded;

        public bool IsDialogueActive => isDialogueActive;

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("【对话系统】DialogueManager 初始化中...");

            if (autoCreateUI && dialoguePanel == null)
            {
                CreateDefaultDialogueUI();
            }

            InitializeDialogueUI();
            Debug.Log("【对话系统】DialogueManager 初始化完成！");
        }

        private void CreateDefaultDialogueUI()
        {
            Debug.Log("【对话系统】自动创建默认对话UI...");

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("DialogueCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            GameObject panelObj = new GameObject("DialoguePanel");
            panelObj.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.05f);
            panelRect.anchorMax = new Vector2(0.9f, 0.35f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.15f, 0.1f, 0.9f);

            GameObject nameObj = new GameObject("NPCNameText");
            nameObj.transform.SetParent(panelObj.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.02f, 0.85f);
            nameRect.anchorMax = new Vector2(0.4f, 0.98f);
            npcNameText = nameObj.AddComponent<Text>();
            npcNameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            npcNameText.fontSize = 28;
            npcNameText.color = new Color(1f, 0.84f, 0f);
            npcNameText.alignment = TextAnchor.MiddleLeft;

            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(panelObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.02f, 0.1f);
            textRect.anchorMax = new Vector2(0.98f, 0.8f);
            dialogueText = textObj.AddComponent<Text>();
            dialogueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            dialogueText.fontSize = 24;
            dialogueText.color = new Color(1f, 0.97f, 0.88f);
            dialogueText.alignment = TextAnchor.UpperLeft;
            dialogueText.horizontalOverflow = HorizontalWrapMode.Wrap;
            dialogueText.verticalOverflow = VerticalWrapMode.Overflow;

            GameObject indicatorObj = new GameObject("ContinueIndicator");
            indicatorObj.transform.SetParent(panelObj.transform, false);
            RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(0.9f, 0.05f);
            indicatorRect.anchorMax = new Vector2(0.98f, 0.15f);
            continueIndicator = indicatorObj.AddComponent<Text>();
            continueIndicator.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            continueIndicator.fontSize = 20;
            continueIndicator.color = new Color(0.7f, 0.7f, 0.7f);
            continueIndicator.text = "▶";
            continueIndicator.alignment = TextAnchor.MiddleCenter;

            GameObject optionsObj = new GameObject("OptionsContainer");
            optionsObj.transform.SetParent(panelObj.transform, false);
            RectTransform optionsRect = optionsObj.AddComponent<RectTransform>();
            optionsRect.anchorMin = new Vector2(0.02f, 0.1f);
            optionsRect.anchorMax = new Vector2(0.98f, 0.8f);
            optionsContainer = optionsObj.transform;

            dialoguePanel = panelObj;

            Debug.Log("【对话系统】默认对话UI创建完成！");
        }

        private void InitializeDialogueUI()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (continueIndicator != null)
            {
                continueIndicator.SetActive(false);
            }

            ClearOptions();
        }

        private void Update()
        {
            if (!isDialogueActive) return;

            if (Input.GetKeyDown(nextDialogueKey) || Input.GetMouseButtonDown(0))
            {
                HandleDialogueInput();
            }
        }

        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null)
            {
                Debug.LogError("【对话系统】错误：对话数据为空！");
                return;
            }

            Debug.Log($"【对话系统】开始对话：{dialogue.npcName}，共 {dialogue.dialogueLines.Length} 行");

            currentDialogue = dialogue;
            currentLineIndex = 0;
            isDialogueActive = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(GameState.Paused);
            }

            OnDialogueStarted?.Invoke();

            ShowDialoguePanel();
            UpdateNPCInfo();
            DisplayCurrentLine();
        }

        private void ShowDialoguePanel()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
                dialoguePanel.transform.SetAsLastSibling();
                Debug.Log("【对话系统】对话面板已显示");
            }
            else
            {
                Debug.LogError("【对话系统】错误：对话面板为空！");
            }
        }

        private void HideDialoguePanel()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        private void UpdateNPCInfo()
        {
            if (currentDialogue == null) return;

            if (npcNameText != null)
            {
                npcNameText.text = currentDialogue.npcName;
                Debug.Log($"【对话系统】NPC名称：{currentDialogue.npcName}");
            }

            if (npcPortrait != null)
            {
                npcPortrait.sprite = currentDialogue.npcPortrait;
                npcPortrait.enabled = currentDialogue.npcPortrait != null;
            }
        }

        private void DisplayCurrentLine()
        {
            if (currentDialogue == null) return;

            if (currentLineIndex >= currentDialogue.dialogueLines.Length)
            {
                HandleDialogueEnd();
                return;
            }

            ClearOptions();

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeText(currentDialogue.dialogueLines[currentLineIndex]));
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;

            if (dialogueText != null)
            {
                dialogueText.text = "";
            }

            if (continueIndicator != null)
            {
                continueIndicator.SetActive(false);
            }

            foreach (char c in text.ToCharArray())
            {
                if (dialogueText != null)
                {
                    dialogueText.text += c;
                }

                if (currentDialogue != null && !currentDialogue.canSkip)
                {
                    yield return new WaitForSeconds(currentDialogue.typingSpeed);
                }
                else
                {
                    yield return null;
                }
            }

            isTyping = false;

            if (continueIndicator != null)
            {
                continueIndicator.SetActive(true);
            }

            Debug.Log($"【对话系统】文本显示完成：{text.Substring(0, Mathf.Min(20, text.Length))}...");
        }

        private void HandleDialogueInput()
        {
            if (isTyping)
            {
                if (currentDialogue != null && currentDialogue.canSkip)
                {
                    SkipTyping();
                }
                return;
            }

            NextLine();
        }

        private void SkipTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            if (currentDialogue != null && dialogueText != null)
            {
                dialogueText.text = currentDialogue.dialogueLines[currentLineIndex];
            }

            isTyping = false;

            if (continueIndicator != null)
            {
                continueIndicator.SetActive(true);
            }

            Debug.Log("【对话系统】跳过打字效果");
        }

        private void NextLine()
        {
            currentLineIndex++;

            if (currentDialogue == null || currentLineIndex >= currentDialogue.dialogueLines.Length)
            {
                HandleDialogueEnd();
            }
            else
            {
                DisplayCurrentLine();
            }
        }

        private void HandleDialogueEnd()
        {
            if (currentDialogue != null && currentDialogue.hasOptions &&
                currentDialogue.options != null && currentDialogue.options.Length > 0)
            {
                ShowOptions();
            }
            else
            {
                EndDialogue();
            }
        }

        private void ShowOptions()
        {
            if (continueIndicator != null)
            {
                continueIndicator.SetActive(false);
            }

            ClearOptions();

            Debug.Log($"【对话系统】显示 {currentDialogue.options.Length} 个选项");

            for (int i = 0; i < currentDialogue.options.Length; i++)
            {
                DialogueOption option = currentDialogue.options[i];
                CreateOptionButton(option, i);
            }
        }

        private void CreateOptionButton(DialogueOption option, int index)
        {
            if (optionButtonPrefab == null)
            {
                CreateDefaultOptionButton(option);
                return;
            }

            if (optionsContainer == null) return;

            GameObject buttonObj = Instantiate(optionButtonPrefab, optionsContainer);
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();

            if (buttonText != null)
            {
                buttonText.text = option.optionText;
            }

            if (button != null)
            {
                button.onClick.AddListener(() => OnOptionSelected(option));
            }
        }

        private void CreateDefaultOptionButton(DialogueOption option)
        {
            if (optionsContainer == null) return;

            GameObject buttonObj = new GameObject("OptionButton_" + option.optionText);
            buttonObj.transform.SetParent(optionsContainer, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 50);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.3f, 0.2f, 0.8f);

            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.4f, 0.3f);
            colors.pressedColor = new Color(0.15f, 0.25f, 0.15f);
            button.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 22;
            buttonText.color = Color.white;
            buttonText.text = option.optionText;
            buttonText.alignment = TextAnchor.MiddleCenter;

            button.onClick.AddListener(() => OnOptionSelected(option));
        }

        private void OnOptionSelected(DialogueOption option)
        {
            Debug.Log($"【对话系统】选择选项：{option.optionText}");

            if (option.isExitOption)
            {
                EndDialogue();
                return;
            }

            if (option.nextDialogue != null)
            {
                StartDialogue(option.nextDialogue);
            }
            else
            {
                EndDialogue();
            }
        }

        private void ClearOptions()
        {
            if (optionsContainer == null) return;

            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void EndDialogue()
        {
            Debug.Log("【对话系统】对话结束");

            isDialogueActive = false;
            currentDialogue = null;
            currentLineIndex = 0;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            ClearOptions();
            HideDialoguePanel();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(GameState.Playing);
            }

            OnDialogueEnded?.Invoke();
        }

        public void SetDialogueUI(GameObject panel, Image portrait, Text nameText, Text text, GameObject indicator)
        {
            dialoguePanel = panel;
            npcPortrait = portrait;
            npcNameText = nameText;
            dialogueText = text;
            continueIndicator = indicator;
        }
    }
}
