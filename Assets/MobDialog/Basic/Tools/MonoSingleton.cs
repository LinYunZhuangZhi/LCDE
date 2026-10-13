using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T:MonoSingleton<T>
{
    private static T instance=null;
    public static T Instance
    { get {return instance; } }
    protected virtual void Awake()
    {

        if (instance == null)
            instance = (T)this;

        else
            Destroy(this);
    }
    protected void OnDestroy()
    {
        if (instance==this)
        {
            instance = null;
        }
    }
}
public class Singleton<T> where T : Singleton<T>,new()
{

    private static T instance = new();
    public static T Instance => instance;
}