using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ConfigurationMono : MonoBehaviour
{
}

public struct BakedCellPrefab : IBufferElementData
{
    public FixedString32Bytes Name;
    public Entity Prefab;
}

public class ConfigurationBaker : Baker<ConfigurationMono>
{
    public override void Bake(ConfigurationMono authoring)
    {
        AddComponent(new WorldProperties
        {
            Dimension = new float2(5, 5),
            Speed = 1f,
            Strength = 1f,
            Influence = 1.6f,
            Scale = .2f,
            Rules = new NativeArray<float>(32 * 32, Allocator.Persistent)
        });

        AddComponent(new CellRandom { Value = Unity.Mathematics.Random.CreateFromIndex(1337) });

        CellConfigurationSO[] scriptableObjects = Resources.LoadAll<CellConfigurationSO>("Cells");
        if (scriptableObjects.Length > 32)
        {
            throw new System.Exception("Too many Cell resources. Maximum number is 32. Remove some from the Cells folder");
        }
        
        // GameObjects types needs to be baked
        var prefabs = scriptableObjects
            .Select(cell => new BakedCellPrefab { Name = cell.name, Prefab = GetEntity(cell.cellPrefab) })
            .ToArray();
        Debug.Log($"{string.Join(',', prefabs.Select(p => p.Name))}");
        var propsBuffer = AddBuffer<BakedCellPrefab>();
        propsBuffer.CopyFrom(prefabs);

        AddBuffer<CellConfigurationProperties>();
    }
}
