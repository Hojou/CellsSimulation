using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CellConfiguration", menuName = "Simulation/CellConfiguration")]
public class CellConfigurationSO : ScriptableObject
{
    [SerializeField]
    [AssetsOnly]
    private GameObject prefab;

    public string Name;
}
