using Fusion;
using UnityEngine;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour
{
    // Cấu trúc Singleton giúp gọi lệnh từ mọi nơi
    public static NetworkManager Instance { get; private set; }
    
    private NetworkRunner _runner;

    private void Awake()
    {
        // Giữ cho GameObject này không bị xóa khi chuyển từ Login sang Campus
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else 
        { 
            Destroy(gameObject); 
        }
    }

    public async Task StartCampusSession(string username, bool isLecturer)
    {
        // Khởi tạo Runner nếu chưa có
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        // Bắt buộc phải có SceneManager để Fusion tự động load map
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        // Lưu tạm thông tin để khi vào Campus, hệ thống sinh nhân vật sẽ đọc và gán quyền
        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.SetInt("IsLecturer", isLecturer ? 1 : 0);

        // Thiết lập thông số phòng
        var args = new StartGameArgs()
        {
            GameMode = GameMode.Shared, // Chế độ Shared lý tưởng cho Campus
            SessionName = "UTTCampus", 
            Scene = SceneRef.FromIndex(1), // Số 1 là thứ tự của Scene Campus trong Build Settings
            SceneManager = sceneManager
        };

        Debug.Log("Đang kết nối máy chủ...");
        await _runner.StartGame(args);
    }
}