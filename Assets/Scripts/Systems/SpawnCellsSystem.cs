using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct SharedRulesGrouping : ISharedComponentData
{
    public int Group;
    public NativeArray<RuleAmount> Rules;
}

//public struct SharedRulesForGrouping : ISharedComponentData
//{
//    public int GroupFor;
//    public float Amount;
//}

[Serializable]
public struct RuleAmount { public int Id; public float Amount; }

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
            var rules = new NativeList<RuleAmount>(Allocator.Temp);
            foreach (var rule in aspect.cellRules)
            {
                if (rule.Id1 != property.Id) continue;
                rules.Add(new RuleAmount {  Id = rule.Id2, Amount = rule.Amount });
            }
            var rulesComponent = new SharedRulesGrouping
            {
                Group = property.Id,
                Rules = rules.ToArray(Allocator.Persistent)
            };

            for (int i = 0; i < property.NumberOfCells; i++)
            {
                var cell = ecb.Instantiate(property.Prefab);
                ecb.SetComponent(cell, new LocalTransform
                {
                    Position = aspect.GetRandomPosition(),
                    Rotation = quaternion.identity,
                    Scale = aspect.Scale
                });

                ecb.AddComponent(cell, new CellProperties {  
                    Id = property.Id,
                    Velocity = new float3(0, 0, 0),
                });

                ecb.AddSharedComponent(cell, rulesComponent);
                //foreach (var rule in aspect.cellRules) // TODO: Not using this?
                //{
                //    if (rule.Id2 != property.Id) continue;  
                //    var groupFor = new SharedRulesForGrouping {  GroupFor = rule.Id1 };
                //    ecb.AddSharedComponent(cell, groupFor);
                //}

                ecb.AddBuffer<VelocityChange>(cell);
            }
        }

        //buffer.Clear();
        ecb.Playback(state.EntityManager);
    } 
}
