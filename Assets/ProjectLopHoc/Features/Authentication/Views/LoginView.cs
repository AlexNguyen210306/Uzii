using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectLopHoc.Features.Authentication.Views
{
    public class LoginView : MonoBehaviour
    {
        [Header("--- Input Fields ---")]
        public TMP_InputField nameInputField;
        public TMP_InputField passwordInputField;

        [Header("--- Role Selection ---")]
        public Toggle studentToggle;
        public Toggle lecturerToggle;
        public GameObject passwordFieldContainer;

        [Header("--- Major Selection (Chuyên ngành) ---")]
        public Toggle toggleDuongSatTDC; 
        public Toggle toggleQuanLyDieuHanh;
        public Toggle toggleCongNghePhuongTien;
        public Toggle toggleXayDungQuanLy;

        [Header("--- Buttons ---")]
        public Button loginButton;

        // Bỏ biến errorText vì UI không có thành phần này

        // =====================================================
        // EVENTS
        // =====================================================
        public event Action<bool> OnRoleChanged;
        public event Action OnLoginClicked;

        // =====================================================
        // AWAKE
        // =====================================================
        private void Awake()
        {
            if (studentToggle != null)
                studentToggle.onValueChanged.AddListener(OnStudentToggleChanged);

            if (lecturerToggle != null)
                lecturerToggle.onValueChanged.AddListener(OnLecturerToggleChanged);

            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginButtonClicked);
        }

        // =====================================================
        // TOGGLE LOGIC
        // =====================================================
        private void OnStudentToggleChanged(bool isOn)
        {
            if (!isOn) return;
            OnRoleChanged?.Invoke(false);
            SetPasswordFieldVisible(false);
        }

        private void OnLecturerToggleChanged(bool isOn)
        {
            if (!isOn) return;
            OnRoleChanged?.Invoke(true);
            SetPasswordFieldVisible(true);
        }

        private void OnLoginButtonClicked()
        {
            OnLoginClicked?.Invoke();
        }

        // =====================================================
        // UI UPDATES
        // =====================================================
        public void SetInitialName(string savedName)
        {
            if (nameInputField != null) nameInputField.text = savedName;
        }

        public void SetPasswordFieldVisible(bool visible)
        {
            if (passwordFieldContainer != null) passwordFieldContainer.SetActive(visible);
        }

        // Thay đổi hàm SetError: In ra Console thay vì hiển thị lên màn hình game
        public void SetError(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log("[Thông báo trạng thái]: " + message);
            }
        }

        // =====================================================
        // DATA GETTERS
        // =====================================================
        public string GetInputName()
        {
            return nameInputField != null ? nameInputField.text.Trim() : string.Empty;
        }

        public string GetInputPassword()
        {
            return passwordInputField != null ? passwordInputField.text.Trim() : string.Empty;
        }

        // Hàm mới để lấy dữ liệu chuyên ngành người chơi chọn
        public string GetSelectedMajor()
        {
            if (toggleDuongSatTDC != null && toggleDuongSatTDC.isOn) return "Đường sắt tốc độ cao";
            if (toggleQuanLyDieuHanh != null && toggleQuanLyDieuHanh.isOn) return "Quản lý và điều hành";
            if (toggleCongNghePhuongTien != null && toggleCongNghePhuongTien.isOn) return "Công nghệ phương tiện";
            if (toggleXayDungQuanLy != null && toggleXayDungQuanLy.isOn) return "Xây dựng và quản lý";
            
            return "Chưa chọn chuyên ngành";
        }

        // =====================================================
        // CLEANUP
        // =====================================================
        private void OnDestroy()
        {
            if (studentToggle != null) studentToggle.onValueChanged.RemoveListener(OnStudentToggleChanged);
            if (lecturerToggle != null) lecturerToggle.onValueChanged.RemoveListener(OnLecturerToggleChanged);
            if (loginButton != null) loginButton.onClick.RemoveListener(OnLoginButtonClicked);
        }
    }
}