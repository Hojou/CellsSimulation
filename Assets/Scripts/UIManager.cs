using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private VisualElement _root;
    private Slider _speed;
    private Slider _strength;
    private Slider _influence;
    private Button _addButton;
    private VisualElement _cellConfigurations;

    public float2 Dimension;
    public event Action<float> onSpeedChanged;
    public event Action<float> onStrengthChanged;
    public event Action<float2> onDimensionChanged;
    public event Action<float> onInfluenceChanged;
    public event Action<uint> onSeedChanged;
    public event Action onSettingsLoaded;

    public readonly SyncableValueHolder<bool> Rules = new SyncableValueHolder<bool>(false);
    public readonly SyncableValueHolder<bool> CellCount = new SyncableValueHolder<bool>(false);

    [SerializeField] private List<SimulationConfigurationSO> simulationConfigurations;

    private SimulationConfigurationSO _currentSimulation;

    public float Speed
    {
        get => _speed?.value ?? 0;
        private set => _speed.SetValueWithoutNotify(value);
    }

    public float Strength
    {
        get => _strength?.value ?? 0;
        private set => _strength.SetValueWithoutNotify(value);
    }

    public float Influence
    {
        get => _influence?.value ?? 0;
        private set => _influence.SetValueWithoutNotify(value);
    }

    public uint Seed
    {
        private set; get;
    }

    void Start()
    {
        CalculateDimensions();
        _root = GetComponent<UIDocument>().rootVisualElement;
        _speed = _root.Query<Slider>(name: "Speed");
        _strength = _root.Query<Slider>(name: "Strength");
        _influence= _root.Query<Slider>(name: "Influence");
        _addButton = _root.Query<Button>(name: "AddButton");
        _cellConfigurations = _root.Query<VisualElement>(name: "CellConfigurations");

        _speed.RegisterValueChangedCallback(evt => onSpeedChanged?.Invoke(evt.newValue));
        _strength.RegisterValueChangedCallback(evt => onStrengthChanged?.Invoke(evt.newValue));
        _influence.RegisterValueChangedCallback(evt => onInfluenceChanged?.Invoke(evt.newValue));
        //_addButton.clicked += _addButton_clicked;
        onDimensionChanged?.Invoke(Dimension);
        onSeedChanged?.Invoke(1337);

        LoadConfiguration();
    }

    public SimulationConfigurationSO CurrentConfiguration => _currentSimulation;

    private void LoadConfiguration()
    {
        _currentSimulation = ScriptableObject.Instantiate(simulationConfigurations[0]);
        Strength = _currentSimulation.Strength;
        Speed = _currentSimulation.Speed;
        Seed = _currentSimulation.RandomSeed;
        _cellConfigurations.Clear();
        var rules = _currentSimulation.rules.ToList();
        var cells = _currentSimulation.cells.Select(c => new { c.cell.Name, c.Count, c.cell }).ToList();
        for (int i = 0; i < cells.Count(); i++)
        {
            var cell = cells[i];
            var config = new CellPropertySettings.CellConfig
            {
                Id = cell.cell.name,
                Name = cell.Name,
                Count = cell.Count,
            };
            var cellRules = cells.Select(c => new CellPropertySettings.Rule
            {
                Id = cell.cell.name,
                Label = $"{cell.Name} vs {c.Name}",
                MinValue = -3f,
                MaxValue = 3f,
                Value = rules.FirstOrDefault(r => r.Cell1 == cell.cell && r.Cell2 == c.cell).Amount
            }).ToList();

            UnityEngine.Debug.Log($"===========> Cell:{cell.Name}, #Rules:{cellRules.Count}");
            AddCellConfig(config, cellRules);
        }

        Strength = _currentSimulation.Strength;
        Speed = _currentSimulation.Speed;
        Influence = _currentSimulation.Influence;

        UnityEngine.Debug.Log("UI MANAGER LOADED");
        onSettingsLoaded?.Invoke();
    }

    //private void _addButton_clicked()
    //{
    //    var cellSettings = new CellPropertySettings();
    //    cellSettings.onRuleChanged += _settings_onRuleChanged;
    //    cellSettings.onCountChanged += _settings_onCountChanged;
    //    cellSettings.onRemove += _cellSettings_onRemove;
    //    _cellConfigurations.Add(cellSettings);
    //}

    //private void _cellSettings_onRemove(CellPropertySettings cellSettings)
    //{
    //    _cellConfigurations.Remove(cellSettings);
    //}

    private void _settings_onCountChanged(CellPropertySettings cellSettings, int count)
    {
        //Debug.Log($"Count changed: {count} for {cellSettings.userData}");
        var cellName = (string)cellSettings.userData;

        var length = _currentSimulation.rules.Length;
        for (int i = 0; i < length; i++)
        {
            var c = _currentSimulation.cells[i];
            if (c.cell.name != cellName) continue;
            _currentSimulation.cells[i].Count = count;
        }

        CellCount.SetDirty();
    }

    private void _settings_onRuleChanged(CellPropertySettings cellSettings, CellPropertySettings.Rule rule)
    {
        //Debug.Log($"Rule changed for {cellSettings.userData}-{rule.Id}: {rule.Value}");
        string cell1Name = (string)cellSettings.userData;
        string cell2Name = rule.Id;
        float amount = rule.Value;

        var length = _currentSimulation.rules.Length;
        for (int i = 0; i < length; i++)
        {
            var r = _currentSimulation.rules[i];
            if (r.Cell1.name != cell1Name || r.Cell2.name != cell2Name) continue;
            _currentSimulation.rules[i].Amount = amount;
        }
        
        Rules.SetDirty();
    }

    private void CalculateDimensions()
    {
        float orthoSize = Camera.main.orthographicSize;
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float width = orthoSize * aspectRatio;
        float height = orthoSize;
        Dimension = new Vector2(width * 2, height * 2);
    }

    private void AddCellConfig(CellPropertySettings.CellConfig config, IEnumerable<CellPropertySettings.Rule> rules)
    {
        var cellSettings = new CellPropertySettings(config, rules);
        cellSettings.userData = config.Id;
        cellSettings.onRuleChanged += _settings_onRuleChanged;
        cellSettings.onCountChanged += _settings_onCountChanged;
        //cellSettings.onRemove += _cellSettings_onRemove;
        _cellConfigurations.Add(cellSettings);
    }

    public struct UIRule
    {
        public string Cell1;
        public string Cell2;
        public float Amount;
    }

    public struct UICount
    {
        public string Cell;
        public int Count;
    }
}
