using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SimulationConfiguration", menuName = "Simulation/SimulationConfiguration")]

public class SimulationConfigurationSO : ScriptableObject
{
    private readonly char[] jsonExtension = ".json".ToCharArray();
    public float Strength;
    public float Scale;
    public float Influence;
    public uint RandomSeed;
    private readonly static string folderPath = "SavedConfigurations";

    [ListDrawerSettings(Expanded = true, ShowIndexLabels = false)]
    [OnValueChanged("UpdateCellConfigData")]
    public CellConfigData[] cells;

    [ListDrawerSettings(Expanded = true, ShowIndexLabels = false)]
    [OnValueChanged("UpdateCellConfigData")]
    public CellRuleData[] rules;
    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(_userFileName)) return name;
            return _userFileName[(folderPath.Length + 1)..].TrimEnd(jsonExtension);
        }
    }

    private List<CellConfigurationSO> _cellTypes;

    internal string _userFileName = null;

    private void OnValidate()
    {
        UpdateCellConfigData();
        LoadCellTypes();
    }

    public SimulationConfigurationSO Save()
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filename = _userFileName ?? FindNextFilename(name);
        string json = JsonUtility.ToJson(this);
        File.WriteAllText(filename, json);
        if (!string.IsNullOrEmpty(_userFileName))
        {
            return this;
        }

        var clone = ScriptableObject.CreateInstance<SimulationConfigurationSO>();
        JsonUtility.FromJsonOverwrite(json, clone);
        clone.name = Name;
        clone._userFileName = filename;
        return clone;
    }
    private static string GetFilename(int version, string name) => $"{folderPath}\\{name}{version}.json";

    private static string FindNextFilename(string currentName = "UserConfiguration")
    {
        string filename;
        var version = 1;
        do
        {
            filename = GetFilename(version++, currentName);
        } while (File.Exists(filename));
        return filename;
    }

    internal static SimulationConfigurationSO[] LoadUserConfigurations()
    {
        if (!Directory.Exists(folderPath))
        {
            return new SimulationConfigurationSO[] { };
            //return null;
        }

        var fileNames = Directory.GetFiles(folderPath);
        return fileNames.Select(filename =>
        {
            SimulationConfigurationSO configuration = ScriptableObject.CreateInstance<SimulationConfigurationSO>();
            string json = File.ReadAllText(filename);
            JsonUtility.FromJsonOverwrite(json, configuration);
            configuration._userFileName = filename;
            return configuration;
        }).ToArray();
    }

    public void SetRule(string Cell1Name, string Cell2Name, float Amount)
    {
        var index = Array.FindIndex(rules, rule => rule.Cell1.name == Cell1Name && rule.Cell2.name == Cell2Name);
        if (index == -1)
        {
            index = rules.Length;
            Array.Resize(ref rules, index + 1);
        }

        rules[index] = new CellRuleData
        {
            Cell1 = cells.First(cell => cell.cell.name == Cell1Name).cell,
            Cell2 = cells.First(cell => cell.cell.name == Cell2Name).cell,
            Amount = Amount
        };
    }

    public void SetCount(string cellName, int count)
    {
        var index = Array.FindIndex(cells, cell => cell.cell.name == cellName);
        if (index == -1)
        {
            index = cells.Length;
            Array.Resize(ref cells, index + 1);
        }

        var cell = cells[index].cell ?? NextFreeCell;
        cells[index] = new CellConfigData { cell = cell, Count = count };
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

    private CellConfigurationSO NextFreeCell => _cellTypes.First(ct => !cells.Select(c => c.cell).Contains(ct));

    private void UpdateCellConfigData()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].cell == null)
            {
                cells[i].cell = NextFreeCell;
            }
        }

        var list = cells?
            .Where(c => c.cell != null)
            .Select(c => new ValueDropdownItem<CellConfigurationSO>(c.cell.Name, c.cell)).ToList();

        for (int i = 0; i < rules.Length; i++)
        {
            rules[i].ListOfPossibleCells = list;
        }
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

