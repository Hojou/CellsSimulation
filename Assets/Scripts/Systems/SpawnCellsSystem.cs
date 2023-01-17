using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnCellsSystem : ISystem
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
        state.Enabled = false;
        var properties = SystemAPI.GetSingletonEntity<WorldProperties>();
        var aspect = SystemAPI.GetAspectRW<WorldPropertiesAspect>(properties);
        var buffer = SystemAPI.GetBuffer<CellConfigurationProperties>(properties);
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var property in buffer) {

            for (int i = 0; i < property.NumberOfCells; i++)
            {
                var cell = ecb.Instantiate(property.Prefab);
                ecb.SetComponent(cell, new LocalTransform
                {
                    Position = aspect.GetRandomPosition(),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                ecb.AddComponent(cell, new CellProperties {  Velocity = new float3(.1f, 0, 0) });
            }
        }

        //buffer.Clear();
        ecb.Playback(state.EntityManager);
    } 
}
