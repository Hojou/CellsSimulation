using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CellPropertySettings : VisualElement
{
    public new class UxmlFactory : UxmlFactory<CellPropertySettings> { }

    private VisualElement _rulesContainer;
    private TextField _cellInput;
    private Button _remove;

    public event Action<Rule> onRuleChanged;
    public event Action<float> onCountChanged;
    public event Action<CellPropertySettings> onRemove;

    public CellPropertySettings() : this(new Rule("test", 0), Enumerable.Empty<Rule>())
    {
    }

    public CellPropertySettings(Rule cellProperties, IEnumerable<Rule> rules)
    {
        styleSheets.Add(Resources.Load<StyleSheet>("CellPropertySettings"));
        
        SetupCellConfiguration();
        Init(cellProperties, rules);
        //SetupInitialPlaceholderValues();
    }

    private void SetupCellConfiguration()
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        _cellInput = new TextField("");
        _cellInput.style.flexGrow = 1;
        _cellInput.RegisterValueChangedCallback(CellCountChanged);
        container.Add(_cellInput);
        _remove = new Button();
        _remove.text = "X";
        _remove.clicked += () => onRemove?.Invoke(this);
        container.Add(_remove);
        hierarchy.Add(container);
        _rulesContainer = new VisualElement();
        hierarchy.Add(_rulesContainer);
    }


    private void CellCountChanged(ChangeEvent<string> evt)
    {
        if (int.TryParse(evt.newValue, out int value))
        {
            onCountChanged?.Invoke(value);
        } 
        else
        {
            _cellInput.SetValueWithoutNotify("");
        }
    }

    public void Init(Rule cell, IEnumerable<Rule> rules)
    {
        _cellInput.SetValueWithoutNotify(Math.Floor(cell.Value).ToString());
        _cellInput.label = cell.Label;

        _rulesContainer.Clear();
        foreach (var rule in rules)
        {
            var slider = new Slider();
            slider.label = rule.Label;
            slider.value = rule.Value;
            slider.RegisterValueChangedCallback(evt => onRuleChanged?.Invoke(new Rule(rule.Id, evt.newValue)));
            _rulesContainer.Add(slider);
        }
    }

    public struct Rule
    {
        public string Id;
        public string Label;
        public float Value;
        public float MinValue;
        public float MaxValue;

        public Rule(string label, float value) : this(label, value, label, 0, 100) { }
        public Rule(string label, float value, string id) : this(label, value, id, 0, 100) { }
        public Rule(string label, float value, string id, float minValue, float maxValue)
        {
            Label = label;
            Value = value;
            Id = id;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public static Rule Empty()
        {
            return new Rule("empty", 0);
        }
    }
}


