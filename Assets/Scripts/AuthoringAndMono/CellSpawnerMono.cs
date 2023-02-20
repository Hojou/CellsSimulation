using System;
using UnityEngine;

[Serializable]
public class CellConfigurationMono
{
    public int Id;
    public string Name;
    public int NumberOfCells;
    public GameObject Prefab;
}

[Serializable]
public class CellRuleMono
{
    public int Id1;
    public int Id2;
    public float Amount;
}

