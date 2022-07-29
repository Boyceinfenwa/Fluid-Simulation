using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpacialHash<T>
{
    float X;
    float Y;
    float Z;

    float scrHeight;
    float scrWidth;
    float scrDepth;

    int cellScale;

    private Dictionary<int, List<T>> dict;
    private Dictionary<T, int> objects;

    public SpacialHash( int CellScale)
    {
        this.cellScale = CellScale;
        dict = new Dictionary<int, List<T>>();
        objects = new Dictionary<T, int>();
    }

    void NewClient(Vector3 pos, float dX,float dY, float dZ)
    {
        
    }

    public void Insert(Vector3 vec, T obj)
    {
        var key = Key(vec);
        if (dict.ContainsKey(key))
        {
            dict[key].Add(obj);
        }
        else
        {
            dict[key] = new List<T> { obj };
        }
        objects[obj] = key;
    }

    public List<T> CheckPos(Vector3 vector)
    {
        var key = Key(vector);
        return dict.ContainsKey(key) ? dict[key] : new List<T>();
    }
    void GetNearby()
    {

    }

    public void UpdtadtePos(Vector3 vec, T obj)
    {
        if (objects.ContainsKey(obj))
        {
            dict[objects[obj]].Remove(obj);
        }
        Insert(vec, obj);
    }

    public void Clear()
    {
        var keys = dict.Keys.ToArray();
        for (var i = 0; i < keys.Length; i++)
            dict[keys[i]].Clear();
        objects.Clear();
    }

    void GetCellIdx()
    {

    }


    private const int BIG_ENOUGH_INT = 16 * 1024;
    private const double BIG_ENOUGH_FLOOR = BIG_ENOUGH_INT + 0.0000;
    private static int FastFloor(float f)
    {
        return (int)(f + BIG_ENOUGH_FLOOR) - BIG_ENOUGH_INT;
    }

    private int Key(Vector3 v)
    {
        return ((FastFloor(v.x / cellScale) * 73856093) ^
                (FastFloor(v.y / cellScale) * 19349663) ^
                (FastFloor(v.z / cellScale) * 83492791));
    }
}
