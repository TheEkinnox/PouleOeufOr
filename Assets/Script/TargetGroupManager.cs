using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class TargetGroupManager : MonoBehaviour
{
    [SerializeField] private float playerRadius;

    private CinemachineTargetGroup _targetGroup;

    private void Awake()
    {
        _targetGroup = GetComponent<CinemachineTargetGroup>();
    }

    public void OnPlayerJoin(PlayerInput player)
    {
        _targetGroup.AddMember(player.transform, 1, playerRadius);
    }

    public void OnPlayerLeave(PlayerInput player)
    {
        _targetGroup.RemoveMember(player.transform);
    }
}
