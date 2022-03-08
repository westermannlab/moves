using UnityEngine;

// source: https://forum.unity.com/threads/bezier-curve.5082/

public class Bezier {

    private readonly Vector2 _p0;
    private readonly Vector2 _p1;
    private readonly Vector2 _p2;

    private readonly Transform _t2;
    private readonly ControllableObject _co2;

    private Vector2 _q0;
    private Vector2 _q1;

    public Bezier(Vector2 v0, Vector2 v1, Vector2 v2){
        _p0 = v0;
        _p1 = v1;
        _p2 = v2;
    }

    public Bezier(Vector2 v0, Vector2 v1, Transform t2)
    {
        _p0 = v0;
        _p1 = v1;
        _t2 = t2;
    }
    
    public Bezier(Vector2 v0, Vector2 v1, ControllableObject co2)
    {
        _p0 = v0;
        _p1 = v1;
        _co2 = co2;
    }

    public Vector2 GetPointAtTime(float t) {
        t = Mathf.Clamp01(t);
        _q0 = Vector2.Lerp(_p0, _p1, t);
        _q1 = Vector2.Lerp(_p1, _p2, t);
        return Vector2.Lerp(_q0, _q1, t);
    }

    public Vector2 GetPointAtTimeTransform(float t)
    {
        t = Mathf.Clamp01(t);
        _q0 = Vector2.Lerp(_p0, _p1, t);
        _q1 = Vector2.Lerp(_p1, _t2.position, t);
        return Vector2.Lerp(_q0, _q1, t);
    }
    
    public Vector2 GetPointAtTimeControllableObject(float t)
    {
        t = Mathf.Clamp01(t);
        _q0 = Vector2.Lerp(_p0, _p1, t);
        _q1 = Vector2.Lerp(_p1, _co2.transform.position, t);
        return Vector2.Lerp(_q0, _q1, t);
    }

    public void DrawDebugPoint(float t)
    {
        t = Mathf.Clamp01(t);
        _q0 = Vector2.Lerp(_p0, _p1, t);
        _q1 = Vector2.Lerp(_p1, _p2, t);
        Utility.DrawPoint(Vector2.Lerp(_q0, _q1, t), new Color(1f, 0.5f, 0f), 5f);
    }

    public float Length (int precisionLevel = 10)
    {
        if (precisionLevel < 2)
        {
            return -1;
        }
        float length = 0;
        for (var i = 1; i < precisionLevel; i++)
        {
            length += (GetPointAtTime(i / (float)precisionLevel) - GetPointAtTime((i - 1) / (float)precisionLevel)).magnitude;
        }
        return length;
    }
}