using Sirenix.Serialization;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.PackageManager;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ApplyRulesSystem : ISystem, ISystemStartStop
{
    private EntityQuery _jobQuery;
    private NativeList<SharedRulesGrouping> _uniqueCellTypes;
    private NativeHashMap<int, NativeHashMap<int, float>> _rules;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldProperties>();
        state.RequireForUpdate<SharedRulesGrouping>();
        using var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
                        .WithAll<CellProperties>()
                        .WithAll<SharedRulesGrouping>()
                        .WithAll<LocalTransform>()
                        ;
        _jobQuery = state.GetEntityQuery(queryBuilder);
        _rules = new NativeHashMap<int, NativeHashMap<int, float>>(32, Allocator.Persistent);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        state.EntityManager.GetAllUniqueSharedComponents(out NativeList<SharedRulesGrouping> uniqueCellRuleTypes, Allocator.Persistent);
        this._uniqueCellTypes = uniqueCellRuleTypes;
        //var properties = SystemAPI.GetSingletonEntity<WorldProperties>();
        //Strength = SystemAPI.GetComponent<WorldProperties>(properties).Strength;
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cellProperties = _jobQuery.ToComponentDataArray<CellProperties>(Allocator.TempJob); 
        var cellLocations = _jobQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();
        var props = SystemAPI.GetComponent<WorldProperties>(worldEntity); // .GetSingleton<WorldProperties>();

        var rulesBuffer = SystemAPI.GetBuffer<CellRule>(worldEntity);
        if (!rulesBuffer.IsEmpty)
        {
            UpdateRules(rulesBuffer, worldEntity);
        }

        foreach (var cellType in _uniqueCellTypes)
        {
            _jobQuery.AddSharedComponentFilter(cellType);

            var cellCount = _jobQuery.CalculateEntityCount();           
            if (cellCount == 0 || !_rules.TryGetValue(cellType.Group, out var rules))
            {
                _jobQuery.ResetFilter();
                continue;
            }

            var job = new ApplyRuleJob
            {
                CellPositions = cellLocations,
                CellProperties = cellProperties,
                Rules = rules,
                Strength = props.Strength
            };
            job.ScheduleParallel(_jobQuery);

            _jobQuery.ResetFilter();
        }
    }

    private void UpdateRules(DynamicBuffer<CellRule> rulesBuffer, Entity worldEntity)
    {
        //UnityEngine.Debug.Log("Updating rules! Count:" + rulesBuffer.Length.ToString());
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var rule in rulesBuffer)
        {
            var Id1 = rule.Id1;
            var Id2 = rule.Id2;
            var Amount = rule.Amount;

            if (!_rules.TryGetValue(Id1, out var rules))
            {
                rules = new NativeHashMap<int, float>(32, Allocator.Persistent);
                _rules.TryAdd(Id1, rules);
            }

            if (Amount == 0)
            {
                _rules.Remove(Id2);
            } else if (rules.ContainsKey(Id2))
            {
                rules[Id2] = Amount;
            }
            else
            {
                rules.Add(Id2, Amount);
            }
        }

        rulesBuffer.Clear();
        ecb.SetBuffer<CellRule>(worldEntity);


        //foreach (var property in rulesBuffer)
        //{
        //    //SharedRulesGrouping rulesComponent = new SharedRulesGrouping { Group = property.Id };  // CreateRulesComponent(ref aspect, property);

        //    for (int i = 0; i < property.NumberOfCells; i++)
        //    {
        //        var cell = ecb.Instantiate(property.Prefab);
        //        ecb.SetComponent(cell, new LocalTransform
        //        {
        //            Position = aspect.GetRandomPosition(),
        //            Rotation = quaternion.identity,
        //            Scale = aspect.Scale
        //        });

        //        ecb.AddComponent(cell, new CellProperties
        //        {
        //            Id = property.Id,
        //            Velocity = new float3(0, 0, 0),
        //        });

        //        ecb.AddSharedComponent(cell, rulesComponent);

        //        ecb.AddBuffer<VelocityChange>(cell);
        //    }
        //}
    }

    private void BuildRules()
    {
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
    }
}

[BurstCompile]
public partial struct ApplyRuleJob: IJobEntity
{
    public float Strength;
    [ReadOnly] public NativeArray<LocalTransform> CellPositions;
    [ReadOnly] public NativeArray<CellProperties> CellProperties;
    [ReadOnly] public NativeHashMap<int, float> Rules;
    public void Execute(in TransformAspect aspect, ref CellProperties properties)
    {
        var pos = aspect.LocalPosition;
        var length = CellPositions.Length;
        var velocityChange = float3.zero;
        var oldId = 0;
        var amount = 0f;
        for (int i = 0; i < length; i++)
        {
            var otherId = CellProperties[i].Id;
            if (otherId != oldId)
            {
                if (!Rules.TryGetValue(otherId, out amount)) continue;
                oldId = otherId;
            }
            var otherPos = CellPositions[i].Position;
            var dx = pos.x - otherPos.x;
            var dz = pos.z - otherPos.z;
            var dist = math.sqrt(dx * dx + dz * dz);
            if (dist > 0 && dist < 1.6f)
            {
                var force = (Strength * amount) / dist;
                velocityChange += force * new float3(dx, 0, dz);
            }
        }
        properties.Velocity = (properties.Velocity + velocityChange) * .5f;
    }
}
