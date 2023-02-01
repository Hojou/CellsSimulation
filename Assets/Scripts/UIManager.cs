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
    //public event Action<Tuple<Event>>

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
        _addButton.clicked += _addButton_clicked;
        onDimensionChanged?.Invoke(Dimension);
    }


    private void _addButton_clicked()
    {
        var cellSettings = new CellPropertySettings();
        cellSettings.onRuleChanged += rule => _settings_onRuleChanged(rule, cellSettings);
        cellSettings.onCountChanged += _settings_onCountChanged;
        cellSettings.onRemove += _cellSettings_onRemove;
        _cellConfigurations.Add(cellSettings);
    }

    private void _cellSettings_onRemove(CellPropertySettings obj)
    {
        _cellConfigurations.Remove(obj);
    }

    private void _settings_onCountChanged(float count)
    {
        Debug.Log($"Count changed: {count}");
    }

    private void _settings_onRuleChanged(CellPropertySettings.Rule rule, CellPropertySettings cellSettings)
    {
        Debug.Log($"Rule changed for {rule.Id}: {rule.Value}");
    }

    private void CalculateDimensions()
    {
        float orthoSize = Camera.main.orthographicSize;
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float width = orthoSize * aspectRatio;
        float height = orthoSize;
        Dimension = new Vector2(width * 2, height * 2);
    }

    internal void AddCellConfig(CellPropertySettings.Rule config, IEnumerable<CellPropertySettings.Rule> rules)
    {
        //var cell = new CellPropertySettings.Rule
        //{
        //    Id = config.Id.ToString(),
        //    Label = config.Name.ToString(),
        //    Value = config.NumberOfCells
        //};
        //var cellRules = new List<CellPropertySettings.Rule>();
        //foreach (var rule in rules)
        //{
        //    cellRules.Add(new CellPropertySettings.Rule
        //    {
        //        Id = rule.Id1.ToString(),
        //        Label = $"vs{rule.Id2}",
        //        Value = rule.Amount
        //    });
        //}
        var cellConfig = new CellPropertySettings(config, rules);
        _cellConfigurations.Add(cellConfig);
    }
}
