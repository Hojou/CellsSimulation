using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


public class FloorScript : MonoBehaviour
{

    public class FloorBaker : Baker<FloorScript>
    {
        public override void Bake(FloorScript authoring)
        {
            AddComponent<FloorTag>();
        }
    }
}

public struct FloorTag : IComponentData { }
