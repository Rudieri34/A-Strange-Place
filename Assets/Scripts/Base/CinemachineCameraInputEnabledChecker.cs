using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraInputEnabledChecker : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController _cinemachineInputAxisController;
    void Update()
    {
        _cinemachineInputAxisController.enabled = GameManager.Instance.IsPlayerInputEnabled;
    }
}
