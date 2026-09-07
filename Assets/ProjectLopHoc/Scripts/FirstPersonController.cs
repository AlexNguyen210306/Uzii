using Fusion;
using UnityEngine;

public class FirstPersonController : NetworkBehaviour
{
    private NetworkCharacterController _ncc;
    private Camera _mainCamera;

    [Header("Cấu hình di chuyển & Chuột")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 1.5f;
    private float _verticalRotation = 0f;

    [Header("Cấu hình Tương tác (Raycast)")]
    public float interactionRange = 4f;       // Khoảng cách tối đa để chạm tới vật thể
    public LayerMask interactableLayer = ~0;   // Mặc định quét tất cả các layer

    private bool _jumpRequested = false;
    private InteractableObject _currentHoveredObject;

    private void Awake()
    {
        _ncc = GetComponent<NetworkCharacterController>();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            _mainCamera = Camera.main;
            if (_mainCamera != null)
            {
                _mainCamera.transform.SetParent(transform);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (!HasStateAuthority || _mainCamera == null) return;

        // 1. Nhận lệnh Nhảy
        if (Input.GetButtonDown("Jump"))
        {
            _jumpRequested = true;
        }

        // 2. Xử lý quét Raycast phát hiện vật thể qua tâm ngắm (Crosshair)
        HandleRaycastInteraction();
    }

    private void HandleRaycastInteraction()
    {
        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
        {
            InteractableObject target = hit.collider.GetComponentInParent<InteractableObject>();

            if (target != null)
            {
                // Nếu di chuyển tâm từ vật thể này sang vật thể khác
                if (_currentHoveredObject != target)
                {
                    ClearCurrentHover();
                    _currentHoveredObject = target;
                    _currentHoveredObject.OnHoverEnter();
                }

                // Nhận diện click: Chuột trái (0) hoặc phím E
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
                {
                    target.OnInteract();
                }

                return;
            }
        }

        // Nếu tia ray bắn ra ngoài không trúng vật thể tương tác nào
        ClearCurrentHover();
    }

    private void ClearCurrentHover()
    {
        if (_currentHoveredObject != null)
        {
            _currentHoveredObject.OnHoverExit();
            _currentHoveredObject = null;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        // Xoay hướng nhìn
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        _verticalRotation -= mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -85f, 85f);

        // Di chuyển WASD
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (transform.forward * moveVertical + transform.right * moveHorizontal).normalized;
        _ncc.Move(moveDirection * moveSpeed * Runner.DeltaTime);

        // Nhảy
        if (_jumpRequested)
        {
            _ncc.Jump();
            _jumpRequested = false;
        }
    }

    private void LateUpdate()
    {
        if (HasStateAuthority && _mainCamera != null)
        {
            _mainCamera.transform.position = transform.position + new Vector3(0, 1.65f, 0);
            _mainCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }
    }
}