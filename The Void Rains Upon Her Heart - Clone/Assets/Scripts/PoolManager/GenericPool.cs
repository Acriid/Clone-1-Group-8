using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

public class GenericPool<T> where T : Component
{
    private GameObject _prefab;
    private Transform _prefabParent;
    private Stack<T> _pool;
    private HashSet<T> _activeInstances;
    private int _totalCreated = 0;

    public GenericPool(GameObject prefab, Transform prefabParent, int initialSize = 0)
    {
        _prefab = prefab;
        _prefabParent = prefabParent;
        _pool = new(initialSize);
        _activeInstances = new();

        for(int i = 0 ; i < initialSize ; i++)
        {
            CreateInstance();
        }
    }

    private T CreateInstance()
    {
        GameObject obj = Object.Instantiate(_prefab,_prefabParent);
        obj.name = $"{_prefab.name}_{_totalCreated++}";
        
        if(!obj.TryGetComponent<T>(out var component))
        {
            Debug.LogError($"Prefab {_prefab.name} does not have componenet {typeof(T).Name}");
            Object.Destroy(obj);
            return null;
        }

        obj.SetActive(false);
        _pool.Push(component);
        return component;
    }

    public T Get()
    {
        T instance;

        if(_pool.Count > 0)
        {
            instance = _pool.Pop();
        }
        else
        {
            instance = CreateInstance();
        }

        if(instance != null)
        {
            instance.gameObject.SetActive(true);
            _activeInstances.Add(instance);
        }

        return instance;
    }

    public void Return(T instance)
    {
        if(instance == null) return;

        instance.gameObject.SetActive(false);
        _activeInstances.Remove(instance);
        _pool.Push(instance);
    }

    public void ReturnAll()
    {
        var instancesCopy = new List<T>(_activeInstances);
        foreach(T instance in instancesCopy)
        {
            Return(instance);
        }

    }

    public int ActiveCount => _activeInstances.Count;
    public int PooledCount => _pool.Count;
    public int TotalCount => _totalCreated;
}
