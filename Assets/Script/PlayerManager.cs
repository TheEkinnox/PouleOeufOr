using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private PlayerInputManager _playerInputManager;
    private Dictionary<Transform, int> _prefabsMap;
    private Queue<int> _unusedPrefabs;

    private void Awake()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();

        if (_playerInputManager.maxPlayerCount > 0)
            Debug.Assert(playerPrefabs.Length >= _playerInputManager.maxPlayerCount,
                $"{name} - Prefabs count is smaller than max player count");

        Debug.Assert(spawnPoints.Length > 0, $"{name} - No spawn points");

        ResetUnused();

        _playerInputManager.playerPrefab = playerPrefabs[_unusedPrefabs.Peek()];
        _prefabsMap ??= new Dictionary<Transform, int>(playerPrefabs.Length);
    }

    public void OnPlayerJoin(PlayerInput player)
    {
        int index = _unusedPrefabs.Dequeue();
        _prefabsMap[player.transform] = index;

        if (_unusedPrefabs.Count == 0)
            ResetUnused();

        _playerInputManager.playerPrefab = playerPrefabs[_unusedPrefabs.Peek()];

        Transform spawnPoint = spawnPoints[(_playerInputManager.playerCount - 1) % spawnPoints.Length];
        player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }

    public void OnPlayerLeave(PlayerInput player)
    {
        if (_playerInputManager.maxPlayerCount <= 0)
            return;

        _unusedPrefabs.Enqueue(_prefabsMap[player.transform]);
        _prefabsMap.Remove(player.transform);
    }

    private void ResetUnused()
    {
        if (_unusedPrefabs == null)
            _unusedPrefabs = new Queue<int>(playerPrefabs.Length);
        else
            _unusedPrefabs.Clear();

        for (int i = 0; i < playerPrefabs.Length; i++)
            _unusedPrefabs.Enqueue(i);

        if (_playerInputManager.maxPlayerCount > 0)
            playerPrefabs = playerPrefabs.OrderBy(_ => Random.Range(0, playerPrefabs.Length)).ToArray();
    }
}
