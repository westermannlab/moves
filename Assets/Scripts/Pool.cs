using System;
using System.Collections.Generic;
using UnityEngine;

public class Pool : ScriptableObject
{
    public PoolObjectData[] Prefabs;
    
    private List<PoolObject>[] _idlePoolObjects;
    private List<PoolObject>[] _usedPoolObjects;
    private Dictionary<Type, PoolObjectData> _typeDictionary;

    public void Init()
    {
        _idlePoolObjects = new List<PoolObject>[Prefabs.Length];
        _usedPoolObjects = new List<PoolObject>[Prefabs.Length];
        _typeDictionary = new Dictionary<Type, PoolObjectData>();
        for (var i = 0; i < Prefabs.Length; i++)
        {
            _idlePoolObjects[i] = new List<PoolObject>();
            _usedPoolObjects[i] = new List<PoolObject>();
            _typeDictionary.Add(Prefabs[i].Prefab.GetType(), Prefabs[i]);
            Prefabs[i].Id = i;
            Prefabs[i].MaxAmount = 0;
            Prefabs[i].CreateInitialPrefabs();
        }
    }
    
    public void Create(PoolObjectData data, int quantity = 1)
    {

        for (var i = 0; i < quantity; i++)
        {
            var poolObject = Instantiate(data.Prefab, References.PoolTf);
            poolObject.Init();
            data.MaxAmount++;
            Return(poolObject);
        }
    }
    
    public PoolObject Get(Type type)
    {
        return Get(_typeDictionary[type]);
    }

    private PoolObject Get(PoolObjectData data)
    {
        if (_idlePoolObjects[data.Id].Count == 0)
        {
            Create(data);
        }

        var poolObject = _idlePoolObjects[data.Id][0];
        _idlePoolObjects[data.Id].Remove(poolObject);
        _usedPoolObjects[data.Id].Add(poolObject);
        poolObject.gameObject.SetActive(true);
        return poolObject;
    }

    public void Return(PoolObject poolObject)
    {

        if (poolObject == null) return;
        poolObject.Reset();
        if (_typeDictionary.ContainsKey(poolObject.GetType()))
        {
            if (_usedPoolObjects[_typeDictionary[poolObject.GetType()].Id].Contains(poolObject))
            {
                _usedPoolObjects[_typeDictionary[poolObject.GetType()].Id].Remove(poolObject);
            }
            _idlePoolObjects[_typeDictionary[poolObject.GetType()].Id].Add(poolObject);
            return;
        }

        Debug.Log("Was unable to return " + poolObject.name + " of type " + poolObject.GetType() + " to pool.");
        Destroy(poolObject.gameObject);
    }

    public void ReturnAll()
    {
        foreach (var list in _usedPoolObjects)
        {
            while (list.Count > 0)
            {
                Return(list[0]);
            }
        }
    }
}
