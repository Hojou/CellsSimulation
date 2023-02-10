using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct SharedRulesGrouping : ISharedComponentData
{
    public int Group;
}

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnCellsSystem : ISystem
{
    private EntityQuery _jobQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldProperties>();
        using var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<CellProperties>()
                .WithAll<SharedRulesGrouping>()
                .WithAll<LocalTransform>()
                ;
        _jobQuery = state.GetEntityQuery(queryBuilder);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();

        var cellsBuffer = SystemAPI.GetBuffer<CellConfigurationProperties>(worldEntity);
        if (!cellsBuffer.IsEmpty)
        {
            UnityEngine.Debug.Log($"CellsBuffer: {cellsBuffer.Length}");
            UpdateCellCount(cellsBuffer, worldEntity, state);
        }

        var rulesBuffer = SystemAPI.GetBuffer<CellRule>(worldEntity);
        if (!rulesBuffer.IsEmpty)
        {
            UnityEngine.Debug.Log($"RulesBuffer: {rulesBuffer.Length}");
            UpdateRules(rulesBuffer, worldEntity, state);
        }
    }

    private void UpdateCellCount(DynamicBuffer<CellConfigurationProperties> cellsBuffer, Entity worldEntity, SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var aspect = SystemAPI.GetAspectRW<WorldPropertiesAspect>(worldEntity);

        foreach (var rule in cellsBuffer)
        {
            var desiredCount = rule.NumberOfCells;
            var rulesComponent = new SharedRulesGrouping { Group = rule.Id };
            _jobQuery.ResetFilter();
            _jobQuery.SetSharedComponentFilter(rulesComponent);
            int currentCount = _jobQuery.CalculateEntityCount();
            int difference = desiredCount - currentCount;

            if (difference > 0)
            {   // Add more
                UnityEngine.Debug.Log($"Adding more {difference}");
                for (int i = 0; i < difference; i++)
                {
                    var cell = ecb.Instantiate(rule.Prefab);
                    ecb.SetComponent(cell, new LocalTransform
                    {
                        Position = aspect.GetRandomPosition(),
                        Rotation = quaternion.identity,
                        Scale = aspect.Scale
                    });

                    ecb.AddComponent(cell, new CellProperties
                    {
                        Id = rule.Id,
                        Velocity = new float3(0, 0, 0),
                    });

                    ecb.AddSharedComponent(cell, rulesComponent);

                    ecb.AddBuffer<VelocityChange>(cell);
                }
            }
            else
            {   // Remove entities
                UnityEngine.Debug.Log($"Removing: {difference}");
                for (int i = 0; i > difference; i--)
                {
                    var entity = _jobQuery.GetSingletonEntity();
                    ecb.DestroyEntity(entity);
                }
            }
        }

        cellsBuffer.Clear();
        ecb.SetBuffer<CellConfigurationProperties>(worldEntity);
        ecb.Playback(state.EntityManager);
    }

    private void UpdateRules(DynamicBuffer<CellRule> rulesBuffer, Entity worldEntity, SystemState state)
    {
        //UnityEngine.Debug.Log("Updating rules! Count:" + rulesBuffer.Length.ToString());
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var rules = SystemAPI.GetSingletonRW<WorldProperties>().ValueRW.Rules;
        
        foreach (var rule in rulesBuffer)
        {
            int keyIndex = 32 * rule.Id1 + rule.Id2;
            rules[keyIndex] = rule.Amount;
        }

        rulesBuffer.Clear();
        ecb.SetBuffer<CellRule>(worldEntity);
        ecb.Playback(state.EntityManager);
    }
}
