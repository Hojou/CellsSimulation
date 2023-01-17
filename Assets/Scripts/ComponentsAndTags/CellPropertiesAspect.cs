using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct CellPropertiesAspect : IAspect
{
    public readonly RefRW<CellProperties> cellProperties;
    public readonly TransformAspect transform;
}
