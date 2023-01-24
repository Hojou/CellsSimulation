using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(ApplyRulesSystem))]
public partial struct MoveCellsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldProperties>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var properties = SystemAPI.GetSingletonEntity<WorldProperties>();
        var aspect = SystemAPI.GetAspectRW<WorldPropertiesAspect>(properties);
        
        var deltaTime = math.min(0.05f, SystemAPI.Time.DeltaTime);


        var handle = new MoveCellJob
        {
            DeltaTime = deltaTime * aspect.Speed,
        }.Schedule(state.Dependency); // where to pass in handle?

        state.Dependency = handle;
    }
}

[BurstCompile]
public partial struct MoveCellJob: IJobEntity
{
    public float DeltaTime;
    private void Execute(ref CellPropertiesAspect aspect)
    {
        float3 velocityChange = float3.zero;
        foreach (var change in aspect.velocityChanges)
        {
            velocityChange = velocityChange + change.Value;
        }
        aspect.velocityChanges.Clear();

        Debug.Log($"{velocityChange}");

        float3 newVelocity = (aspect.Velocity + velocityChange) * .75f;
        float3 newPos = aspect.LocalPosition + newVelocity * DeltaTime;
        aspect.LocalPosition= aspect.LocalPosition + newVelocity * DeltaTime;

        bool flipX = (newPos.x <= -5f || newPos.x >= 5f);
        bool flipZ = newPos.z <= -5f || newPos.z >= 5f;

        newVelocity = new float3(
            flipX ? -newVelocity.x * 3: newVelocity.x,
            0,
            flipZ ? -newVelocity.z * 3: newVelocity.z
        );
        aspect.Velocity = newVelocity;
        //if (flipX) newPos.x = math.clamp(newPos.x, -5, 5);
        //if (flipZ) newPos.z = math.clamp(newPos.z, -5, 5);
    }

}
