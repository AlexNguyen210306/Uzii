using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion; // Cần dùng thư viện Fusion để chuyển Scene đồng bộ mạng

public class InteractableObject : MonoBehaviour
{
    [Header("Thông tin Tương tác")]
    public string objectName = "Cửa Lớp Học";
    
    [Header("Hiển thị Highlight")]
    public Renderer highlightRenderer; 

    [Header("Chuyển Scene (Nếu là Cửa)")]
    [Tooltip("Để trống nếu không phải là cửa. Nhập chính xác tên Scene cần đến, ví dụ: Classroom")]
    public string targetSceneName = "Classroom";

    private void Awake()
    {
        if (highlightRenderer == null)
        {
            highlightRenderer = GetComponent<Renderer>();
        }
        
        if (highlightRenderer != null)
        {
            highlightRenderer.enabled = false;
        }
    }

    public void OnHoverEnter()
    {
        if (highlightRenderer != null)
        {
            highlightRenderer.enabled = true;
        }
    }

    public void OnHoverExit()
    {
        if (highlightRenderer != null)
        {
            highlightRenderer.enabled = false;
        }
    }

    public virtual void OnInteract()
    {
        Debug.Log($"[Đã tương tác]: {objectName} ({gameObject.name})");

        // Nếu có khai báo Scene đích, thực hiện chuyển Scene
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            ChuyenScene(targetSceneName);
        }
    }

    private void ChuyenScene(string sceneName)
    {
        NetworkRunner runner = FindAnyObjectByType<NetworkRunner>();
        
        // Nếu đang chạy mạng Photon Fusion
        if (runner != null && runner.IsRunning)
        {
            int sceneIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
            if (sceneIndex >= 0)
            {
                runner.LoadScene(SceneRef.FromIndex(sceneIndex));
            }
            else
            {
                Debug.LogError($"[InteractableObject] Chưa thêm Scene '{sceneName}' vào Build Settings!");
            }
        }
        else
        {
            // Dự phòng khi chạy đơn máy test cục bộ
            SceneManager.LoadScene(sceneName);
        }
    }
}