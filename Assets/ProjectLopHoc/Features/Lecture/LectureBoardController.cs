using Fusion;
using UnityEngine;
using UnityEngine.UI;
using ProjectLopHoc.Core.ScriptableObjects;

namespace ProjectLopHoc.Features.Lecture
{
    /// <summary>
    /// Quản lý nạp slide và đồng bộ trạng thái bài giảng thời gian thực qua mạng WebGL (Photon Fusion).
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class LectureBoardController : NetworkBehaviour
    {
        [Header("--- Dữ liệu Cấu hình ---")]
        [Tooltip("File ScriptableObject chứa danh mục bài giảng")]
        public ClassroomConfigSO classroomConfig;

        [Header("--- Thành phần Hiển thị ---")]
        [Tooltip("Gán Renderer mặt bảng 3D (nếu dùng Quad/Mesh)")]
        public Renderer boardRenderer;
        [Tooltip("Gán Image UI (nếu dùng WorldSpace Canvas)")]
        public Image boardImage;

        [Networked] public int CurrentSlideIndex { get; set; }
        [Networked] public int CurrentLectureIndex { get; set; }

        private Sprite[] _loadedSlides = new Sprite[0];
        private int _cachedLectureIndex = -1;
        private int _cachedSlideIndex = -1;

        private MaterialPropertyBlock _propBlock;
        private static readonly int BaseMapPropertyId = Shader.PropertyToID("_BaseMap");
        private static readonly int MainTexPropertyId = Shader.PropertyToID("_MainTex");

        public override void Spawned()
        {
            _propBlock = new MaterialPropertyBlock();
            LoadLectureSlides(CurrentLectureIndex);
            RefreshDisplayVisual();
        }

        public override void Render()
        {
            if (CurrentLectureIndex != _cachedLectureIndex)
            {
                LoadLectureSlides(CurrentLectureIndex);
                RefreshDisplayVisual();
                return;
            }

            if (CurrentSlideIndex != _cachedSlideIndex)
            {
                RefreshDisplayVisual();
            }
        }

        private void LoadLectureSlides(int lectureIndex)
        {
            _cachedLectureIndex = lectureIndex;

            if (classroomConfig == null || classroomConfig.availableLectures == null || classroomConfig.availableLectures.Count == 0)
            {
                Debug.LogWarning("[LectureBoard] Chưa cấu hình danh mục bài giảng trong ClassroomConfigSO.");
                _loadedSlides = new Sprite[0];
                return;
            }

            int safeIndex = Mathf.Clamp(lectureIndex, 0, classroomConfig.availableLectures.Count - 1);
            string folder = classroomConfig.availableLectures[safeIndex];

            _loadedSlides = Resources.LoadAll<Sprite>($"Lectures/{folder}");
            System.Array.Sort(_loadedSlides, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

            if (_loadedSlides.Length == 0)
            {
                Debug.LogWarning($"[LectureBoard] Không tìm thấy ảnh slide tại Resources/Lectures/{folder}");
            }
        }

        private void RefreshDisplayVisual()
        {
            _cachedSlideIndex = CurrentSlideIndex;

            if (_loadedSlides == null || _loadedSlides.Length == 0) return;

            int index = Mathf.Clamp(CurrentSlideIndex, 0, _loadedSlides.Length - 1);
            Sprite targetSprite = _loadedSlides[index];
            if (targetSprite == null) return;

            // 1. Áp dụng cho WorldSpace Canvas UI
            if (boardImage != null)
            {
                boardImage.sprite = targetSprite;
            }

            // 2. Áp dụng cho Mesh Renderer 3D
            if (boardRenderer != null)
            {
                boardRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetTexture(BaseMapPropertyId, targetSprite.texture);
                _propBlock.SetTexture(MainTexPropertyId, targetSprite.texture);
                boardRenderer.SetPropertyBlock(_propBlock);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RequestChangeSlide(int direction)
        {
            if (_loadedSlides == null || _loadedSlides.Length == 0) return;

            CurrentSlideIndex = Mathf.Clamp(CurrentSlideIndex + direction, 0, _loadedSlides.Length - 1);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RequestChangeLecture(int direction)
        {
            if (classroomConfig == null || classroomConfig.availableLectures == null || classroomConfig.availableLectures.Count == 0) return;

            CurrentLectureIndex = Mathf.Clamp(CurrentLectureIndex + direction, 0, classroomConfig.availableLectures.Count - 1);
            CurrentSlideIndex = 0;
        }

        public void NextSlide() => RPC_RequestChangeSlide(1);
        public void PreviousSlide() => RPC_RequestChangeSlide(-1);
        public void NextLecture() => RPC_RequestChangeLecture(1);
        public void PreviousLecture() => RPC_RequestChangeLecture(-1);
    }
}