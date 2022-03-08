using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Transition
{
    public string other;
    public int priority;
    public int sampleCount;
    public float[] soul;
    public float[] cloud;
    public float[] handcar;
    public float[] ground;
    public float[] cart;
    
    private float[] _row;

    private readonly List<float>[] _drawPots = new List<float>[5];
    private readonly List<float> _tempList = new List<float>();
    private int _shuffleIndex;
    private int _currentDrawPotIndex;

    public void Init()
    {
        if (sampleCount <= 1)
        {
            sampleCount = 20;
        }
    }
    
    public Personality.Role Evaluate(Personality.Role currentRole)
    {
        switch (currentRole)
        {
            case Personality.Role.Soul:
                _row = soul;
                break;
            case Personality.Role.Cloud:
                _row = cloud;
                break;
            case Personality.Role.Handcar:
                _row = handcar;
                break;
            case Personality.Role.Ground:
                _row = ground;
                break;
            case Personality.Role.Cart:
                _row = cart;
                break;
        }

        _currentDrawPotIndex = (int) currentRole;

        var randomNumber = DrawValueFromPot(_currentDrawPotIndex);
        var sum = 0f;
        for (var i = 0; i < _row.Length; i++)
        {
            sum += _row[i];
            if (randomNumber <= sum)
            {
                return (Personality.Role) i;
            }
        }

        return Personality.Role.Soul;
    }

    public bool Evaluate(Personality.Role currentRole, Personality.Motivation motivation)
    {
        var randomNumber = DrawValueFromPot((int) currentRole);
        switch (motivation)
        {
            case Personality.Motivation.Demand:
                switch (currentRole)
                {
                    case Personality.Role.Cloud:
                        if (cloud.Length == 0) return false;
                        return randomNumber <= cloud[0];
                    case Personality.Role.Handcar:
                        if (handcar.Length == 0) return false;
                        return randomNumber <= handcar[0];
                    case Personality.Role.Ground:
                        if (ground.Length == 0) return false;
                        return randomNumber <= ground[0];
                    case Personality.Role.Cart:
                        if (cart.Length == 0) return false;
                        return randomNumber <= cart[0];
                    default:
                        return false;
                }
            case Personality.Motivation.Avoid:
                switch (currentRole)
                {
                    case Personality.Role.Cloud:
                        if (cloud.Length < 2) return false;
                        return randomNumber <= cloud[1];
                    case Personality.Role.Handcar:
                        if (handcar.Length < 2) return false;
                        return randomNumber <= handcar[1];
                    case Personality.Role.Ground:
                        if (ground.Length < 2) return false;
                        return randomNumber <= ground[1];
                    case Personality.Role.Cart:
                        if (cart.Length < 2) return false;
                        return randomNumber <= cart[1];
                    default:
                        return false;
                }
            default:
                return false;
        }
    }

    public Personality.Role GetMostLikelyRole(Personality.Role currentRole)
    {
        switch (currentRole)
        {
            case Personality.Role.Soul:
                _row = soul;
                break;
            case Personality.Role.Cloud:
                _row = cloud;
                break;
            case Personality.Role.Handcar:
                _row = handcar;
                break;
            case Personality.Role.Ground:
                _row = ground;
                break;
            case Personality.Role.Cart:
                _row = cart;
                break;
        }

        _currentDrawPotIndex = (int) currentRole;
        
        var highestChance = 0f;
        var mostLikelyIndex = 0;
        for (var i = 0; i < _row.Length; i++)
        {
            if (_row[i] > highestChance)
            {
                highestChance = _row[i];
                mostLikelyIndex = i;
            }
        }

        return (Personality.Role)mostLikelyIndex;
    }

    private float DrawValueFromPot(int index)
    {
        if (_drawPots[index] == null)
        {
            _drawPots[index] = new List<float>();
        }

        if (_drawPots[index].Count == 0)
        {
            RefillDrawPot(index);
        }

        var value = _drawPots[index][0];
        _drawPots[index].RemoveAt(0);
        return value;
    }

    private void RefillDrawPot(int index)
    {
        if (index < 0 || index >= _drawPots.Length) return;
        
        // just to be sure...
        _drawPots[index].Clear();
        _tempList.Clear();
        
        for (var i = 1; i <= sampleCount; i++)
        {
            _tempList.Add((float)i / sampleCount);
        }

        while (_tempList.Count > 0)
        {
            _shuffleIndex = Random.Range(0, _tempList.Count);
            _drawPots[index].Add(_tempList[_shuffleIndex]);
            _tempList.RemoveAt(_shuffleIndex);
        }

        /*
        var order = "Draw pot " + index + ": ";
        for (var i = 0; i < _drawPots[index].Count; i++)
        {
            order += _drawPots[index][i] + " ";
        }
        Debug.Log(order);
        */
    }
}
