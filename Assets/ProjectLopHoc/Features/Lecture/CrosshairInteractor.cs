using UnityEngine;
using ProjectLopHoc.Core.Interfaces;

namespace ProjectLopHoc.Features.Lecture
{
    /// <summary>
    /// Gắn vào Main Camera của nhân vật. Xử lý quét tia tương tác và click chuột trên WebGL.
    /// </summary>
    public class CrosshairInteractor : MonoBehaviour
    {
        [Header("--- Cấu hình Tương tác ---")]
        [Tooltip("Khoảng cách tối đa có thể tương tác với vật thể")]
        public float interactRange = 5f;
        public LayerMask interactableLayer = ~0;

        private Camera _cam;
        private IInteractable _currentHovered;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;
        }

        private void Update()
        {
            if (_cam == null) return;

            ProcessHover();

            // Nhận diện click chuột trái hoặc phím E
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            {
                ProcessInteraction();
            }
        }

        private void ProcessHover()
        {
            IInteractable detected = PerformRaycast();

            if (detected != _currentHovered)
            {
                _currentHovered?.OnHoverExit();
                _currentHovered = detected;
                _currentHovered?.OnHoverEnter();
            }
        }

        private void ProcessInteraction()
        {
            _currentHovered?.OnInteract(transform.root.gameObject);
        }

        private IInteractable PerformRaycast()
        {
            // Nếu chuột bị khóa tâm ngắm (FPS), bấm thẳng từ giữa màn hình; nếu mở chuột, bấm theo tọa độ trỏ
            Ray ray = (Cursor.lockState == CursorLockMode.Locked) 
                ? new Ray(_cam.transform.position, _cam.transform.forward) 
                : _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
            {
                return hit.collider.GetComponentInParent<IInteractable>();
            }

            return null;
        }

        private void OnDisable()
        {
            if (_currentHovered != null)
            {
                _currentHovered.OnHoverExit();
                _currentHovered = null;
            }
        }
    }
}