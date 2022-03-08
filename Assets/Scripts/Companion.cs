using System.IO;
using UnityEngine;

public class Companion : PoolObject
{
    public Texture2D SpriteSheet;
    private string _path;

    public override void Init()
    {
        base.Init();
        _path = string.Concat(Application.streamingAssetsPath, "/", "data.moves");
    }

    public override void SetPosition(Vector2 position)
    {
        base.SetPosition(position);
        DecryptImage(SpriteSheet);
    }

    private void EncryptImage(Texture2D texture2D)
    {
        if (!File.Exists(_path))
        {
            File.Create(_path);
        }

        using (var stream = new FileStream(_path, FileMode.Truncate))
        {
            using (var writer = new StreamWriter(stream))
            {
                for (var y = 0; y < texture2D.height; y++)
                {
                    for (var x = 0; x < texture2D.width; x++)
                    {
                        var colorString = ColorUtility.ToHtmlStringRGBA(texture2D.GetPixel(x, y));
                        writer.WriteLine(string.Concat('#', colorString));
                    }   
                }
            }
        }
    }
    
    private void DecryptImage(Texture2D texture2D)
    {
        if (!File.Exists(_path))
        {
            return;
        }

        var reader = new StreamReader(_path, true);
        for (var y = 0; y < texture2D.height; y++)
        {
            for (var x = 0; x < texture2D.width; x++)
            {
                var colorString = reader.ReadLine();
                ColorUtility.TryParseHtmlString(colorString, out var color);
                texture2D.SetPixel(x, y, color);
            }   
        }
        texture2D.Apply();
        reader.Close();
    }
        
}
