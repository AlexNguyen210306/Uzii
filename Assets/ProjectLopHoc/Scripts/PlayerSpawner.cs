using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("--- Cấu hình Nhân vật ---")]
    public NetworkPrefabRef studentPrefab;
    public NetworkPrefabRef lecturerPrefab;

    [Header("--- Vị trí Spawn ---")]
    public Transform lecturerSpawnPoint;
    public Transform studentSpawnPoint;

    private bool _spawnedInThisScene = false;

    private void Start()
    {
        NetworkRunner runner = FindAnyObjectByType<NetworkRunner>();
        if (runner != null)
        {
            runner.AddCallbacks(this);
        }
    }

    // Khi chuyển Scene hoàn tất, Fusion kích hoạt callback này
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (runner.LocalPlayer.IsValid)
        {
            ThucHienSpawnAsync(runner, runner.LocalPlayer);
        }
    }

    // Dành cho Scene đầu tiên khi người chơi mới kết nối vào phòng
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            ThucHienSpawnAsync(runner, player);
        }
    }

    private async void ThucHienSpawnAsync(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedInThisScene || runner == null || !runner.IsRunning) return;

        bool isLecturer = PlayerPrefs.GetInt("IsLecturer", 0) == 1;
        NetworkPrefabRef prefabToSpawn = isLecturer ? lecturerPrefab : studentPrefab;
        Transform targetPoint = isLecturer ? lecturerSpawnPoint : studentSpawnPoint;

        Vector3 spawnPos = (targetPoint != null) ? targetPoint.position : Vector3.zero;
        Quaternion spawnRot = (targetPoint != null) ? targetPoint.rotation : Quaternion.identity;

        try
        {
            // Sử dụng SpawnAsync để chờ nạp Prefab mà không gây đứng hình / quăng Exception
            await runner.SpawnAsync(prefabToSpawn, spawnPos, spawnRot, player);
            _spawnedInThisScene = true;
            Debug.Log($"[PlayerSpawner] Đã đưa {(isLecturer ? "Giảng viên lên bục" : "Sinh viên vào cửa lớp")} thành công!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PlayerSpawner] Lỗi khi Spawn: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        NetworkRunner runner = FindAnyObjectByType<NetworkRunner>();
        if (runner != null)
        {
            runner.RemoveCallbacks(this);
        }
    }

    // ==========================================
    // CÁC CALLBACK RỖNG CỦA FUSION
    // ==========================================
#pragma warning disable UNT0006
#pragma warning disable CS0618
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
#pragma warning restore CS0618
#pragma warning restore UNT0006
}