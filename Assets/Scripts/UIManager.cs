using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static SimulationConfigurationSO;

public class UIManager : MonoBehaviour
{
    private VisualElement _root;
    //private Slider _speed;
    private Slider _strength;
    private Slider _influence;
    private Button _openClose;
    private Button _reset;
    private DropdownField _configurationDropdown;
    //private Button _addButton;
    private VisualElement _cellConfigurations;
    private VisualElement _menu;

    public float2 Dimension;
    //public event Action<float> onSpeedChanged;
    //public event Action<float> onStrengthChanged;
    //public event Action<float2> onDimensionChanged;
    //public event Action<float> onInfluenceChanged;
    //public event Action<uint> onSeedChanged;

    public readonly DirtyTracker Rules = new DirtyTracker();
    public readonly DirtyTracker CellCount = new DirtyTracker();
    public readonly DirtyTracker SettingsLoaded = new DirtyTracker();
    public readonly DirtyTracker Properties = new DirtyTracker();

    public bool IsDirty => Properties.IsDirty || SettingsLoaded.IsDirty || Rules.IsDirty || CellCount.IsDirty;

    [SerializeField] private List<SimulationConfigurationSO> simulationConfigurations;

    private SimulationConfigurationSO _currentSimulation;

    [ShowInInspector]
    private List<CellRuleData> CellRules => _currentSimulation?.rules;

    //private float Speed
    //{
    //    get => _speed?.value ?? 0;
    //    set => _speed.SetValueWithoutNotify(value);
    //}

    private float Strength
    {
        get => _strength?.value ?? 0;
        set => _strength.SetValueWithoutNotify(value);
    }

    float Influence
    {
        get => _influence?.value ?? 0;
        set => _influence.SetValueWithoutNotify(value);
    }

    public uint Seed
    {
        private set; get;
    }

    void Start()
    {
        CalculateDimensions();
        _root = GetComponent<UIDocument>().rootVisualElement;
        //_speed = _root.Query<Slider>(name: "Speed");
        _strength = _root.Query<Slider>(name: "Strength");
        _influence = _root.Query<Slider>(name: "Influence");
        _openClose = _root.Query<Button>(name: "OpenCloseButton");
        _reset = _root.Query<Button>(name: "ResetButton");
        _configurationDropdown = _root.Query<DropdownField>(name: "ConfigDropdown");
        _cellConfigurations = _root.Query<VisualElement>(name: "CellConfigurations");
        _menu = _root.Query<VisualElement>(name: "Menu");

        //_speed.RegisterValueChangedCallback(UpdateProperties);
        _strength.RegisterValueChangedCallback(UpdateProperties);
        _influence.RegisterValueChangedCallback(UpdateProperties);

        _openClose.clicked += ToggleMenu;
        _reset.clicked += ResetSimulation;
        //_addButton.clicked += _addButton_clicked;
        //onDimensionChanged?.Invoke(Dimension);
        //onSeedChanged?.Invoke(1337);

        _configurationDropdown.choices = simulationConfigurations.Select(c => c.name).ToList();
        _configurationDropdown.RegisterValueChangedCallback(ConfigurationSelected);
        _configurationDropdown.value = simulationConfigurations[0].name;
        _currentSimulation = ScriptableObject.Instantiate(simulationConfigurations[0]);

        LoadConfiguration();
    }

    private void ConfigurationSelected(ChangeEvent<string> evt)
    {
        var config = simulationConfigurations.Find(c => c.name == evt.newValue);
        if (config == null) { return; }
        _configurationDropdown.value = config.name;
        _currentSimulation = ScriptableObject.Instantiate(config);
        LoadConfiguration();
    }

    private void UpdateProperties(ChangeEvent<float> evt)
    {
        CurrentConfiguration.Strength = Strength;
        //CurrentConfiguration.Speed = Speed;
        CurrentConfiguration.Influence = Influence;

        Properties.SetDirty();
    }

    private void ResetSimulation()
    {
        Debug.Log("Resetting sim");
        LoadConfiguration();
    }

    private void ToggleMenu()
    {
        var open = _menu.style.display != DisplayStyle.None;
        var newState = open ? DisplayStyle.None : DisplayStyle.Flex;
        var text = open ? ">" : "X";

        _menu.style.display = newState;
        _openClose.text = text;
    }

    public SimulationConfigurationSO CurrentConfiguration => _currentSimulation;

    private void LoadConfiguration()
    {
        Strength = _currentSimulation.Strength;
        //Speed = _currentSimulation.Speed;
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
                Id = c.cell.name,
                Label = $"{cell.Name} vs {c.Name}",
                MinValue = -3f,
                MaxValue = 3f,
                Value = rules.FirstOrDefault(r => r.Cell1 == cell.cell && r.Cell2 == c.cell).Amount
            }).ToList();
            foreach (var otherCell in cells)
            {
                if (_currentSimulation.rules.Any(s => s.Cell1 == cell.cell && s.Cell2 == otherCell.cell))
                {
                    continue;
                }
                var newRule = new CellRuleData
                {
                    Cell1 = cell.cell,
                    Cell2 = otherCell.cell,
                    Amount = 0
                };
                _currentSimulation.rules.Add(newRule);
            }

            AddCellConfig(config, cellRules);
        }

        Strength = _currentSimulation.Strength;
        //Speed = _currentSimulation.Speed;
        Influence = _currentSimulation.Influence;

        SettingsLoaded.SetDirty();
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
        var cellName = (string)cellSettings.userData;

        var length = _currentSimulation.cells.Length;
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
        string cell1Name = (string)cellSettings.userData;
        string cell2Name = rule.Id;

        var ruleIndex = _currentSimulation.rules.FindIndex(r => (r.Cell1.name == cell1Name && r.Cell2.name == cell2Name));
        var newRule = _currentSimulation.rules[ruleIndex];
        newRule.Amount = rule.Value;
        _currentSimulation.rules[ruleIndex] = newRule;
        
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
