using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ApplyRulesSystem : ISystem, ISystemStartStop
{
    private EntityQuery _jobQuery;
    private NativeList<SharedRulesGrouping> _uniqueCellTypes;
    private float Strength;

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
        var properties = SystemAPI.GetSingletonEntity<WorldProperties>();
        Strength = SystemAPI.GetComponent<WorldProperties>(properties).Strength;
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
        var strength = SystemAPI.GetSingleton<WorldProperties>().Strength;

        foreach (var cellType in _uniqueCellTypes)
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
                Rules = cellType.Rules,
                Strength = strength
            };
            job.ScheduleParallel(_jobQuery);

            _jobQuery.ResetFilter();
        }
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
