using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsDisplay : MonoBehaviour
{
    public Gradient Gradient;
    
    private readonly List<Pixel> _pixelList = new List<Pixel>();
    private Pixel _currentPixel;

    private Transform _tf;
    private Vector2 _anchor;
    private bool _isActive = true;

    public bool IsActive => _isActive;

    private void Awake()
    {
        _tf = transform;
        if (References.Settings.TerminalEnabled)
        {
            StartCoroutine(ShowRoutine());
        }
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
    }

    private IEnumerator ShowRoutine()
    {
        while (References.Settings.TerminalEnabled)
        {
            if (_pixelList.Count >= 60 || _pixelList.Count > 0 && !_isActive)
            {
                _pixelList[0].Return();
                _pixelList.RemoveAt(0);
            }

            foreach (var pixel in _pixelList)
            {
                pixel.MoveLocally(0.03125f * Vector2.left);
            }

            if (_isActive)
            {
                _currentPixel = References.Prefabs.GetPixel();
                _currentPixel.SetParent(_tf);
                _currentPixel.SetScale(1f, 2f);
                var estimatedFps = Mathf.Clamp(1f / Time.deltaTime, 0f, 60f);
                _currentPixel.SetLocalPosition(_anchor + estimatedFps * 0.00625f * Vector2.up);
                _currentPixel.SetColor(Gradient.Evaluate(Mathf.InverseLerp(0f, 60f, estimatedFps)));
                _currentPixel.SetSpriteLayer("Ui");
                _pixelList.Add(_currentPixel);
            }

            yield return null;
        }
    }
}
