using UnityEngine;

namespace UnityLearningLab
{
    /// <summary>
    /// A simple demonstration script to test AI agent interaction,
    /// dynamic property tuning, and live PlayMode manipulation.
    /// </summary>
    public class LearningDemo : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private Vector3 rotationSpeed = new Vector3(15f, 30f, 45f);

        [Header("Oscillation Settings")]
        [SerializeField] private float bounceHeight = 0.5f;
        [SerializeField] private float bounceFrequency = 2f;

        private Vector3 initialPosition;

        private void Start()
        {
            initialPosition = transform.position;
            Debug.Log($"[UnityLearningLab] LearningDemo initialized on {gameObject.name} at {initialPosition}");
        }

        private void Update()
        {
            // Continuous rotation
            transform.Rotate(rotationSpeed * Time.deltaTime);

            // Smooth vertical bouncing
            float newY = initialPosition.y + Mathf.Sin(Time.time * bounceFrequency) * bounceHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        /// <summary>
        /// Can be called via agent C# eval or custom command:
        /// unity command eval 'GameObject.Find("DemoCube").GetComponent<UnityLearningLab.LearningDemo>().SetSpeed(100f);'
        /// </summary>
        public void SetSpeed(float multiplier)
        {
            rotationSpeed = new Vector3(15f * multiplier, 30f * multiplier, 45f * multiplier);
            Debug.Log($"[UnityLearningLab] Rotation speed updated with multiplier: {multiplier}");
        }
    }
}
