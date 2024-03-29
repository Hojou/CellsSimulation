using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[RequireMatchingQueriesForUpdate]
[UpdateAfter(typeof(ApplyRulesSystem))]
public partial struct MoveCellsSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = math.min(0.05f, SystemAPI.Time.DeltaTime);
        var worlProperties = SystemAPI.GetSingleton<WorldProperties>();
        var dimension = worlProperties.Dimension;

        new MoveCellJob
        {
            DeltaTime = deltaTime * worlProperties.Strength,
            MinX = -(dimension.x / 2),
            MaxX = (dimension.x / 2),
            MinY = -(dimension.y / 2),
            MaxY= (dimension.y / 2),
        }.ScheduleParallel();
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
    
    [BurstCompile]
    private void Execute(ref CellPropertiesAspect aspect)
    {
        var velocity = aspect.Velocity;
        float3 newPos = aspect.LocalPosition + velocity * DeltaTime;

        bool flipX = newPos.x <= MinX || newPos.x >= MaxX;
        bool flipZ = newPos.z <= MinY || newPos.z >= MaxY;

        if (flipX || flipZ)
        {
            velocity = new float3(
                flipX ? -velocity.x : velocity.x,
                0,
                flipZ ? -velocity.z : velocity.z
            );
            aspect.Velocity = velocity;
            newPos = aspect.LocalPosition + velocity * DeltaTime;
        }

        newPos.x = math.clamp(newPos.x, MinX, MaxX);
        newPos.y = math.clamp(newPos.y, MinY, MaxY);
        aspect.LocalPosition = newPos;
    }

}
