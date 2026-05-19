using UnityEngine;

namespace ForestMessenger.Environment
{
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public Transform transform;
            public float parallaxFactor;
            [HideInInspector] public Vector3 startPosition;
        }

        [SerializeField] private ParallaxLayer[] layers;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private bool autoFindCamera = true;

        private Vector3 previousCameraPosition;

        private void Start()
        {
            if (autoFindCamera)
            {
                cameraTransform = Camera.main.transform;
            }

            previousCameraPosition = cameraTransform.position;

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].transform != null)
                {
                    layers[i].startPosition = layers[i].transform.position;
                }
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].transform == null) continue;

                Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;
                layers[i].transform.position += deltaMovement * layers[i].parallaxFactor;
            }

            previousCameraPosition = cameraTransform.position;
        }
    }
}
