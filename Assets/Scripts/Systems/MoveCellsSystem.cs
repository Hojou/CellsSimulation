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
        var dimensions = aspect.Dimensions;
        var deltaTime = math.min(0.05f, SystemAPI.Time.DeltaTime);

        var handle = new MoveCellJob
        {
            DeltaTime = deltaTime * aspect.Speed,
            MinX = -(dimensions.x / 2),
            MaxX = (dimensions.x / 2),
            MinY = -(dimensions.y / 2),
            MaxY= (dimensions.y / 2),
        }.ScheduleParallel(state.Dependency);

        state.Dependency = handle;
    }
}

[BurstCompile]
public partial struct MoveCellJob: IJobEntity
{
    public float DeltaTime;
    public float MinX;
    public float MaxX;
    public float MinY;
    public float MaxY;
    private void Execute(ref CellPropertiesAspect aspect)
    {
        var velocity = aspect.Velocity;
        float3 newPos = aspect.LocalPosition + velocity * DeltaTime;

        bool flipX = newPos.x <= -5f || newPos.x >= 5f;
        bool flipZ = newPos.z <= -5f || newPos.z >= 5f;

        if (flipX || flipZ)
        {
            velocity = new float3(
                flipX ? -velocity.x * 1: velocity.x,
                0,
                flipZ ? -velocity.z * 1: velocity.z
            );
            aspect.Velocity = velocity;

            if (flipX) newPos.x = math.clamp(newPos.x, -5, 5);
            if (flipZ) newPos.z = math.clamp(newPos.z, -5, 5);
        }

        aspect.LocalPosition = aspect.LocalPosition + velocity * DeltaTime;
    }

}
