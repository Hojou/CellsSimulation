using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.Collections;

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

public class CellSpawnerMono : MonoBehaviour
{
    public float2 Dimension;
    public float Speed;
    public float Strength;
    public uint RandomSeed;
    public float Scale;
    [SerializeReference]
    public List<CellConfigurationMono> CellConfigurations;
    public CellRuleMono[] Rules;
}

public class CellSpawnerBaker : Baker<CellSpawnerMono>, ICellSpawnerBaker
{
    public override void Bake(CellSpawnerMono authoring)
    {
        var cells = authoring.CellConfigurations.Select(a => new CellConfigurationProperties
        {
            Id = a.Id,
            NumberOfCells = a.NumberOfCells,
            Name = a.Name,
            Prefab = GetEntity(a.Prefab)
        }).ToArray();
        var propsBuffer = AddBuffer<CellConfigurationProperties>();
        propsBuffer.CopyFrom(cells);

        var tempRules = authoring.Rules.Select(rule => new CellRule
        {
            Id1 = rule.Id1,
            Id2 = rule.Id2,
            Amount = rule.Amount,
        }).ToArray();
        var rulesBuffer = AddBuffer<CellRule>();
        rulesBuffer.CopyFrom(tempRules);

        //var lookup = authoring.Rules.ToLookup(r => r.Id1);
        //var rules = new NativeHashMap<int, NativeHashMap<int, float>>(cells.Length, Allocator.Persistent);
        //foreach (var group in lookup)
        //{
        //    var innerMap = new NativeHashMap<int, float>(group.Count(), Allocator.Persistent);
        //    foreach (var g in group)
        //    {
        //        innerMap.TryAdd(g.Id2, g.Amount);
        //    }
        //    rules.TryAdd(group.Key, innerMap);
        //}

        AddComponent(new WorldProperties
        {
            Dimension = authoring.Dimension,
            Speed = authoring.Speed,
            Strength = authoring.Strength,
            Scale = authoring.Scale,
            Rules = new NativeArray<float>(32*32, Allocator.Persistent)
        });

        AddComponent(new CellRandom { Value = Random.CreateFromIndex(authoring.RandomSeed) });
    }
}
