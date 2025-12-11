using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범용 오브젝트 풀링 유틸리티
/// 
/// [기능]
/// - GameObject의 생성/파괴를 최소화하여 메모리 및 성능 최적화
/// - 프리팹 기반 풀링 또는 커스텀 키 기반 풀링 지원
/// 
/// [사용법]
/// 1. Warmup: 미리 오브젝트 생성 (선택)
///    ObjectPooling.Warmup(prefab, count, parent);
///    ObjectPooling.Warmup("MyKey", factory, count, parent);
/// 
/// 2. Get: 풀에서 오브젝트 가져오기
///    var obj = ObjectPooling.Get(prefab, parent);
///    var obj = ObjectPooling.Get("MyKey", factory, parent);
/// 
/// 3. Return: 사용 후 풀에 반환
///    ObjectPooling.Return(obj);
///    Return 함수는 각 풀 오브젝트의 스크립트에서 호출함
/// 
/// [특징]
/// - PooledObject 컴포넌트가 자동으로 추가되어 풀 추적
/// - 풀이 비어있으면 자동으로 새 오브젝트 생성
/// - static 클래스로 어디서든 접근 가능
/// </summary>
public static class ObjectPooling
{
    private enum OwnerType
    {
        Prefab,
        CustomKey
    }

    private class Pool
    {
        public readonly Queue<GameObject> queue = new Queue<GameObject>();
        public readonly Transform parent;
        public readonly Func<GameObject> factory;

        public Pool(Transform parent, Func<GameObject> factory)
        {
            this.parent = parent;
            this.factory = factory;
        }
    }

    private class PooledObject : MonoBehaviour
    {
        public int prefabId;
        public string customKey;
        public OwnerType ownerType;
    }

    private static readonly Dictionary<int, Pool> prefabPools = new Dictionary<int, Pool>();
    private static readonly Dictionary<string, Pool> keyPools = new Dictionary<string, Pool>();

    public static void Warmup(GameObject prefab, int count, Transform parent = null)
    {
        if (prefab == null || count <= 0) return;
        for (int i = 0; i < count; i++)
        {
            var go = Get(prefab, parent);
            Return(go);
        }
    }

    public static void Warmup(string key, Func<GameObject> factory, int count, Transform parent = null)
    {
        if (string.IsNullOrEmpty(key) || factory == null || count <= 0) return;
        for (int i = 0; i < count; i++)
        {
            var go = Get(key, factory, parent);
            Return(go);
        }
    }

    public static GameObject Get(GameObject prefab, Transform parent = null)
    {
        if (prefab == null) return null;

        var id = prefab.GetInstanceID();
        if (prefabPools.TryGetValue(id, out var pool) == false)
        {
            pool = new Pool(parent, () =>
            {
                var inst = UnityEngine.Object.Instantiate(prefab, parent);
                var marker = inst.GetComponent<PooledObject>();
                if (marker == null) marker = inst.AddComponent<PooledObject>();
                marker.ownerType = OwnerType.Prefab;
                marker.prefabId = id;
                marker.customKey = null;
                inst.SetActive(false);
                return inst;
            });
            prefabPools.Add(id, pool);
        }

        var go = pool.queue.Count > 0 ? pool.queue.Dequeue() : pool.factory();
        if (go.transform.parent != parent && parent != null) go.transform.SetParent(parent, false);
        go.SetActive(true);
        return go;
    }

    public static GameObject Get(string key, Func<GameObject> factory, Transform parent = null)
    {
        if (string.IsNullOrEmpty(key)) return null;

        if (keyPools.TryGetValue(key, out var pool) == false)
        {
            pool = new Pool(parent, () =>
            {
                var inst = factory();
                if (inst.transform.parent != parent && parent != null) inst.transform.SetParent(parent, false);
                var marker = inst.GetComponent<PooledObject>();
                if (marker == null) marker = inst.AddComponent<PooledObject>();
                marker.ownerType = OwnerType.CustomKey;
                marker.customKey = key;
                marker.prefabId = 0;
                inst.SetActive(false);
                return inst;
            });
            keyPools.Add(key, pool);
        }

        var go = pool.queue.Count > 0 ? pool.queue.Dequeue() : pool.factory();
        if (go.transform.parent != parent && parent != null) go.transform.SetParent(parent, false);
        go.SetActive(true);
        return go;
    }

    public static void Return(GameObject instance)
    {
        if (instance == null) return;

        var marker = instance.GetComponent<PooledObject>();
        if (marker == null)
        {
            UnityEngine.Object.Destroy(instance);
            return;
        }

        instance.SetActive(false);

        if (marker.ownerType == OwnerType.Prefab)
        {
            if (prefabPools.TryGetValue(marker.prefabId, out var pool))
            {
                pool.queue.Enqueue(instance);
                return;
            }
        }
        else
        {
            if (string.IsNullOrEmpty(marker.customKey) == false && keyPools.TryGetValue(marker.customKey, out var pool))
            {
                pool.queue.Enqueue(instance);
                return;
            }
        }

        // If pool not found (shouldn't happen), destroy to avoid leaks
        UnityEngine.Object.Destroy(instance);
    }
}
