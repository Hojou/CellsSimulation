using Unity.Collections;
using Unity.Entities;

public struct CellRules : IComponentData
{
    public NativeArray<float> Value; // Id1*32+Id2
}
