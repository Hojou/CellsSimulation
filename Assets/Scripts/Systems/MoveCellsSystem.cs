using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(ApplyRulesSystem))]
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

        new MoveCellJob
        {
            DeltaTime = deltaTime,
        }.Run();
    }
}

[BurstCompile]
public partial struct MoveCellJob: IJobEntity
{
    public float DeltaTime;
    private void Execute(CellPropertiesAspect aspect, [EntityIndexInQuery] int sortKey)
    {
        float3 velocityChange = float3.zero;
        foreach (var change in aspect.velocityChanges)
        {
            velocityChange = velocityChange + change.Value;
        }
        aspect.velocityChanges.Clear();

        //Debug.Log($"{sortKey}: {velocityChange}");

        float3 newVelocity = (aspect.Velocity + velocityChange) * .5f;
        float3 newPos = aspect.LocalPosition + newVelocity * DeltaTime;

        bool flipX = newPos.x < -5f || newPos.x > 5f;
        bool flipZ = newPos.z < -5f || newPos.z > 5f;

        aspect.Velocity = new float3(
            flipX ? -newVelocity.x : newVelocity.x, 
            0, 
            flipZ ?-newVelocity.z : newVelocity.z
        );
        if (flipX) newPos.x = math.clamp(newPos.x, -5, 5);
        if (flipZ) newPos.z = math.clamp(newPos.z, -5, 5);
        aspect.LocalPosition =  newPos;
    }

}
