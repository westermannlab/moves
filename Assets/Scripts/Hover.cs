using UnityEngine;

public class Hover : MonoBehaviour
{
    public float MaxDelta = 0.125f;
    public float Speed = 1f;

    private Transform _tf;
    private Vector2 _basePosition;
    private Vector2 _currentPosition;
    private float _timer;
    private float _currentYDelta;

    private void Awake()
    {
        _tf = transform;
        _basePosition = _tf.localPosition;
        _currentPosition = _basePosition;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        _currentYDelta = Mathf.Sin(_timer * Mathf.PI * Speed) * MaxDelta;
        _currentPosition = _basePosition + Vector2.up * _currentYDelta;
        ApplyCurrentOffset();
    }

    private void ApplyCurrentOffset()
    {
        _tf.localPosition = _currentPosition;
    }
}
