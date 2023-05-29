using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial struct ApplyRulesSystem : ISystem
{
    private EntityQuery _jobQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        using var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
                        .WithAll<CellProperties>()
                        .WithAll<SharedRulesGrouping>()
                        .WithAll<LocalTransform>();
        _jobQuery = state.GetEntityQuery(queryBuilder);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();
        if (!state.EntityManager.HasComponent<CellRules>(worldEntity))
        {
            return;
        }
        using var cellProperties = _jobQuery.ToComponentDataArray<CellProperties>(Allocator.TempJob); 
        using var cellLocations = _jobQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var props = SystemAPI.GetComponent<WorldProperties>(worldEntity);
        var rules = SystemAPI.GetComponent<CellRules>(worldEntity).Value;

        state.EntityManager.GetAllUniqueSharedComponents(out NativeList<SharedRulesGrouping> uniqueCellRuleTypes, Allocator.TempJob);
        foreach (var cellType in uniqueCellRuleTypes)
        {
            _jobQuery.AddSharedComponentFilter(cellType);
            var cellCount = _jobQuery.CalculateEntityCount();           
            if (cellCount == 0)
            {
                _jobQuery.ResetFilter();
                continue;
            }

            var job = new ApplyRuleJob
            {
                CellPositions = cellLocations,
                CellProperties = cellProperties,
                CellRules = rules,
                Distance = props.Influence
            };
            job.ScheduleParallel(_jobQuery);

            _jobQuery.ResetFilter();
        }
        uniqueCellRuleTypes.Dispose();
        state.Dependency.Complete();
    }
}

[BurstCompile]
public partial struct ApplyRuleJob: IJobEntity
{
    public float Distance;
    [ReadOnly] public NativeArray<LocalTransform> CellPositions;
    [ReadOnly] public NativeArray<CellProperties> CellProperties;
    [ReadOnly] public NativeArray<float> CellRules;
    public void Execute(in TransformAspect aspect, ref CellProperties properties)
    {
        var pos = aspect.LocalPosition;
        var velocityChange = float3.zero;
        var index = properties.Id * 32;
        var length = CellPositions.Length;
        for (int i = 0; i < length; i++)
        {
            var otherId = CellProperties[i].Id;
            var amount = CellRules[index + otherId];
            if (amount == 0) { continue; }
            var otherPos = CellPositions[i].Position;
            var dx = pos.x - otherPos.x;
            var dz = pos.z - otherPos.z;
            var dist = math.sqrt(dx * dx + dz * dz);
            if (dist > 0 && dist < Distance)
            {
                var force = amount / dist;
                velocityChange += force * new float3(dx, 0, dz);
            }
        }
        properties.Velocity = (properties.Velocity + velocityChange) * .5f;
    }
}
