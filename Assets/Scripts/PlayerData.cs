using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public string Name;
    public int Age;
    public string Sex;

    public string Stringify()
    {
        return JsonUtility.ToJson(this);
    }
    public static PlayerData Parse(string json)
    {
        return JsonUtility.FromJson<PlayerData>(json);
    }
}
