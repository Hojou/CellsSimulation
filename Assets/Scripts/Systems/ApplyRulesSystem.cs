using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Video;

[BurstCompile]
public partial struct ApplyRulesSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        //var cellProps = SystemAPI.Query<TransformAspect>().WithAll<CellProperties>();
        //var positions = cellProps.Select(c => c.LocalPosition).ToArray();
        //var cellPositions = new NativeArray<float3>();
        //cellPositions.CopyFrom(positions);

        var list = new NativeList<float3>(Allocator.Temp); // Can be cached
        foreach (var prop in SystemAPI.Query<TransformAspect>().WithAll<CellProperties>())
        {
            list.Add(prop.LocalPosition);
        }

        new ApplyRuleJob
        {
            DeltaTime = deltaTime,
            CellPositions = list.ToArray(Allocator.TempJob),
            RuleAmount = 100,
        }.Run();
    }
}

public partial struct ApplyRuleJob: IJobEntity
{
    public float DeltaTime;
    public NativeArray<float3> CellPositions;
    public float RuleAmount;
    private void Execute(CellPropertiesAspect aspect)
    {
        var pos = aspect.transform.LocalPosition;
        foreach (var cellPosition in CellPositions)
        {
            var dx = pos.x - cellPosition.x;
            var dz = pos.z - cellPosition.z;
            var dist = math.sqrt(dx * dx + dz * dz);

            if (dist > 0 && dist < 80) {
                var force = RuleAmount / dist;
                var velocityChange = new float3(dx * force, 0, dz * force);
                aspect.velocityChanges.Add(new VelocityChange { Value = velocityChange });
            }
        }


    }
}
