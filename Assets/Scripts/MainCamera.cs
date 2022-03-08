using Cinemachine;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Camera ActualCamera;
    private CinemachineVirtualCamera _virtualCamera;
    private Transform _targetTf;

    private Transform _tf;
    private Vector3 _currentPosition;
    private Vector2 _targetPosition;

    private void Awake()
    {
        _tf = transform;
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        _currentPosition = _tf.position;
    }
    
    private void LateUpdate()
    {
        if (_targetTf != null)
        {
            _currentPosition.x = ActualCamera.transform.position.x;
            _currentPosition.y = ActualCamera.transform.position.y;
            ApplyCurrentPosition();
        }
    }

    public void SetCameraAngle(float angle)
    {
        _tf.localRotation = Quaternion.Euler(Vector3.forward * angle);
    }
    
    public void SetTarget(ControllableObject controllableObject)
    {
        _targetTf = controllableObject.Tf;
        _virtualCamera.Follow = _targetTf;
    }

    private void ApplyCurrentPosition()
    {
        //_tf.position = _currentPosition;
        Controllers.Camera.UpdateOffsets(_currentPosition.x, _currentPosition.y);
    }
}
