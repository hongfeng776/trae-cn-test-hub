using UnityEngine;

namespace ForestMessenger.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("目标设置")]
        [SerializeField] private Transform target;
        [SerializeField] private bool autoFindPlayer = true;
        [SerializeField] private string playerTag = "Player";

        [Header("跟随设置")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);
        [SerializeField] private float smoothTime = 0.25f;
        [SerializeField] private bool clampPosition;
        [SerializeField] private Vector2 minPosition;
        [SerializeField] private Vector2 maxPosition;

        private Vector3 velocity;

        private void Start()
        {
            if (autoFindPlayer && target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            if (clampPosition)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, minPosition.x, maxPosition.x),
                    Mathf.Clamp(transform.position.y, minPosition.y, maxPosition.y),
                    transform.position.z
                );
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
