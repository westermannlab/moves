using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFormDisplay : SpriteObject
{
    private readonly int _levelHash = Animator.StringToHash("_Level");

    public Transform CornerTf;
    
    private Animator _animator;
    private Vector3 _currentEulerRotation;
    private float _currentLevel;
    private float _rotationTimer;
    private float _currentAngle;
    private float _angleHolder;
    private int _rotationId;
    private bool _isActive;

    public override void Init()
    {
        base.Init();
        _animator = GetComponent<Animator>();
    }

    public void Show()
    {
        _isActive = true;
        Tf.parent = Controllers.Ui.UiTf;
        var offPosition = Controllers.Ui.GetWaveFormDisplayPosition() + Vector3.down * 1.5f;
        SetLocalPosition(offPosition);
        MoveDelta(Vector3.up * 1.5f, 0.1875f);
        RotateCorner(30f, 0.1875f);
        Sr.size = new Vector2(1f, 0.34375f);
        StartCoroutine(ShowRoutine());
    }

    public void Hide()
    {
        _isActive = false;
        MoveDelta(Vector3.down * 1.5f, 0.125f);
        RotateCorner(-30f, 0.125f);
        Return(0.25f);
    }

    private void UpdateLevel()
    {
        _animator.SetFloat(_levelHash, Mathf.InverseLerp(-6.25f, -1f, Mathf.Log10(_currentLevel)));
    }

    private void RotateCorner(float targetAngle, float duration)
    {
        _rotationId = Utility.AddOne(_rotationId);
        StartCoroutine(RotateCornerRoutine(targetAngle, duration, _rotationId));
    }

    private void ApplyCornerAngle()
    {
        _currentEulerRotation.z = _currentAngle;
        CornerTf.localRotation = Quaternion.Euler(_currentEulerRotation);
    }

    private IEnumerator ShowRoutine()
    {
        while (_isActive)
        {
            //_currentLevel = References.Recorder.GetLevelMax();
            UpdateLevel();
            yield return null;
        }

        yield return null;
    }

    private IEnumerator RotateCornerRoutine(float targetAngle, float duration, int id)
    {
        _angleHolder = _currentAngle;
        while (_rotationTimer < duration && _rotationId == id)
        {
            _rotationTimer += Time.deltaTime;
            _currentAngle = Mathf.Lerp(_angleHolder, targetAngle, _rotationTimer / duration);
            ApplyCornerAngle();
            yield return null;
        }

        if (_rotationId == id)
        {
            _currentAngle = targetAngle;
            ApplyCornerAngle();
        }
        
        yield return null;
    }
}
