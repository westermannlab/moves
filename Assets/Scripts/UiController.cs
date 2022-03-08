using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiController : MonoBehaviour
{
    public bool ShowHotkeys;
    public Transform UiTf;
    public FpsDisplay FpsDisplay;

    private const int FrameSizeInPixels = 10;

    private FadePlane _fadePlane;
    private Resolution _currentResolution;
    private Vector3 _textBoxAnchor;
    private float _orthographicCameraSize;

    private void Start()
    {
        _orthographicCameraSize = Controllers.Camera.OrthographicSize;
        
        _fadePlane = UiTf.GetComponentInChildren<FadePlane>();

        ApplyToResolution();
        FadeIn(1f);
    }
    
    public void ApplyToResolution()
    {
        if (References.Terminal != null)
        {
            References.Terminal.SetLocalPosition(new Vector3(-_orthographicCameraSize * GetScreenRatio() + Utility.PixelsToUnit(FrameSizeInPixels), -_orthographicCameraSize + Utility.PixelsToUnit(FrameSizeInPixels) + 0.25f, References.Terminal.transform.localPosition.z));
        }

        if (References.Timer != null)
        {
            References.Timer.SetLocalPosition(new Vector3(-_orthographicCameraSize * GetScreenRatio() + Utility.PixelsToUnit(FrameSizeInPixels) + 0.6875f, _orthographicCameraSize - 1f, 0f));
        }

        if (References.Menu != null)
        {
            References.Menu.SetLocalPosition(new Vector3(0f, -_orthographicCameraSize, 0f));    
        }
        
        _textBoxAnchor = new Vector3(_orthographicCameraSize * GetScreenRatio() - 0.375f, _orthographicCameraSize - 0.375f, 0f);
        _fadePlane.SetLocalPosition(new Vector3(-_orthographicCameraSize * GetScreenRatio() - 0.25f, -_orthographicCameraSize - 0.25f, 0f));
        _fadePlane.SetTileSize(new Vector2(_orthographicCameraSize * 2f * GetScreenRatio() + 0.5f, _orthographicCameraSize * 2f + 0.5f));
    }

    public Vector2 GetLowerLeftCorner()
    {
        return new Vector2(-_orthographicCameraSize * GetScreenRatio() + Utility.PixelsToUnit(FrameSizeInPixels), -_orthographicCameraSize + Utility.PixelsToUnit(FrameSizeInPixels));
    }

    public Vector2 GetUpperRightCorner()
    {
        return new Vector2(_orthographicCameraSize * GetScreenRatio() - Utility.PixelsToUnit(FrameSizeInPixels), _orthographicCameraSize - Utility.PixelsToUnit(FrameSizeInPixels));
    }
    
    public float GetScreenRatio ()
    {
        return Controllers.Camera.PixelWidth / (float)Controllers.Camera.PixelHeight;
    }

    public Vector2 GetResolution ()
    {
        return new Vector2(Controllers.Camera.PixelWidth, Controllers.Camera.PixelHeight);
    }

    public void SetFpsDisplayActive(bool isActive)
    {
        FpsDisplay.SetActive(isActive);
    }

    public void FadeIn(float duration)
    {
        _fadePlane.Deactivate(duration);
    }

    public void FadeOut(float duration)
    {
        _fadePlane.Activate(duration);
    }

    public Vector3 GetWaveFormDisplayPosition()
    {
        return new Vector3(_orthographicCameraSize * GetScreenRatio() - (Utility.PixelsToUnit(FrameSizeInPixels) + 0.6125f), -_orthographicCameraSize + Utility.PixelsToUnit(FrameSizeInPixels) + 0.375f, 10f);
    }

    public Vector3 GetTextBoxPosition(float boxWidth, float boxHeight)
    {
        return new Vector3(_textBoxAnchor.x - boxWidth / 2f, _textBoxAnchor.y - boxHeight / 2f, _textBoxAnchor.z);
    }
}
