using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public const float PixelToUnit = 0.03125f;
    private const int PixelsPerUnit = 32;
    private const int PixelSteps = PixelsPerUnit * 4;

    public static float PixelsToUnit(float pixel)
    {
        return pixel / PixelsPerUnit;
    }
    
    public static Vector2 Snap(Vector2 v)
    {
        return new Vector2(Mathf.Round(v.x * PixelSteps) / PixelSteps, Mathf.Round(v.y * PixelSteps) / PixelSteps);
    }
    
    public static void DrawPoint(Vector2 point, Color color, float duration = 60f)
    {
        Debug.DrawLine(point + new Vector2(-0.05f, 0.05f), point + new Vector2(0.05f, -0.05f), color, duration);
        Debug.DrawLine(point + new Vector2(-0.05f, -0.05f), point + new Vector2(0.05f, 0.05f), color, duration);
    }

    public static float EaseInOut(float progress)
    {
        return Mathf.Sin(progress * Mathf.PI + 3f * Mathf.PI / 2f) / 2f + 0.5f;
        // return (Mathf.Sin(-Mathf.PI * 0.5f + progress * Mathf.PI) + 1f) / 2f;
    }
    
    public static int AddOne(int id)
    {
        return id = (id + 1) % 256;
    }

    public static string Timestamp()
    {
        var now = DateTime.Now;
        return string.Concat(now.Year, '-', now.Month.ToString("D2"), '-', now.Day.ToString("D2"), '-', now.Hour.ToString("D2"), '-', now.Minute.ToString("D2"), '-', now.Second.ToString("D2"));
    }

    public static string FormatTime(float seconds)
    {
        var rounded = Mathf.CeilToInt(Math.Max(seconds, 0f));
        var min = rounded / 60;
        var sec = rounded % 60;
        return string.Concat(min, ':', sec.ToString("00"));
    }
    
    public static float Mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
