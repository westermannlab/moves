using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public MainCamera MainCamera;
    public GroundCamera GroundCamera;
    private CinemachineVirtualCamera _virtualCamera;

    [Header("Layers")]
    public LayerMask UiLayerMask;

    public Transform ForemostForegroundTf;
    public Transform ForegroundTf;
    public Transform VeryNearBackgroundTf;
    public Transform NearBackgroundTf;
    public Transform NotSoNearBackgroundTf;
    public Transform NotSoFarBackgroundTf;
    public Transform FarBackgroundTf;
    public Transform VeryFarBackgroundTf;

    private Camera _camera;

    private Vector3 _foremostForegroundPosition;
    private Vector3 _foregroundPosition;

    private Vector3 _veryNearBackgroundPosition;
    private Vector3 _nearBackgroundPosition;
    private Vector3 _notSoNearBackgroundPosition;
    
    private Vector3 _notSoFarBackgroundPosition;
    private Vector3 _farBackgroundPosition;
    private Vector3 _veryFarBackgroundPosition;

    public int PixelWidth => _camera.pixelWidth;
    public int PixelHeight => _camera.pixelHeight;
    public int UiLayer => Mathf.RoundToInt(Mathf.Log(UiLayerMask, 2));
    public float OrthographicSize => _virtualCamera != null ? _virtualCamera.m_Lens.OrthographicSize : 5f;

    private void Awake()
    {
        // in main menu, where no main camera is defined, we don't need any of this
        if (MainCamera == null)
        {
            _camera = FindObjectOfType<Camera>();
            return;
        }
        
        _camera = MainCamera.ActualCamera;
        _virtualCamera = MainCamera.GetComponent<CinemachineVirtualCamera>();

        _foremostForegroundPosition = ForemostForegroundTf.position;
        _foregroundPosition = ForegroundTf.position;
        
        _veryNearBackgroundPosition = VeryNearBackgroundTf.position;
        _nearBackgroundPosition = NearBackgroundTf.position;
        _notSoNearBackgroundPosition = NotSoNearBackgroundTf.position;

        _notSoFarBackgroundPosition = NotSoFarBackgroundTf.position;
        _farBackgroundPosition = FarBackgroundTf.position;
        _veryFarBackgroundPosition = VeryFarBackgroundTf.position;
    }
    
    public void UpdateOffsets(float xPosition, float yPosition)
    {
        // foremost foreground is a child of foreground
        _foremostForegroundPosition.x = xPosition * -0.1f;
        ForemostForegroundTf.localPosition = _foremostForegroundPosition;
        
        _foregroundPosition.x = xPosition * -0.2f;
        _foregroundPosition.y = yPosition - 2f;
        ForegroundTf.localPosition = _foregroundPosition;
        
        _veryNearBackgroundPosition.x = xPosition * 0.1f;
        VeryNearBackgroundTf.localPosition = _veryNearBackgroundPosition;
        
        _nearBackgroundPosition.x = xPosition * 0.2f;
        NearBackgroundTf.localPosition = _nearBackgroundPosition;

        _notSoNearBackgroundPosition.x = xPosition * 0.3f;
        NotSoNearBackgroundTf.localPosition = _notSoNearBackgroundPosition;

        _notSoFarBackgroundPosition.x = xPosition * 0.7f;
        _notSoFarBackgroundPosition.y = yPosition - 2f;
        NotSoFarBackgroundTf.localPosition = _notSoFarBackgroundPosition;

        _farBackgroundPosition.x = xPosition * 0.8f;
        _farBackgroundPosition.y = yPosition - 2f;
        FarBackgroundTf.localPosition = _farBackgroundPosition;

        _veryFarBackgroundPosition.x = xPosition * 0.9f;
        _veryFarBackgroundPosition.y = yPosition - 2f;
        VeryFarBackgroundTf.localPosition = _veryFarBackgroundPosition;
    }
}
