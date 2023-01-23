using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ApplyRulesSystem : ISystem, ISystemStartStop
{
    private EntityQuery _jobQuery;
    private NativeList<SharedRulesGrouping> _uniqueCellTypes;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldProperties>();
        state.RequireForUpdate<SharedRulesGrouping>();
        using var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
                        .WithAll<CellProperties>()
                        .WithAll<SharedRulesGrouping>()
                        .WithAll<LocalToWorld>();
        _jobQuery = state.GetEntityQuery(queryBuilder);
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
        // TODO: REMOVE disable system
        state.Enabled = false;

        //float deltaTime = math.min(0.05f, SystemAPI.Time.DeltaTime);
        //var allCells = _jobQuery.ToComponentDataArray<CellProperties>(Allocator.TempJob);

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

            //var map = new NativeHashMap<int, float>(cellType.Rules.Length, Allocator.TempJob);
            //foreach (var rule in cellType.Rules)
            //{
            //    map.Add(rule.Id, rule.Amount);
            //}

            var job = new ApplyRuleJob
            {
                //DeltaTime = deltaTime,
                //AllCells = allCells,
                //Rules = map,
            };
            job.ScheduleParallel(_jobQuery);
            //job.Run();

            _jobQuery.ResetFilter();
        }
    }
}

public partial struct ApplyRuleJob: IJobEntity
{
    //public float DeltaTime;
    //[ReadOnly] public NativeArray<CellProperties> AllCells;
    //[ReadOnly] public NativeHashMap<int, float> Rules;
    public void Execute([ReadOnly] in CellProperties aspect)
    {
        //var pos = aspect.LocalPosition;
        UnityEngine.Debug.Log("Run");
        //foreach (var cellPosition in AllCells)
        //{
        //    var pos = aspect.LocalPosition;
        //    var dx = pos.x - cellPosition.Position.x;
        //    var dz = pos.z - cellPosition.Position.z;
        //    var dist = math.sqrt(dx * dx + dz * dz);

        //    if (dist > 0 && dist < 80)
        //    {
        //        Rules.TryGetValue()
        //        var force = (RuleAmount * .01f) / dist;
        //        var velocityChange = new float3(dx * force, 0, dz * force);
        //        aspect.velocityChanges.Add(new VelocityChange { Value = velocityChange });
        //    }
        //}
    }
}
