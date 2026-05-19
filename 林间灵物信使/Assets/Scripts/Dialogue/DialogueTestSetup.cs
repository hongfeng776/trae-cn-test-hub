using UnityEngine;
using ForestMessenger.NPC;
using ForestMessenger.Dialogue;

namespace ForestMessenger.Tools
{
    [ExecuteInEditMode]
    public class DialogueTestSetup : MonoBehaviour
    {
        [Header("NPC设置")]
        public string npcName = "测试NPC";
        public bool createTestNPC = true;

        [Header("玩家设置")]
        public GameObject playerObject;
        public bool autoFindPlayer = true;

        [Header("对话设置")]
        public bool createTestDialogue = true;
        public float interactionRadius = 3f;

        private DialogueData testDialogue;

        [ContextMenu("🔧 一键设置对话测试环境")]
        public void SetupDialogueTest()
        {
            Debug.Log("【对话测试】开始设置对话测试环境...");

            SetupDialogueManager();
            SetupPlayer();
            CreateTestDialogue();

            if (createTestNPC)
            {
                CreateTestNPC();
            }

            Debug.Log("【对话测试】设置完成！运行游戏后移动玩家靠近NPC即可测试");
        }

        [ContextMenu("🧪 立即测试对话")]
        public void TestDialogue()
        {
            if (DialogueManager.Instance == null)
            {
                SetupDialogueManager();
            }

            if (testDialogue == null)
            {
                CreateTestDialogue();
            }

            if (DialogueManager.Instance != null && testDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(testDialogue);
                Debug.Log("【对话测试】已触发测试对话！");
            }
        }

        private void SetupDialogueManager()
        {
            DialogueManager existing = FindObjectOfType<DialogueManager>();
            if (existing == null)
            {
                GameObject managerObj = new GameObject("DialogueManager");
                managerObj.AddComponent<DialogueManager>();
                Debug.Log("【对话测试】已创建 DialogueManager");
            }
            else
            {
                Debug.Log("【对话测试】DialogueManager 已存在");
            }
        }

        private void SetupPlayer()
        {
            if (autoFindPlayer)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }

            if (playerObject == null)
            {
                Debug.LogWarning("【对话测试】未找到Player，请确保场景中有Tag为Player的对象");
                return;
            }

            if (playerObject.GetComponent<Collider2D>() == null)
            {
                playerObject.AddComponent<BoxCollider2D>();
                Debug.Log("【对话测试】已为玩家添加 BoxCollider2D");
            }

            Debug.Log($"【对话测试】玩家已设置：{playerObject.name}");
        }

        private void CreateTestDialogue()
        {
            testDialogue = ScriptableObject.CreateInstance<DialogueData>();
            testDialogue.name = "TestDialogue_" + npcName;
            testDialogue.npcName = npcName;
            testDialogue.dialogueLines = new string[]
            {
                "你好呀，旅行者！",
                "欢迎来到我们的神秘森林！",
                "这片森林里住着很多可爱的小动物哦~",
                "往前走你会发现更多有趣的东西！",
                "祝你旅途愉快！"
            };
            testDialogue.typingSpeed = 0.04f;
            testDialogue.canSkip = true;

            Debug.Log($"【对话测试】已创建测试对话数据：{testDialogue.name}");
        }

        private void CreateTestNPC()
        {
            GameObject npcObj = GameObject.Find(npcName);
            if (npcObj == null)
            {
                npcObj = new GameObject(npcName);
                npcObj.transform.position = new Vector3(0, 0, 0);
            }

            NPCInteractable interactable = npcObj.GetComponent<NPCInteractable>();
            if (interactable == null)
            {
                interactable = npcObj.AddComponent<NPCInteractable>();
            }

            CircleCollider2D trigger = npcObj.GetComponent<CircleCollider2D>();
            if (trigger == null)
            {
                trigger = npcObj.AddComponent<CircleCollider2D>();
            }
            trigger.radius = interactionRadius;
            trigger.isTrigger = true;

            SpriteRenderer renderer = npcObj.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = npcObj.AddComponent<SpriteRenderer>();
                Texture2D tex = new Texture2D(64, 64);
                Color[] colors = tex.GetPixels();
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color(0.8f, 0.6f, 0.3f);
                }
                tex.SetPixels(colors);
                tex.Apply();
                renderer.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
                renderer.sortingOrder = 10;
            }

            interactable.SetDialogue(testDialogue);

            Debug.Log($"【对话测试】NPC已创建：{npcName}，位置：{npcObj.transform.position}");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.Label("📝 对话测试工具", GUILayout.Height(25));

            if (GUILayout.Button("🧪 测试弹出对话", GUILayout.Height(30)))
            {
                TestDialogue();
            }

            if (GUILayout.Button("🔄 重置对话", GUILayout.Height(30)))
            {
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.EndDialogue();
                }
            }

            GUILayout.EndArea();
        }
    }
}
