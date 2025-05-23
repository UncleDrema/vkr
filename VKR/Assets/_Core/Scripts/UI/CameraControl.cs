using UnityEngine;

namespace Game.UI
{
    [ExecuteAlways]
    public class CameraControl : MonoBehaviour
    {
        public BoxCollider bounds;
        public float speed = 10f;
        public float angleSpeed = 60f;

        private void Update()
        {
            Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (Input.GetKey(KeyCode.LeftControl))
            {
                direction.y = -1;
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                direction.y = 1;
            }
                
            // Умножаем на поворот камеры, чтобы движение соответствовало направлению камеры
            direction = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * direction;
            Vector3 move = direction * Time.deltaTime * speed;
            Vector3 pos = transform.position + move;
            pos.x = Mathf.Clamp(pos.x, bounds.bounds.min.x, bounds.bounds.max.x);
            pos.y = Mathf.Clamp(pos.y, bounds.bounds.min.y, bounds.bounds.max.y);
            pos.z = Mathf.Clamp(pos.z, bounds.bounds.min.z, bounds.bounds.max.z);
            transform.position = pos;
            
            var rotation = transform.rotation.eulerAngles;
            if (Input.GetKey(KeyCode.Q))
            {
                rotation.y -= angleSpeed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                rotation.y += angleSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.R))
            {
                rotation.x -= angleSpeed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.F))
            {
                rotation.x += angleSpeed * Time.deltaTime;
            }
            transform.rotation = Quaternion.Euler(rotation);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
                Debug.Log("Game closed");
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Рейкаст для определения объекта под курсором
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    // Проверяем, что объект имеет компонент Collider
                    if (hit.collider != null)
                    {
                        // Получаем ссылку на объект, на который кликнули
                        GameObject clickedObject = hit.collider.gameObject;
                        var clickPosition = hit.point;
                        Debug.Log($"Clicked on {clickedObject.name} at {clickPosition}");
                    }
                }
            }
        }
    }
}