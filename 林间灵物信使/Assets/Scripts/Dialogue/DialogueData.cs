using UnityEngine;

namespace ForestMessenger.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data", order = 1)]
    public class DialogueData : ScriptableObject
    {
        [Header("NPC信息")]
        [Tooltip("NPC的显示名称")]
        public string npcName = "森林精灵";

        [Tooltip("NPC头像（可选）")]
        public Sprite npcPortrait;

        [Header("对话内容")]
        [Tooltip("对话文本数组，按顺序显示")]
        [TextArea(3, 10)]
        public string[] dialogueLines = new string[]
        {
            "欢迎来到神秘森林！",
            "我是这里的信使，专门负责传递小动物们的信件。",
            "你好呀，旅行者！"
        };

        [Header("对话选项")]
        [Tooltip("是否显示对话选项")]
        public bool hasOptions = false;

        [Tooltip("对话选项列表")]
        public DialogueOption[] options;

        [Header("对话设置")]
        [Tooltip("打字机效果速度，值越小越快")]
        [Range(0.01f, 0.1f)]
        public float typingSpeed = 0.04f;

        [Tooltip("是否允许跳过打字效果")]
        public bool canSkip = true;

        private void OnValidate()
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
            {
                dialogueLines = new string[] { "..." };
            }
        }
    }

    [System.Serializable]
    public class DialogueOption
    {
        [Tooltip("选项显示文本")]
        public string optionText = "选项";

        [Tooltip("选择后跳转的对话（可选）")]
        public DialogueData nextDialogue;

        [Tooltip("选择后是否直接退出对话")]
        public bool isExitOption = false;
    }
}
