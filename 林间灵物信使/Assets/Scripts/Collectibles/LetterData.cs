using UnityEngine;

namespace ForestMessenger.Collectibles
{
    public enum LetterType
    {
        Common,
        Rare,
        Epic,
        Legendary,
        Quest
    }

    [CreateAssetMenu(fileName = "NewLetter", menuName = "Collectibles/Letter Data", order = 2)]
    public class LetterData : ScriptableObject
    {
        [Header("信件基本信息")]
        [Tooltip("信件唯一ID")]
        public string letterId = "letter_001";

        [Tooltip("信件显示名称")]
        public string letterName = "神秘信件";

        [Tooltip("信件类型")]
        public LetterType letterType = LetterType.Common;

        [Tooltip("信件描述")]
        [TextArea(2, 5)]
        public string description = "一封来自森林深处的神秘信件...";

        [Header("视觉设置")]
        [Tooltip("信件精灵图标")]
        public Sprite letterIcon;

        [Tooltip("信件颜色")]
        public Color letterColor = Color.white;

        [Tooltip("浮动动画速度")]
        public float floatSpeed = 1f;

        [Tooltip("浮动高度")]
        public float floatHeight = 0.3f;

        [Header("收集设置")]
        [Tooltip("收集后获得的分数")]
        public int scoreValue = 10;

        [Tooltip("是否可以重复收集")]
        public bool canCollectMultiple = false;

        [Tooltip("收集后自动销毁")]
        public bool destroyOnCollect = true;

        [Header("拾取设置")]
        [Tooltip("自动拾取范围")]
        public float autoPickupRange = 1.5f;

        [Tooltip("拾取延迟")]
        public float pickupDelay = 0f;

        [Header("音效")]
        [Tooltip("拾取音效")]
        public AudioClip pickupSound;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(letterId))
            {
                letterId = "letter_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            }
        }
    }
}
