using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[Serializable]
public class CellConfiguration
{
    public int Id;
    public int NumberOfCells;
    public GameObject Prefab;
}

public class CellSpawnerMono : MonoBehaviour
{
    public float2 Dimension;
    public float Speed;
    [SerializeReference]
    public List<CellConfiguration> CellConfigurations;
}

public class CellSpawnerBaker : Baker<CellSpawnerMono>
{
    public override void Bake(CellSpawnerMono authoring)
    {
        var propsBuffer = AddBuffer<CellConfigurationProperties>();
        authoring.CellConfigurations.ForEach(a =>
        {
            propsBuffer.Add(new CellConfigurationProperties
            {
                Id = a.Id,
                NumberOfCells = a.NumberOfCells,
                Prefab = GetEntity(a.Prefab)
            });
        });

        AddComponent(new WorldProperties
        {
            Dimension = authoring.Dimension,
            Speed = authoring.Speed,
        });

        AddComponent(new CellRandom { Value = Random.CreateFromIndex(1337) });
    }
}
