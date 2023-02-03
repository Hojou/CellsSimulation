using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using sRule = CellPropertySettings.Rule;

public partial class ManagedCodeBridge : SystemBase
{
    private UIManager _uiManager;
    
    private readonly SyncableValueHolder<float> _speed = new SyncableValueHolder<float>(1);
    private readonly SyncableValueHolder<float> _strength = new SyncableValueHolder<float>(1);
    private readonly SyncableValueHolder<float2> _dimension = new SyncableValueHolder<float2>(new float2(5, 5));
    private readonly Dictionary<string, Tuple<sRule, IEnumerable<sRule>>> _rulesToProcess = new Dictionary<string, Tuple<sRule, IEnumerable<sRule>>>();

    protected override void OnCreate()
    {
        UnityEngine.Debug.Log("OnCreate");
        RequireForUpdate<WorldProperties>();
        _uiManager = GameObject.FindObjectOfType<UIManager>();
        _uiManager.onSpeedChanged += _speed.SetValue;                                                                                           
        _uiManager.onStrengthChanged += _strength.SetValue;
        _uiManager.onDimensionChanged += _dimension.SetValue;
        _uiManager.onRuleChanged += (cell, rules) => _rulesToProcess.TryAdd(cell.Id, new Tuple<sRule, IEnumerable<sRule>>(cell, rules));
    }

    protected override void OnStartRunning()
    {
        UnityEngine.Debug.Log("InitializeCellConfigurations");
        InitializeCellConfigurations();
    }

    private void InitializeCellConfigurations()
    {
        UnityEngine.Debug.Log("Initialize");
        _rulesToProcess.Clear();

        var entity = SystemAPI.GetSingletonEntity<WorldProperties>();
        var world = SystemAPI.GetAspectRO<WorldPropertiesAspect>(entity);

        var cellConfigurations = world.CellConfigurations;
        using var rules = world.CellRules;

        foreach (var config in cellConfigurations)
        {
            var cellConfig = new sRule
            {
                Id = config.Id.ToString(),
                Label = config.Name.ToString(),
                Value = config.NumberOfCells
            };

            var cellRules = new List<sRule>();
            foreach (var rule in rules)
            {
                if (rule.Id1 != config.Id) continue;
                var vsName = cellConfigurations.First(r => r.Id == rule.Id2).Name;
                cellRules.Add(new sRule
                {
                    Id = rule.Id1.ToString(),
                    Label = $"vs {vsName}",
                    Value = rule.Amount
                });
            }

            _uiManager.AddCellConfig(cellConfig, cellRules);
        }
    }

    protected override void OnUpdate()
    {
        UnityEngine.Debug.Log("OnUpdate");
        var properties = SystemAPI.GetSingletonRW<WorldProperties>();
        _speed.SyncValue(ref properties.ValueRW.Speed);
        _strength.SyncValue(ref properties.ValueRW.Strength);
        _dimension.SyncValue(ref properties.ValueRW.Dimension);
        if (_rulesToProcess.Any())
        {
            UpdateRules();
        }
    }

    private void UpdateRules()
    {
        //var entity = SystemAPI.GetSingletonEntity<WorldProperties>();
        //SystemAPI.GetAspectRW<WorldPropertiesAspect>(entity);

        Debug.Log("Rules to process " + _rulesToProcess.Count());
        var (cell, rules) = _rulesToProcess.First().Value;
        var firstRule = rules.First();
        Debug.Log($"Cell:{cell.Id}={cell.Value}. rule value: {firstRule.Value}");
        _rulesToProcess.Clear();

        //_uiManager.
    }
}
