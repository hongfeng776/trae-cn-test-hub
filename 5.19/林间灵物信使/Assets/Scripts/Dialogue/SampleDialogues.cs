using UnityEngine;

namespace ForestMessenger.Dialogue
{
    public class SampleDialogues : MonoBehaviour
    {
        [Header("狐狸对话")]
        public DialogueData foxGreeting;
        public DialogueData foxQuest;

        [Header("松鼠对话")]
        public DialogueData squirrelGreeting;
        public DialogueData squirrelHint;

        [Header("兔子对话")]
        public DialogueData rabbitGreeting;
        public DialogueData rabbitStory;

        [ContextMenu("创建示例对话数据")]
        public void CreateSampleDialogues()
        {
            Debug.Log("请在Unity编辑器中右键 -> Create -> Dialogue -> Dialogue Data 来创建对话数据");
        }
    }
}
