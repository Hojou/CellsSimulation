using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct MoveCellsSystem : ISystem
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

        new MoveCellsJobs
        {
            DeltaTime = deltaTime,
        }.Run();
    }
}

[BurstCompile]
public partial struct MoveCellsJobs: IJobEntity
{
    public float DeltaTime;
    private void Execute(CellPropertiesAspect aspect)
    {
        aspect.transform.LocalPosition += aspect.cellProperties.ValueRO.Velocity * DeltaTime;
    }

}
