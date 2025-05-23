using System;
using UnityEngine;

namespace Game
{
    public class SpinningCat : MonoBehaviour
    {
        public float rotationSpeed = 300f;
        public float upDownSpeed = 1f;
        public float upDownDistance = 1f;
        public Renderer catRenderer;
        public Color startColor = Color.white;
        public Color targetColor = Color.white;
        public float colorPickedTime = 1f;
        public float colorChangeDuration = 1f;
        public float startY = 0f;
        
        private float _randomValue;

        private void Start()
        {
            _randomValue = UnityEngine.Random.value * 100f;
            colorPickedTime = Time.time;
            targetColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            startY = transform.position.y;
            // set random y rotation
            transform.rotation = Quaternion.Euler(0, UnityEngine.Random.value * 360f, 0);
        }

        private void Update()
        {
            // Rotate the cat
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            // Move the cat up and down
            float newY = Mathf.Sin((Time.time + _randomValue) * upDownSpeed) * upDownDistance;
            transform.position = new Vector3(transform.position.x, startY + newY, transform.position.z);

            // lerp color to random color
            if (catRenderer != null)
            {
                if (Time.time - colorPickedTime > colorChangeDuration)
                {
                    startColor = catRenderer.material.color;
                    targetColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    colorPickedTime = Time.time;
                }
                
                float t = (Time.time - colorPickedTime) / colorChangeDuration;
                catRenderer.material.color = Color.Lerp(startColor, targetColor, t);
            }
        }
    }
}