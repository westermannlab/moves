using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public int Width = 24;
    public int Height = 16;

    private Transform _tf;
    private BackgroundTile[,] _tiles;
    private BackgroundTile _tile;

    private int _x;
    private int _y;

    private void Awake()
    {
        _tf = transform;
        _tiles = new BackgroundTile[Width,Height];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _tile = References.Prefabs.GetBackgroundTile();
                _tile.SetParent(_tf);
                _tile.SetScale(1f);
                _tile.SetLocalPosition(x * Utility.PixelsToUnit(24), y * Utility.PixelsToUnit(24));
                _tile.UpdateTile(GetCode(x, y));
               // _tile.MoveDelta(new Vector2(-32f, -24f), 80f);
               // _tile.Return(80.25f);
                _tiles[x, y] = _tile;
            }
        }

        //StartCoroutine(SpawnTilesRoutine(2f));
    }

    private void Update()
    {
        //_tf.localPosition += new Vector3(-1f, -1f) * Time.deltaTime * 0.25f;
    }

    private int GetCode(int x, int y)
    {
        var code = 0;
        
        code += y > 0 && (_tiles[x, y - 1].Code & 1) == 1 ? 4 : 0;
        code += x > 0 && (_tiles[x - 1, y].Code & 2) == 2 ? 8 : 0;

        if (code > 8)
        {
            code += Random.value > 0.5f ? 1 : 0;
            code += Random.value > 0.5f ? 2 : 0;
        }
        else if (code > 0)
        {
            code += Random.value > 0.5f ? 1 : 2;
        }
        else
        {
            code += Random.value > 0.5f ? 3 : 0;
        }
        
        return code;
    }

    private IEnumerator SpawnTilesRoutine(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (y != Height - 1 && x != Width - 1) continue;
                    _tile = References.Prefabs.GetBackgroundTile();
                    _tile.SetParent(_tf);
                    _tile.SetLocalPosition(x * Utility.PixelsToUnit(24), y * Utility.PixelsToUnit(24));
                    _tile.UpdateTile(GetCode(x, y));
                    _tile.MoveDelta(new Vector2(-32f, -24f), 80f);
                    _tile.Return(80.25f);
                }
            }
        }

        yield return null;
    }
    
    
}
