using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
//这里放了一些工具类
public static class Tools
{
    ///<param name="parentType">给定的类型</param>
    ///<returns>所有子类</returns>
    public static List<Type> GetSubClass(Type parentType)
    {
        var subTypeList = new List<Type>();
        var assembly = parentType.Assembly;
        var assemblyAllType = assembly.GetTypes();
        foreach (var item in assemblyAllType)
        {
            if (parentType.IsAssignableFrom(item))
            {
                subTypeList.Add(item);
            }
        }
        return subTypeList;
    }
    public static bool Random(float number)
    {
        return number> UnityEngine.Random.value;
    }
    public static int Value(float number)
    {
        if(number> UnityEngine.Random.value)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    public static void Wash<T>(this List<T> ts)
    {
        for (int i = ts.Count - 1; i >= 0; i--)
        {
            var j=UnityEngine.Random.Range(0, i);
            (ts[i], ts[j]) = (ts[j], ts[i]);
        }
    }
    public static void Wash<T>(this T[] ts)
    {
        for (int i = ts.Length - 1; i >= 0; i--)
        {
            var j = UnityEngine.Random.Range(0, i);
            (ts[i], ts[j]) = (ts[j], ts[i]);
        }
    }
    public static T Random<T>(this IEnumerable<T> values)
    {
        return values.ElementAt(UnityEngine.Random.Range(0,values.Count()));
    }
    public static T Random<T>(this List<T> values)
    {
        return values[UnityEngine.Random.Range(0, values.Count)];
    }
    public static T Random<T>(this T[] values)
    {
        return values[UnityEngine.Random.Range(0, values.Length)];
    }
    public static float[] RandomDistance(int num,float distance)
    {
        var array = new float[num];
        float d = distance / num;
        array[0]=0;
        for (int i = 1; i < array.Length-1; i++)
        {
            array[i]=Math.Clamp(array[i-1] +d*UnityEngine.Random.Range(0,2f),d*(i-1),d*(i+1));
        }
        array[^1]=distance;
        return array;
    }
    public static float[] RandomDistance(int num, float distance,int floor)
    {
        var array = new float[num];
        float d = distance / num;
        array[0] = 0;
        for (int i = 1; i < array.Length - 1; i++)
        {
            array[i] = Math.Clamp(array[i - 1] + d * UnityEngine.Random.Range(0, 2f), d * (i - 1), d * (i + 1));
        }
        for (int i = 2;i < array.Length - 1;i++)
        {
            int f = (int)(i / (float)floor + 0.5f)*floor;
            array[i] = (array[i]*4 + array[f])/5;
        }
        array[^1] = distance;
        return array;
    }
    public static void ChangeActive(this GameObject gameObject) { gameObject.SetActive(!gameObject.activeSelf); }
}
[Serializable]
public class Rolls<T>:IEnumerable<T>,IProgress<Rolls<T>.Roll>,IReadOnlyList<T>,IEnumerator<T>
{
    [Serializable]
    public class Roll
    {
        public byte weight;
        public T roll;
    }
    public Rolls(IEnumerable<Roll> rolls)
    {
        this.rolls = new(rolls);
        rollsWill = new(rolls);
        Update();
    }
    public Rolls() { }

    public List<Roll> rolls;
    List<Roll> rollsWill=new();
    ushort weight;
    public void Update()
    {
        weight = 0;
        foreach (Roll item in rollsWill)
        {
            weight += item.weight;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Report(Roll roll)
    {
        rolls.Add(roll);
        rollsWill.Add(roll);
    }

    public bool MoveNext()
    {
        return rollsWill.Count > 0;
    }

    public void Reset()
    {
        if (rollsWill == null) 
            rollsWill = new();
        else
            rollsWill.Clear();
        rollsWill.AddRange(rolls);
    }

    public void Dispose()
    {
        rolls.Clear();
        rollsWill.Clear();
        weight = 0;
    }

    public T Current
    {
        get 
        {
            if(rollsWill.Count==0)
                Reset();
            var i = UnityEngine.Random.Range(0, weight);
            foreach (var roll in rollsWill)
            {
                if (i<roll.weight)
                {
                    rollsWill.Remove(roll);
                    Update();
                    return roll.roll;
                }
                else
                {
                    i-=roll.weight;
                }
            }
            return default;
        }
    }

    public int ID
    {
        get
        {
            if (rollsWill.Count == 0)
                Reset();
            var i = UnityEngine.Random.Range(0, weight);
            for (int j = 0; j < rollsWill.Count; j++)
            {
                Roll roll = rollsWill[j];
                if (i < roll.weight)
                {
                    rollsWill.Remove(roll);
                    Update();
                    return j;
                }
                else
                {
                    i -= roll.weight;
                }
            }
            return default;
        }
    }
    public T CurrentA
    {
        get
        {
            var i = UnityEngine.Random.Range(0, weight);
            foreach (var roll in rolls)
            {
                if (i < roll.weight)
                {
                    return roll.roll;
                }
                else
                {
                    i -= roll.weight;
                }
            }
            return default;
        }
    }

    public int IDA
    {
        get
        {
            var i = UnityEngine.Random.Range(0, weight);
            for (int j = 0; j < rollsWill.Count; j++)
            {
                Roll roll = rolls[j];
                if (i < roll.weight)
                {
                    return j;
                }
                else
                {
                    i -= roll.weight;
                }
            }
            return default;
        }
    }

    public int Count => rollsWill.Count;

    object IEnumerator.Current => Current;

    public T this[int index] => rollsWill[index].roll;
}
public class ActionOnEnd : IDisposable
{
    public Action onEnd;
    public ActionOnEnd(Action onEnd)
    { this.onEnd = onEnd; }
    public void Dispose()
    {
        onEnd.Invoke();
    }
}
[Serializable]
public class Array2D<T> : IEnumerable<IEnumerable<T>>
{
    public int wide;
    public T[] values;

    public IEnumerator<IEnumerable<T>> GetEnumerator()
    {
        for (int i = 0; i < values.Length / wide; i++)
        {
            yield return enumerable();
            IEnumerable<T> enumerable()
            {
                if (i * wide > values.Length)
                {
                    for (int j = i * wide; j < values.Length; j++)
                    {
                        yield return values[j];
                    }
                }
                else
                {
                    for (int j = i * wide; j < i * wide + wide; j++)
                    {
                        yield return values[j];
                    }
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; i < values.Length / wide; i++)
        {
            yield return enumerable();
            IEnumerable<T> enumerable()
            {
                if (i * wide > values.Length)
                {
                    for (int j = i * wide; j < values.Length; j++)
                    {
                        yield return values[j];
                    }
                }
                else
                {
                    for (int j = i * wide; j < i * wide + wide; j++)
                    {
                        yield return values[j];
                    }
                }
            }
        }
    }
}
public class DebugText:IDisposable
{
    public StreamWriter writer;
    public string debugPath;
    public DebugText(string path)
    {
        Begin(path);
    }
    public DebugText() { }
    public bool hasBug=false;

    public void Begin(string path)
    {
        string p = Path.Combine(Application.persistentDataPath, "Debug", path + ".txt");
        writer = File.CreateText(p);
        debugPath = p;
        hasBug = false;
    }
    public void Write(string text)
    {
        if (writer != null)
        {
            writer.WriteLine(text);
            writer.Flush();
            hasBug = true;
        }
    }
    public void End()
    {
        if (writer != null)
        {
            writer.Dispose();
            writer = null;
            if (string.IsNullOrEmpty(File.ReadAllText(debugPath)))
            {
                File.Delete(debugPath);
            }
            debugPath = null;
        }
    }

    public void Dispose()
    {
        End();
    }
}