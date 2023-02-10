using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SimulationConfiguration", menuName = "Simulation/SimulationConfiguration")]

public class SimulationConfigurationSO : ScriptableObject
{
    [ListDrawerSettings(Expanded = true, ShowIndexLabels = false)]
    [OnValueChanged("UpdateCellConfigData")]
    [SerializeField] CellConfigData[] cells;

    [OnValueChanged("UpdateCellConfigData")]
    [SerializeField] CellRuleData[] rules;

    private void OnValidate()
    {
        UpdateCellConfigData();
    }

    private void UpdateCellConfigData()
    {
        var list = cells?
            .Where(c => c.cell != null)
            .Select(c => new ValueDropdownItem<CellConfigurationSO>(c.cell.Name, c.cell)).ToList();

        for (int i = 0; i < rules.Length; i++)
        {
            rules[i].ListOfCells = list;
        }
    }

    [Serializable]
    public struct CellConfigData
    {
        public CellConfigurationSO cell;
        public int Count;
    }

    [Serializable]
    public struct CellRuleData
    {
        [ValueDropdown("ListOfCells"), Required]
        public CellConfigurationSO Cell1;
        [ValueDropdown("ListOfCells"), Required]
        public CellConfigurationSO Cell2;

        public float Amount;

        [NonSerialized]
        internal List<ValueDropdownItem<CellConfigurationSO>> ListOfCells;
    }


}

