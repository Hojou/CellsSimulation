using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using sRule = CellPropertySettings.Rule;
using sConfig = CellPropertySettings.CellConfig;

public partial class ManagedCodeBridge : SystemBase
{
    private UIManager _uiManager;
    
    private readonly SyncableValueHolder<float> _speed = new SyncableValueHolder<float>(1);
    private readonly SyncableValueHolder<float> _strength = new SyncableValueHolder<float>(1);
    private readonly SyncableValueHolder<float2> _dimension = new SyncableValueHolder<float2>(new float2(5, 5));
    private readonly Dictionary<string, Tuple<sRule, IEnumerable<sRule>>> _rulesToProcess = new Dictionary<string, Tuple<sRule, IEnumerable<sRule>>>();

    EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate()
    {
        //UnityEngine.Debug.Log("OnCreate");
        RequireForUpdate<WorldProperties>();
        _uiManager = GameObject.FindObjectOfType<UIManager>();
        _uiManager.onSpeedChanged += _speed.SetValue;                                                                                           
        _uiManager.onStrengthChanged += _strength.SetValue;
        _uiManager.onDimensionChanged += _dimension.SetValue;
        _uiManager.onRuleChanged += (cell, rules) => _rulesToProcess.TryAdd(cell.Id, new Tuple<sRule, IEnumerable<sRule>>(cell, rules));
    }

    protected override void OnStartRunning()
    {
        //UnityEngine.Debug.Log("InitializeCellConfigurations");
        InitializeCellConfigurations();
    }

    private void InitializeCellConfigurations()
    {
        //UnityEngine.Debug.Log("Initialize");
        _rulesToProcess.Clear();

        var entity = SystemAPI.GetSingletonEntity<WorldProperties>();
        var world = SystemAPI.GetAspectRO<WorldPropertiesAspect>(entity);

        var cellConfigurations = world.CellConfigurations;
        using var rules = world.CellRules;

        foreach (var config in cellConfigurations)
        {
            var cellConfig = new sConfig
            {
                //Id = config.Id.ToString(),
                Name = config.Name.ToString(),
                Count = config.NumberOfCells
            };

            var cellRules = new List<sRule>();
            foreach (var rule in rules)
            {
                if (rule.Id1 != config.Id) continue;
                var vsName = cellConfigurations.Single(r => r.Id == rule.Id2).Name;
                //UnityEngine.Debug.Log($"Rule Id{rule.Id1}: vs {rule.Id2}({vsName}), value:{rule.Amount}");
                cellRules.Add(new sRule
                {
                    Id = rule.Id2.ToString(),
                    Label = $"vs {vsName}",
                    Value = rule.Amount
                });
            }

            _uiManager.AddCellConfig(cellConfig, cellRules);
        }
    }

    protected override void OnUpdate()
    {
        //UnityEngine.Debug.Log("OnUpdate");
        var properties = SystemAPI.GetSingletonRW<WorldProperties>();
        _speed.SyncValue(ref properties.ValueRW.Speed);
        _strength.SyncValue(ref properties.ValueRW.Strength);
        _dimension.SyncValue(ref properties.ValueRW.Dimension);

        if (_rulesToProcess.Any())
        {
            Debug.Log("Rules to process " + _rulesToProcess.Count());
            var first = _rulesToProcess.First();
            var (cell, rules) = first.Value;
            UpdateRules(cell, rules);
            _rulesToProcess.Remove(first.Key);
        }
    }

    private void UpdateRules(sRule cell, IEnumerable<sRule> rules)
    {
        int cellId = int.Parse(cell.Id);
        var nativeRules = new NativeArray<CellRule>(rules.Count(), Allocator.TempJob);
        nativeRules.CopyFrom(rules.Select(r => new CellRule
        {
            Id1 = cellId,
            Id2 = int.TryParse(r.Id, out int otherId) ? otherId : -1,
            Amount = r.Value
        }).ToArray());

        var worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();
        var rulesBuffer = SystemAPI.GetBuffer<CellRule>(worldEntity);
        rulesBuffer.AddRange(nativeRules);


        //var aspect = SystemAPI.GetAspectRW<WorldPropertiesAspect>(worldEntity);
        //aspect.SetRules(cellId, nativeRules);
        //_uiManager.
    }
}
