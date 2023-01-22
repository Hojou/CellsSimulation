using System.Diagnostics;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public partial struct ApplyRulesSystem : ISystem, ISystemStartStop
{
    private NativeArray<CellRule> _rules;
    private NativeHashMap<int, NativeArray<float3>> _cellsMap;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldProperties>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnStartRunning(ref SystemState state)
    {
        _cellsMap = new NativeHashMap<int, NativeArray<float3>>();
        var world = SystemAPI.GetSingletonEntity<WorldProperties>();
        var worldProperties = SystemAPI.GetAspectRW<WorldPropertiesAspect>(world);
        _rules = worldProperties.cellRules.AsNativeArray();

        //foreach (var group in SystemAPI.Query<CellPropertiesAspect>().GroupBy(c => c.Id))
            //foreach (var group in SystemAPI.Query<CellProperties>().GroupBy(c => c.Id))
            //{
            //    //var cellsInGroup = new NativeArray<float3>(group.Count(), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            //    //_cellsMap.Add(group.Key, cellsInGroup);
            //}


    }

    public void OnStopRunning(ref SystemState state)
    {
        //_cellsMap.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var deltaTime = SystemAPI.Time.DeltaTime;



        //NativeArray<float3> targetList;
        //foreach (var group in SystemAPI.Query<CellPropertiesAspect>().GroupBy(c => c.Id))
        //{
        //    if (!_cellsMap.TryGetValue(group.Key, out targetList)) continue;
        //    var i = 0;
        //    foreach (var cell in group)
        //    {
        //        targetList[i++] = cell.LocalPosition;
        //    }
        //}

        //NativeArray<float3> cell1Positions;
        //NativeArray<float3> cell2Positions;
        //foreach (var rule in _rules)
        //{
        //    if (!_cellsMap.TryGetValue(rule.Id1, out cell1Positions)) continue;
        //    if (!_cellsMap.TryGetValue(rule.Id2, out cell2Positions)) continue;
        //    var job = new ApplyRuleJob
        //    {
        //        DeltaTime = deltaTime,
        //        Cell1Positions = cell1Positions,
        //        Cell2Positions = cell2Positions,
        //        RuleAmount = rule.Amount,
        //    }.Schedule(100, 16);
        //}
    }
}

public partial struct ApplyRuleJob: IJobParallelFor
{
    public float DeltaTime;
    public float RuleAmount;
    [ReadOnly] public NativeArray<float3> Cell1Positions;
    [ReadOnly] public NativeArray<float3> Cell2Positions;
    public void Execute(int index)
    {
        //var pos = aspect.LocalPosition;
        Debug.WriteLine("Run " + index);
        //foreach (var cell1Position in Cell1Positions)
        //{
        //    var dx = pos.x - cellPosition.x;
        //    var dz = pos.z - cellPosition.z;
        //    var dist = math.sqrt(dx * dx + dz * dz);

        //    if (dist > 0 && dist < 80) {
        //        var force = (RuleAmount * .01f) / dist;
        //        var velocityChange = new float3(dx * force, 0, dz * force);
        //        aspect.velocityChanges.Add(new VelocityChange { Value = velocityChange });
        //    }
        //}


    }

}
