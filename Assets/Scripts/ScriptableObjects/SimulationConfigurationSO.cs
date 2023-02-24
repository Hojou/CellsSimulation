using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SimulationConfiguration", menuName = "Simulation/SimulationConfiguration")]

public class SimulationConfigurationSO : ScriptableObject
{
    public float Strength;
    //public float Speed;
    public float Scale;
    public float Influence;
    public uint RandomSeed;

    [ListDrawerSettings(Expanded = true, ShowIndexLabels = false)]
    [OnValueChanged("UpdateCellConfigData")]
    public CellConfigData[] cells;

    [OnValueChanged("UpdateCellConfigData")]
    [ShowInInlineEditors]
    public List<CellRuleData> rules = new List<CellRuleData>();

    private List<CellConfigurationSO> _cellTypes;

    private void OnValidate()
    {
        UpdateCellConfigData();
        LoadCellTypes();
    }

    private void LoadCellTypes()
    {
        CellConfigurationSO[] cellTypes = Resources.LoadAll<CellConfigurationSO>("Cells");
        if (cellTypes.Length > 32)
        {
            throw new System.Exception("Too many Cell resources. Maximum number is 32. Remove some from the Cells folder");
        }
        this._cellTypes = cellTypes.ToList();
    }

    private void UpdateCellConfigData()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].cell == null)
            {
                var nextCell = _cellTypes.First(ct => !cells.Select(c => c.cell).Contains(ct));
                cells[i].cell = nextCell;
            }
        }

        var list = cells?
            .Where(c => c.cell != null)
            .Select(c => new ValueDropdownItem<CellConfigurationSO>(c.cell.Name, c.cell)).ToList();

        rules.ForEach(rule => rule.ListOfPossibleCells = list);

        //for (int i = 0; i < rules.Length; i++)
        //{
        //    rules[i].ListOfPossibleCells = list;
        //}

    }

    

    [Serializable]
    public struct CellConfigData
    {
        [HideInInspector]
        public CellConfigurationSO cell;

        [ShowInInspector]
        public string Name => cell?.Name;

        public int Count;

    }

    [Serializable]

    public struct CellRuleData
    {
        [ValueDropdown("ListOfPossibleCells"), Required]
        public CellConfigurationSO Cell1;
        [ValueDropdown("ListOfPossibleCells"), Required]
        public CellConfigurationSO Cell2;

        public float Amount;

        [NonSerialized]
        internal List<ValueDropdownItem<CellConfigurationSO>> ListOfPossibleCells;
    }


}

