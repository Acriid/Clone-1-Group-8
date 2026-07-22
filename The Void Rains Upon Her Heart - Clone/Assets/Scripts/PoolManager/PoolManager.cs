using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance {get; private set;}
    [SerializeField] private Transform _poolRoot;
    [SerializeField] private bool _dontDestroyOnLoad = true;
    private Dictionary<Type, object> _pools = new();

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if(_dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        if(_poolRoot == null)
            _poolRoot = transform;
    }

    public GenericPool<T> GetPool<T>(GameObject prefab, int initialSize = 5) where T : Component
    {
        Type type = typeof(T);

        if(!_pools.TryGetValue(type, out object poolObj))
        {
            Transform poolParent = new GameObject($"{type.Name}_Pool").transform;
            poolParent.SetParent(_poolRoot);

            var pool = new GenericPool<T>(prefab,poolParent,initialSize);
            _pools[type] = pool;
            
            return pool;
        }

        return (GenericPool<T>)poolObj;
    }

    public T Get<T>(GameObject prefab) where T : Component
    {
        return GetPool<T>(prefab).Get();
    }

    public void Return<T>(T instance) where T : Component
    {
        Type type = typeof(T);
        if(_pools.TryGetValue(type, out object poolObj))
        {
            ((GenericPool<T>)poolObj).Return(instance);
        }
    }

    public void ClearAllPools()
    {
        foreach(var pool in _pools.Values)
        {
            var method = pool.GetType().GetMethod("ReturnAll");
            method?.Invoke(pool,null);
        }
    }

    public Dictionary<string,(int active, int pooled)> GetPoolStats()
    {
        var stats = new Dictionary<string, (int,int)>();
        foreach (var kvp in _pools)
        {
            var poolType = kvp.Value.GetType();
            var activeCount = (int)poolType.GetProperty("ActiveCount").GetValue(kvp.Value);
            var pooledCount = (int)poolType.GetProperty("PooledCount").GetValue(kvp.Value);

            stats[kvp.Key.Name] = (activeCount, pooledCount);
        }

        return stats;
    }

    void OnDestroy()
    {
        if(Instance == this)
            Instance = null;
    }
}
