using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnCellsSystem : ISystem
{
    private EntityQuery _jobQuery;
    private EntityCommandBuffer _ecb;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        using var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<CellProperties>()
                .WithAll<SharedRulesGrouping>()
                .WithAll<LocalTransform>();
        _jobQuery = state.GetEntityQuery(queryBuilder);
        _ecb = new EntityCommandBuffer(Allocator.Persistent);

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        _ecb.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();
        var cellsBuffer = SystemAPI.GetBuffer<CellConfigurationProperties>(worldEntity);
        if (!cellsBuffer.IsEmpty)
        {
            UpdateCellCount(cellsBuffer, worldEntity, state);
        }
    }

    private void UpdateCellCount(DynamicBuffer<CellConfigurationProperties> cellsBuffer, Entity worldEntity, SystemState state)
    {
        var aspect = SystemAPI.GetAspectRW<WorldPropertiesAspect>(worldEntity);

        foreach (var rule in cellsBuffer)
        {
            var desiredCount = math.clamp(rule.NumberOfCells, 0, 10000);
            var rulesComponent = new SharedRulesGrouping { Group = rule.Id };
            _jobQuery.ResetFilter();
            _jobQuery.SetSharedComponentFilter(rulesComponent);
            int currentCount = _jobQuery.CalculateEntityCount();
            int difference = desiredCount - currentCount;

            if (difference > 0)
            {   // Add more
                for (int i = 0; i < difference; i++)
                {
                    var cell = _ecb.Instantiate(rule.Prefab);
                    _ecb.SetComponent(cell, new LocalTransform
                    {
                        Position = aspect.GetRandomPosition(),
                        Rotation = quaternion.identity,
                        Scale = aspect.Scale
                    });

                    _ecb.AddComponent(cell, new CellProperties
                    {
                        Id = rule.Id,
                        Velocity = new float3(0, 0, 0),
                    });

                    _ecb.AddSharedComponent(cell, rulesComponent);

                    _ecb.AddBuffer<VelocityChange>(cell);
                }
            }
            else
            {   // Remove entities
                var count = math.abs(difference);
                using var allEntities = _jobQuery.ToEntityArray(Allocator.Temp);
                var entities = new NativeArray<Entity>(count, Allocator.Temp);

                for (int i = 0; i < count; i++)
                {
                    entities[i] = allEntities[i];
                }

                _ecb.DestroyEntity(entities);
                entities.Dispose();
            }
        }

        cellsBuffer.Clear();
        _ecb.SetBuffer<CellConfigurationProperties>(worldEntity);
        _ecb.Playback(state.EntityManager);
    }
}
