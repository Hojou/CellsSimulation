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
        Debug.Log($"{sortKey}: {velocityChange}");
        aspect.velocityChanges.Clear();

        float3 newVelocity = (aspect.cellProperties.ValueRW.Velocity + velocityChange) * .5f;
        float3 newPos = aspect.transform.LocalPosition + newVelocity * DeltaTime * .01f;

        bool flipX = newPos.x < -5f || newPos.x > 5f;
        bool flipZ = newPos.z < -5f || newPos.z > 5f;
        aspect.cellProperties.ValueRW.Velocity = new float3(
            flipX ? -newVelocity.x : newVelocity.x, 
            0, 
            flipZ ?-newVelocity.z : newVelocity.z
        );
        if (flipX)
        {
            //if (sortKey == 1) Debug.Log($"{newPos} | {newVelocity} : {aspect.cellProperties.ValueRW.Velocity}");
            newPos.x = math.clamp(newPos.x, -5, 5);
        }
        if (flipZ) newPos.z = math.clamp(newPos.z, -5, 5);
        aspect.transform.LocalPosition =  newPos;

        //if (newPos.x < -5f || newPos.x > 5f)
        //{
        //    aspect.cellProperties.ValueRW.Velocity = new float3(-newVelocity.x, newVelocity.y, newVelocity.z);
        //}

        //if (newPos.y < -5f || newPos.y > 5f)
        //{
        //    aspect.cellProperties.ValueRW.Velocity = new float3(newVelocity.x, -newVelocity.y, newVelocity.z);
        //}


        //aspect.transform.LocalPosition = newPos;

    }

}
