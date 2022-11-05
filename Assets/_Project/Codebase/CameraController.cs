using UnityEngine;

namespace _Project.Codebase
{
    public class CameraController : MonoBehaviour
    {
        public Transform targetTransform;
        private Vector2 _localCameraOffset;

        private const float _MOUSE_OFFSET_SCALE = 2.5f;

        private void LateUpdate()
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 mousePositionRelToCenter = new Vector2(Mathf.Clamp(mousePos.x, 0f, Screen.width), 
                Mathf.Clamp(mousePos.y, 0f, Screen.height))
                                               - new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            _localCameraOffset = new Vector2(mousePositionRelToCenter.x * _MOUSE_OFFSET_SCALE / Screen.width,
               mousePositionRelToCenter.y * _MOUSE_OFFSET_SCALE / Screen.height);
            
            Vector2 targetPos = Vector2.Lerp(transform.position, 
                
                (Vector2)targetTransform.position + _localCameraOffset, 5f * Time.deltaTime);
            transform.position = ((Vector3) targetPos).SetZ(-10f);
        }
    }
}