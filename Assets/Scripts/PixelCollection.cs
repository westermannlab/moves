using UnityEngine;

public class PixelCollection : MonoBehaviour
{
    private Pixel[] _pixels;
    
    private void Awake()
    {
        _pixels = GetComponentsInChildren<Pixel>();
    }

    public Pixel[] GetPixels()
    {
        return _pixels;
    }
}
