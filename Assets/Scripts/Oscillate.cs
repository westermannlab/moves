using UnityEngine;

public class Oscillate : MonoBehaviour
{
    public float MaxAngle = 5f;
    public float Speed = 1f;

    private Transform _tf;
    private Vector3 _eulerRotation;
    private float _timer;
    private float _currentAngle;

    private void Awake()
    {
        _tf = transform;
        // add offset
        _timer = _tf.position.x;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        _currentAngle = Mathf.Sin(_timer * Mathf.PI * Speed) * MaxAngle;
        ApplyCurrentAngle();
    }

    private void ApplyCurrentAngle()
    {
        _eulerRotation.z = _currentAngle;
        _tf.localRotation = Quaternion.Euler(_eulerRotation);
    }
}
