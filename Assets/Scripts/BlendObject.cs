using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendObject : ControllableObject
{
    protected readonly int BlendHash = Shader.PropertyToID("_Blend");
    protected readonly int OffsetXHash = Shader.PropertyToID("_OffsetX");
    protected readonly int OffsetYHash = Shader.PropertyToID("_OffsetY");
    protected readonly int PreviousOffsetXHash = Shader.PropertyToID("_PreviousOffsetX");
    protected readonly int PreviousOffsetYHash = Shader.PropertyToID("_PreviousOffsetY");
    
    protected MaterialPropertyBlock Block;

    protected Vector2Int CurrentBlendOffsets;
    private float _blendTimer;
    private float _currentBlendValue;
    private float _blendValueHolder;
    private int _blendId;

    protected override void Awake()
    {
        base.Awake();
        Block = new MaterialPropertyBlock();
    }
    
    public override void Enter(Player player)
    {
        base.Enter(player);
        SetBlendOffsets(player.BlendOffset.x, player.BlendOffset.y);
        _currentBlendValue = 0f;
        Blend(1f, 1f);
    }

    public override void Leave(Player player)
    {
        if (player == CurrentPlayer)
        {
            SetBlendOffsets(0, 0);
            _currentBlendValue = 0f;
            Blend(1f, 1f);
        }
        base.Leave(player);
    }

    protected virtual void SetBlendOffsets(int offsetX, int offsetY)
    {
        Sr.GetPropertyBlock(Block);
        Block.SetInt(PreviousOffsetXHash, CurrentBlendOffsets.x);
        Block.SetInt(PreviousOffsetYHash, CurrentBlendOffsets.y);
        CurrentBlendOffsets.x = offsetX;
        CurrentBlendOffsets.y = offsetY;
        Block.SetInt(OffsetXHash, offsetX);
        Block.SetInt(OffsetYHash, offsetY);
        Sr.SetPropertyBlock(Block);
    }

    protected void Blend(float targetValue, float duration)
    {
        _blendId++;
        StartCoroutine(BlendRoutine(targetValue, duration, _blendId));
    }
    
    protected virtual void ApplyCurrentBlendValue(float blendValue)
    {
        Sr.GetPropertyBlock(Block);
        Block.SetFloat(BlendHash, blendValue);
        Sr.SetPropertyBlock(Block);
    }
    
    private IEnumerator BlendRoutine(float targetValue, float duration, int id)
    {
        _blendTimer = 0f;
        _blendValueHolder = _currentBlendValue;
        
        while (_blendTimer < duration && _blendId == id)
        {
            _blendTimer += Time.deltaTime;
            _currentBlendValue = Mathf.Lerp(_blendValueHolder, targetValue, _blendTimer / duration);
            ApplyCurrentBlendValue(_currentBlendValue);
            yield return null;
        }

        if (_blendId == id)
        {
            _currentBlendValue = targetValue;
            ApplyCurrentBlendValue(_currentBlendValue);
        }
        
        yield return null;
    }
}
