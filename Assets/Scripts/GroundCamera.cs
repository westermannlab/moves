using UnityEngine;

public class GroundCamera : MonoBehaviour
{   
    public Camera ActualCamera;
    private Transform _targetTf;

    private Transform _tf;
    private Vector3 _currentPosition;

    private void Awake()
    {
        _tf = transform;
        _currentPosition = _tf.position;
    }

    public void SetCameraAngle(float angle)
    {
        _tf.localRotation = Quaternion.Euler(Vector3.forward * angle);
    }

    private void ApplyCurrentPosition()
    {
        //_tf.position = _currentPosition;
        // Controllers.Camera.UpdateOffsets(_currentPosition.x, _currentPosition.y);
    }
}
