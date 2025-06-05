using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CustomPool<T> where T : MonoBehaviour
{
    private T prefab;
    private List<T> objects;

    public CustomPool(T prefab, int preWarmObjCount, Transform parent)
    {
        this.prefab = prefab;
        objects = new List<T>();
        for (int i = 0; i < preWarmObjCount; i++)
        {
            var obj = GameObject.Instantiate(prefab,parent);
            obj.gameObject.SetActive(false);
            objects.Add(obj);
        }

    }
    // this
    public T Get()
    {
        var obj = objects.FirstOrDefault(x => !x.isActiveAndEnabled);
        if (obj == null)
        {
            if (obj == null)
            {
                obj = Create();
            }
        }
        obj.gameObject.SetActive(true);
        return obj;
    }

    public T GetActive()
    {
        var obj = objects.FirstOrDefault(x => x.isActiveAndEnabled);
        if (obj == null)
        {
            obj = Create();
        }
        obj.gameObject.SetActive(true);
        return obj;
    }

    private T Create()
    {
        var obj = GameObject.Instantiate(prefab);
        objects.Add(obj);
        return obj;
    }

    // Update is called once per frame
    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
    }

    public T GetByIndex(int index)
    {
        return objects[index];
    }

    public int Count()
    {
        return objects.Count;
    }
}