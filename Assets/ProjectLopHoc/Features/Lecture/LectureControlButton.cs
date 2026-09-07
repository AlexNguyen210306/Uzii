using UnityEngine;
using ProjectLopHoc.Core.Interfaces;

namespace ProjectLopHoc.Features.Lecture
{
    public enum WebButtonAction
    {
        NextSlide,
        PreviousSlide,
        NextLecture,
        PreviousLecture
    }

    /// <summary>
    /// Gắn vào khối 3D trên bàn giảng viên, nhận click chuột để điều khiển slide bài giảng.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WebLectureControlButton : MonoBehaviour, IInteractable
    {
        [Header("--- Cấu hình Thao tác ---")]
        [Tooltip("Kéo đối tượng chứa LectureBoardController vào đây")]
        public LectureBoardController boardController;
        public WebButtonAction action = WebButtonAction.NextSlide;

        [Header("--- Hiệu ứng Highlight Chuột ---")]
        public Renderer buttonRenderer;
        public Color hoverColor = Color.cyan;

        private Color _defaultColor = Color.white;
        private MaterialPropertyBlock _propBlock;
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();

            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponent<Renderer>();
            }

            if (buttonRenderer != null && buttonRenderer.sharedMaterial != null)
            {
                _defaultColor = buttonRenderer.sharedMaterial.color;
            }
        }

        public void OnInteract(GameObject interactor)
        {
            if (boardController == null)
            {
                Debug.LogWarning($"[{name}] Chưa cấu hình LectureBoardController trên Inspector.");
                return;
            }

            switch (action)
            {
                case WebButtonAction.NextSlide:
                    boardController.NextSlide();
                    break;
                case WebButtonAction.PreviousSlide:
                    boardController.PreviousSlide();
                    break;
                case WebButtonAction.NextLecture:
                    boardController.NextLecture();
                    break;
                case WebButtonAction.PreviousLecture:
                    boardController.PreviousLecture();
                    break;
            }
        }

        public void OnHoverEnter()
        {
            ApplyColor(hoverColor);
        }

        public void OnHoverExit()
        {
            ApplyColor(_defaultColor);
        }

        private void ApplyColor(Color color)
        {
            if (buttonRenderer == null) return;

            buttonRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(BaseColorPropertyId, color);
            _propBlock.SetColor(ColorPropertyId, color);
            buttonRenderer.SetPropertyBlock(_propBlock);
        }
    }
}