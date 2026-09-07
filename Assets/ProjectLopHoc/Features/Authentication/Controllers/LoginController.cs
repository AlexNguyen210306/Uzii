using UnityEngine;
using System;
using System.Threading.Tasks; // Thêm thư viện Task cho async/await
using ProjectLopHoc.Core.ScriptableObjects;
using ProjectLopHoc.Features.Authentication.Models;
using ProjectLopHoc.Features.Authentication.Views;

namespace ProjectLopHoc.Features.Authentication.Controllers
{
    public class LoginController : MonoBehaviour
    {
        [Header("--- Tham chiếu cấu hình ---")]
        [Tooltip("Kéo file Classroom_01_Config đã tạo vào đây")]
        public ClassroomConfigSO classroomConfig;

        [Header("--- Tham chiếu View ---")]
        public LoginView loginView;

        private LoginModel _model;
        private bool _isSelectingLecturer = false;
        private bool _loginInProgress = false;

        // Event phát ra khi đăng nhập thành công
        public static event Action<string, bool> OnLoginSuccess;

        private void Start()
        {
            if (loginView == null)
            {
                Debug.LogError("[LoginController] LoginView chưa được gán!");
                return;
            }

            _model = new LoginModel();
            loginView.SetInitialName(_model.DisplayName);
            loginView.SetPasswordFieldVisible(false);

            loginView.OnRoleChanged += HandleRoleChanged;
            loginView.OnLoginClicked += HandleLogin;

            Debug.Log("[LoginController] Khởi tạo Login thành công.");
        }

        private void OnDestroy()
        {
            if (loginView != null)
            {
                loginView.OnRoleChanged -= HandleRoleChanged;
                loginView.OnLoginClicked -= HandleLogin;
            }
        }

        private void HandleRoleChanged(bool isLecturer)
        {
            // Nếu đã đăng nhập thì không cho đổi role
            if (_loginInProgress) return;

            _isSelectingLecturer = isLecturer;
            loginView.SetPasswordFieldVisible(isLecturer);
            loginView.SetError("");
        }

        // Đổi thành async để gọi hệ thống mạng
        private async void HandleLogin()
        {
            // ==========================================
            // CHỐNG DOUBLE CLICK
            // ==========================================
            if (_loginInProgress)
            {
                Debug.LogWarning("[LoginController] Login đang được xử lý. Bỏ qua lần bấm tiếp theo.");
                return;
            }

            if (loginView == null)
            {
                Debug.LogError("[LoginController] LoginView không tồn tại!");
                return;
            }

            string inputName = loginView.GetInputName();
            string inputPassword = loginView.GetInputPassword();

            // ==========================================
            // KIỂM TRA TÊN & MẬT KHẨU
            // ==========================================
            if (string.IsNullOrWhiteSpace(inputName))
            {
                loginView.SetError("Vui lòng nhập tên hiển thị!");
                return;
            }

            if (_isSelectingLecturer)
            {
                if (classroomConfig == null)
                {
                    loginView.SetError("Lỗi: Chưa cấu hình ClassroomConfigSO!");
                    Debug.LogError("[LoginController] classroomConfig chưa được gán!");
                    return;
                }

                if (inputPassword != classroomConfig.lecturerSecretPin)
                {
                    loginView.SetError("Mật khẩu Giảng viên không chính xác!");
                    return;
                }
            }

            // ==========================================
            // XÁC NHẬN HỢP LỆ & CHUẨN BỊ KẾT NỐI
            // ==========================================
            _loginInProgress = true;
            _model.SetUserData(inputName, _isSelectingLecturer);

            // --- ĐOẠN NÀY ĐỂ PLAYER_SPAWNER ĐỌC ĐƯỢC ---
            PlayerPrefs.SetInt("IsLecturer", _isSelectingLecturer ? 1 : 0);
            PlayerPrefs.SetString("PlayerName", inputName);
            PlayerPrefs.Save();
            
            // Thông báo trên UI để người dùng biết đang kết nối
            loginView.SetError("Đang kết nối vào khuôn viên UTT...");

            Debug.Log($"[LoginSuccess] Tên: {_model.DisplayName} | Giảng viên: {_model.IsLecturer}");

            // Vẫn giữ event cũ để các hệ thống khác (như âm thanh) lắng nghe nếu cần
            try
            {
                OnLoginSuccess?.Invoke(_model.DisplayName, _model.IsLecturer);
            }
            catch (Exception ex)
            {
                Debug.LogError("[LoginController] Lỗi khi phát OnLoginSuccess:\n" + ex);
            }

            // ==========================================
            // GỌI HỆ THỐNG MẠNG (PHOTON FUSION)
            // ==========================================
            if (NetworkManager.Instance != null)
            {
                await NetworkManager.Instance.StartCampusSession(_model.DisplayName, _model.IsLecturer);
                
                // Sau khi kết nối thành công, hệ thống mạng tự chuyển Scene, lúc này ta mới ẩn UI
                loginView.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("[LoginController] Không tìm thấy NetworkManager trong Scene!");
                loginView.SetError("Lỗi: Không tìm thấy hệ thống mạng!");
                _loginInProgress = false; // Mở khóa để cho phép thử lại
            }
        }
    }
}