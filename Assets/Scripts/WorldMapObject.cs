using System.Collections;
using UnityEngine;

public class WorldMapObject : MonoBehaviour
{
    public int SoulIndex;
    
    protected Player Inhabitant;
    protected Transform Tf;
    protected Vector2 CenterPosition;
    protected Vector2 Offset;
    protected float MoveTimer;
    
    protected float MaxAlpha;
    private float _alphaHolder;
    private float _changeAlphaTimer;
    private int _changeAlphaId;

    protected virtual void Awake()
    {
        Tf = transform;
        CenterPosition = Tf.localPosition;
    }
    
    private void Start()
    {
        var data = References.Io.GetData();
        if (SoulIndex >= 0 && SoulIndex < data.roomOrder.Length && data.roomOrder[SoulIndex] < References.Entities.Players.Length)
        {
            Inhabitant = References.Entities.Players[data.roomOrder[SoulIndex]];
        }
        else
        {
            Inhabitant = References.Entities.Players[6];
        }
        ChangeMaxAlpha(0.125f, 0f);
    }
    
    private void OnEnable()
    {
        References.Events.OnSelectLevelButton += ChangeLevelButtonSelectionState;
    }

    private void OnDisable()
    {
        References.Events.OnSelectLevelButton -= ChangeLevelButtonSelectionState;
    }

    protected virtual void Update()
    {
        MoveTimer += Time.deltaTime;
    }
    
    private void ChangeLevelButtonSelectionState(Player inhabitant, bool isSelected)
    {
        if (inhabitant == Inhabitant)
        {
            ChangeMaxAlpha(isSelected ? 1f : 0.125f, 0.25f);
        }
    }
    
    private void ChangeMaxAlpha(float targetAlpha, float duration)
    {
        _changeAlphaId = Utility.AddOne(_changeAlphaId);
        StartCoroutine(ChangeAlphaRoutine(targetAlpha, duration, _changeAlphaId));
    }

    protected virtual void ApplyMaxAlpha()
    {
        
    }

    private IEnumerator ChangeAlphaRoutine(float targetAlpha, float duration, int id)
    {
        _changeAlphaTimer = 0f;
        _alphaHolder = MaxAlpha;
        while (_changeAlphaTimer < duration && _changeAlphaId == id)
        {
            _changeAlphaTimer += Time.deltaTime;
            MaxAlpha = Mathf.Lerp(_alphaHolder, targetAlpha, _changeAlphaTimer / duration);
            ApplyMaxAlpha();
            yield return null;
        }

        if (_changeAlphaId == id)
        {
            MaxAlpha = targetAlpha;
            ApplyMaxAlpha();
        }
        
        yield return null;
    }
}
