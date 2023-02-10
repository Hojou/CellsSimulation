using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private VisualElement _root;
    private Slider _speed;
    private Slider _strength;
    private Button _addButton;
    private VisualElement _cellConfigurations;

    public float2 Dimension;
    public event Action<float> onSpeedChanged;
    public event Action<float> onStrengthChanged;
    public event Action<float2> onDimensionChanged;

    public event Action<CellPropertySettings.Rule, IEnumerable<CellPropertySettings.Rule>> onRuleChanged;

    public List<SimulationConfigurationSO> simulationConfigurations;

    public float Speed
    {
        get => _speed.value; 
        set => _speed.SetValueWithoutNotify(value);
    }

    public float Strength
    {
        get => _strength.value;
        set => _strength.SetValueWithoutNotify(value);
    }

    public 

    void Start()
    {
        CalculateDimensions();
        _root = GetComponent<UIDocument>().rootVisualElement;
        _speed = _root.Query<Slider>(name: "Speed");
        _strength = _root.Query<Slider>(name: "Strength");
        _addButton = _root.Query<Button>(name: "AddButton");
        _cellConfigurations = _root.Query<VisualElement>(name: "CellConfigurations");

        _speed.RegisterValueChangedCallback(evt => onSpeedChanged?.Invoke(evt.newValue / 100f));
        _strength.RegisterValueChangedCallback(evt => onStrengthChanged?.Invoke(evt.newValue / 100f));
        //_addButton.clicked += _addButton_clicked;
        onDimensionChanged?.Invoke(Dimension);
    }

    //private void _addButton_clicked()
    //{
    //    var cellSettings = new CellPropertySettings();
    //    cellSettings.onRuleChanged += _settings_onRuleChanged;
    //    cellSettings.onCountChanged += _settings_onCountChanged;
    //    cellSettings.onRemove += _cellSettings_onRemove;
    //    _cellConfigurations.Add(cellSettings);
    //}

    private void _cellSettings_onRemove(CellPropertySettings cellSettings)
    {
        _cellConfigurations.Remove(cellSettings);
    }

    private void _settings_onCountChanged(CellPropertySettings cellSettings, int count)
    {
        //Debug.Log($"Count changed: {count}");
        InvokeRuleChanged(cellSettings);
    }

    private void _settings_onRuleChanged(CellPropertySettings cellSettings, CellPropertySettings.Rule rule)
    {
        //Debug.Log($"Rule changed for {rule.Id}: {rule.Value}");
        InvokeRuleChanged(cellSettings);
    }

    private void CalculateDimensions()
    {
        float orthoSize = Camera.main.orthographicSize;
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float width = orthoSize * aspectRatio;
        float height = orthoSize;
        Dimension = new Vector2(width * 2, height * 2);
    }

    internal void AddCellConfig(CellPropertySettings.CellConfig config, IEnumerable<CellPropertySettings.Rule> rules)
    {

        var cellSettings = new CellPropertySettings(config, rules);
        //cellSettings.userData = config.Id;
        cellSettings.onRuleChanged += _settings_onRuleChanged;
        cellSettings.onCountChanged += _settings_onCountChanged;
        cellSettings.onRemove += _cellSettings_onRemove;
        _cellConfigurations.Add(cellSettings);
    }

    private void InvokeRuleChanged(CellPropertySettings cellSettings)
    {
        var cellRule = new CellPropertySettings.Rule
        {
            Id = (string)cellSettings.userData,
            Value = cellSettings.Count
        };
        var rules = cellSettings.GetRulesData();
        onRuleChanged?.Invoke(cellRule, rules);
    }
}
