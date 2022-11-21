using UnityEngine;

namespace Controls
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 0.06f;
        [SerializeField] private float zoomSpeed = 10.0f;
        [SerializeField] private float rotateSpeed = 0.1f;
    
        [SerializeField] private float maxHeight = 100f;
        [SerializeField] private float minHeight = 10f;

        private Vector3 _cameraRotationStart;
    
        void Update()
        {
            MoveCamera();
            RotateCamera();
        }

        private void MoveCamera()
        {
            var t = transform;
            var position = t.position;
            
            float horizontalSpeed = speed * Input.GetAxis("Horizontal") * position.y;
            float verticalSpeed = speed * Input.GetAxis("Vertical") * position.y;
            float scrollSpeed = -zoomSpeed * Input.GetAxis("Mouse ScrollWheel") * Mathf.Log(position.y);

            if (position.y >= maxHeight && scrollSpeed > 0)
            {
                scrollSpeed = 0;
            }
            else if (position.y <= minHeight && scrollSpeed < 0)
            {
                scrollSpeed = 0;
            }
        
            Vector3 verticalMove = new Vector3(0, scrollSpeed, 0);
            Vector3 lateralMove = horizontalSpeed * t.right;
            Vector3 forwardMove = t.forward;
            forwardMove.y = 0;
            forwardMove.Normalize();
            forwardMove *= verticalSpeed;
        
            Vector3 move = verticalMove + lateralMove + forwardMove;
        
            t.position += move;
        }

        private void RotateCamera()
        {
            if (Input.GetMouseButtonDown(2))
            {
                _cameraRotationStart = Input.mousePosition;
            }

            if (Input.GetMouseButton(2))
            {
                Vector3 rotationEnd = Input.mousePosition;
            
                var rotationChange = rotationEnd - _cameraRotationStart;
                float dx = (rotationChange).x * rotateSpeed;
                float dy = (rotationChange).y * rotateSpeed;
            
                transform.rotation *= Quaternion.Euler(new Vector3(0, dx, 0));
                transform.GetChild(0).transform.rotation *= Quaternion.Euler(new Vector3(-dy, 0, 0));

                _cameraRotationStart = rotationEnd;
            }
        }
    }
}
