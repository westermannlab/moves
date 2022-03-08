using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelExplosion : PoolObject
{
    public Gradient[] Gradients;
    
    private Pixel[] _pixels;
    private PixelCollection[] _pixelCollections;
    private const float MinForceAmount = 16f;
    private const float MaxForceAmount = 32f;
    private int _colorId;
    
    protected override void Awake()
    {
        base.Awake();
        _pixels = GetComponentsInChildren<Pixel>();
        _pixelCollections = GetComponentsInChildren<PixelCollection>();
    }

    public override void Reset()
    {
        foreach(var pixel in _pixels)
        {
            pixel.ResetPosition();
            pixel.SetScale(2f);
            pixel.SetAlpha(1f);
        }
        base.Reset();
    }

    public void Explode()
    {
        foreach (var pixel in _pixels)
        {
            pixel.AddForce(Random.insideUnitCircle, Random.Range(MinForceAmount, MaxForceAmount));
            pixel.Fade(0f, 3f);
            pixel.ChangeGravity(1f, 0.25f);
            if (Random.value > 0.75f)
            {
                pixel.SetSpriteSortingOrder(43);
            }
        }

        StartCoroutine(DestroyRoutine(3f));
    }

    public void SetColor(int colorId)
    {
        if (colorId < 0 || _colorId >= Gradients.Length)
        {
            return;
        }
        
        _colorId = colorId;
        for (var i = 0; i < _pixelCollections.Length; i++)
        {
            foreach (var pixel in _pixelCollections[i].GetPixels())
            {
                pixel.SetColor(Gradients[colorId].Evaluate(i * 0.25f + 0.05f));
            }
        }
    }

    private IEnumerator DestroyRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        yield return null;
        Return();
    }
}
