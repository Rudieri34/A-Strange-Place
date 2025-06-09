using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPool<T> where T : Component
{
    private readonly Queue<T> _pool = new();
    private readonly T _prefab;
    private readonly Transform _parent;

    public SimpleObjectPool(T prefab, Transform parent, int initialCount = 10)
    {
        _prefab = prefab;
        _parent = parent;
        for (int i = 0; i < initialCount; i++)
        {
            var obj = GameObject.Instantiate(prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (_pool.Count > 0)
        {
            T obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }

        return GameObject.Instantiate(_prefab, _parent);
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }

    public void ReturnAll(IEnumerable<T> activeItems)
    {
        foreach (var item in activeItems)
        {
            Return(item);
        }
    }
}
