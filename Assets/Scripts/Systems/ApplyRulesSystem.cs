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
    private EntityQuery _matchQuery;
    private NativeList<SharedRulesGrouping> _uniqueCellTypes;

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
        _matchQuery = state.GetEntityQuery(queryBuilder);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnStartRunning(ref SystemState state)
    {
        state.EntityManager.GetAllUniqueSharedComponents(out NativeList<SharedRulesGrouping> uniqueCellRuleTypes, Allocator.Persistent);
        this._uniqueCellTypes = uniqueCellRuleTypes;
    }

    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var handle = state.Dependency;

        float deltaTime = math.min(0.05f, SystemAPI.Time.DeltaTime);
        var cellProperties = _jobQuery.ToComponentDataArray<CellProperties>(Allocator.TempJob); 
        var cellLocations = _jobQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        foreach (var cellType in _uniqueCellTypes)
        {
            //UnityEngine.Debug.Log("uniqueCellType " + cellType.Group);
            _jobQuery.AddSharedComponentFilter(cellType);

            var cellCount = _jobQuery.CalculateEntityCount();
            if (cellCount == 0)
            {
                _jobQuery.ResetFilter();
                continue;
            }

            var rulesMap = new NativeHashMap<int, float>(cellType.Rules.Length, Allocator.TempJob);
            foreach (var rule in cellType.Rules)
            {
                rulesMap.Add(rule.Id, rule.Amount);
            }

            var job = new ApplyRuleJob
            {
                DeltaTime = deltaTime,
                CellPositions = cellLocations,
                CellProperties = cellProperties,
                Rules = rulesMap,
            };
            handle = job.ScheduleParallel(_jobQuery, handle);

            _jobQuery.ResetFilter();
        }
        state.Dependency = handle;
    }
}

[BurstCompile]
public partial struct ApplyRuleJob: IJobEntity
{
    public float DeltaTime;
    [ReadOnly] public NativeArray<LocalTransform> CellPositions;
    [ReadOnly] public NativeArray<CellProperties> CellProperties;
    [ReadOnly] public NativeHashMap<int, float> Rules;
    public void Execute(in TransformAspect aspect, ref CellProperties properties)
    {
        var pos = aspect.LocalPosition;
        var length = CellPositions.Length;
        var velocityChange = float3.zero;
        for (int i = 0; i < length; i++)
        {
            var otherId = CellProperties[i].Id;
            if (!Rules.TryGetValue(otherId, out float amount)) continue;
            var otherPos = CellPositions[i].Position;
            var dx = pos.x - otherPos.x;
            var dz = pos.z - otherPos.z;
            var dist = math.sqrt(dx * dx + dz * dz);
            if (dist > 0 && dist < 1.6f)
            {
                var force = amount / dist;
                velocityChange += force * new float3(dx, 0, dz);
            }
        }
        properties.Velocity = (properties.Velocity + velocityChange) * .5f;
    }
}
