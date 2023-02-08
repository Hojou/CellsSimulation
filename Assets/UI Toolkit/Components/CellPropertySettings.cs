using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class CellPropertySettings : VisualElement
{
    public new class UxmlFactory : UxmlFactory<CellPropertySettings> { }

    private VisualElement _rulesContainer;
    private TextField _cellInput;
    private Button _remove;

    public int Count => int.TryParse(_cellInput.value, out int result) ? result : 0;

    public event Action<CellPropertySettings, Rule> onRuleChanged;
    public event Action<CellPropertySettings, int> onCountChanged;
    public event Action<CellPropertySettings> onRemove;

    public CellPropertySettings() : this(new Rule("test", 0), Enumerable.Empty<Rule>())
    {
        //var rules = new List<Rule>
        //{
        //    new Rule("vs Black", .35f),
        //    new Rule("vs Red", -1.5f)
        //};
        //Init(new Rule("Pink", 1000), rules);
    }

    public CellPropertySettings(Rule cellProperties, IEnumerable<Rule> rules)
    {
        styleSheets.Add(Resources.Load<StyleSheet>("CellPropertySettings"));
        
        SetupCellConfiguration();
        Init(cellProperties, rules);
    }

    private void SetupCellConfiguration()
    {
        var container = new VisualElement();
        container.AddToClassList("cell-counter");
        container.style.flexDirection = FlexDirection.Row;
        _cellInput = new TextField("");
        _cellInput.style.flexGrow = 1;
        _cellInput.RegisterValueChangedCallback(CellCountChanged);
        container.Add(_cellInput);
        _remove = new Button();
        _remove.AddToClassList("close-button");
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
            onCountChanged?.Invoke(this, value);
        } 
        else
        {
            _cellInput.SetValueWithoutNotify("");
        }
    }

    public IEnumerable<Rule> GetRulesData()
    {
        var rules = _rulesContainer.Children().OfType<Slider>();
        return rules.Select(rule => new Rule
        {
            Id = (string)rule.userData,
            Value = rule.value
        });
    }

    public void Init(Rule cell, IEnumerable<Rule> rules)
    {
        var countValue = Math.Floor(cell.Value);
        _cellInput.SetValueWithoutNotify(countValue.ToString());
        _cellInput.label = cell.Label;

        _rulesContainer.Clear();
        foreach (var rule in rules)
        {
            UnityEngine.Debug.Log($"Cell {cell.Label}. vs {rule.Label}({rule.Id}): {rule.Value}");
            var slider = new Slider();
            slider.lowValue = -3f;
            slider.highValue = 3f;
            slider.label = rule.Label;
            slider.value = rule.Value;
            slider.userData = rule.Id;
            slider.showInputField = true;
            slider.RegisterValueChangedCallback(evt => onRuleChanged?.Invoke(this, new Rule(rule.Id, evt.newValue)));
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


