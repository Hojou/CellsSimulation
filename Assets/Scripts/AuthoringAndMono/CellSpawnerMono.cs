using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using System.Linq;
using Unity.Burst.Intrinsics;

[Serializable]
public class CellConfigurationMono
{
    public int Id;
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

public class CellSpawnerMono : MonoBehaviour
{
    public float2 Dimension;
    public float Speed;
    public uint RandomSeed;
    public float Scale;
    [SerializeReference]
    public List<CellConfigurationMono> CellConfigurations;
    public CellRuleMono[] Rules;
}

public class CellSpawnerBaker : Baker<CellSpawnerMono>
{
    public override void Bake(CellSpawnerMono authoring)
    {
        var cells = authoring.CellConfigurations.Select(a => new CellConfigurationProperties
        {
            Id = a.Id,
            NumberOfCells = a.NumberOfCells,
            Prefab = GetEntity(a.Prefab)
        }).ToArray();
        var propsBuffer = AddBuffer<CellConfigurationProperties>();
        propsBuffer.CopyFrom(cells);

        var rules = authoring.Rules.Select(rule => new CellRule
        {
            Id1 = rule.Id1,
            Id2 = rule.Id2,
            Amount = rule.Amount,
        }).ToArray();
        var rulesBuffer = AddBuffer<CellRule>();
        rulesBuffer.CopyFrom(rules);

        AddComponent(new WorldProperties
        {
            Dimension = authoring.Dimension,
            Speed = authoring.Speed,
            Scale = authoring.Scale,
        });

        AddComponent(new CellRandom { Value = Random.CreateFromIndex(authoring.RandomSeed) });
    }
}
