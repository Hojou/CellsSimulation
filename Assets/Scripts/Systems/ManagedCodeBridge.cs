using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using sRule = CellPropertySettings.Rule;
using sConfig = CellPropertySettings.CellConfig;
using Unity.VisualScripting;
using Sirenix.Utilities;
using static UIManager;

public partial class ManagedCodeBridge : SystemBase
{
    private UIManager _uiManager;

    private readonly SyncableValueHolder<float> _speed = new SyncableValueHolder<float>(1);
    private readonly SyncableValueHolder<float> _strength = new SyncableValueHolder<float>(1);
    private readonly SyncableValueHolder<float> _scale = new SyncableValueHolder<float>(1);
    private readonly SyncableValueHolder<float> _influence = new SyncableValueHolder<float>(1);
    private readonly SyncableValueHolder<float2> _dimension = new SyncableValueHolder<float2>(new float2(5, 5));
    private readonly SyncableValueHolder<bool> _loaded = new SyncableValueHolder<bool>(false);
    //private readonly SyncableValueHolder<uint> _seed = new SyncableValueHolder<uint>(0);
    //private readonly Dictionary<string, Tuple<sRule, IEnumerable<sRule>>> _rulesToProcess = new Dictionary<string, Tuple<sRule, IEnumerable<sRule>>>();

    private NativeHashMap<FixedString32Bytes, Entity> _prefabMap;
    private NativeHashMap<FixedString32Bytes, int> _cellLookup;

    //EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate()
    {
        //UnityEngine.Debug.Log("OnCreate");
        RequireForUpdate<WorldProperties>();
        _uiManager = GameObject.FindObjectOfType<UIManager>();
        _uiManager.onSpeedChanged += _speed.SetValue;
        _uiManager.onStrengthChanged += _strength.SetValue;
        _uiManager.onDimensionChanged += _dimension.SetValue;
        _uiManager.onInfluenceChanged += _influence.SetValue;
        _uiManager.onSettingsLoaded += () => { UnityEngine.Debug.Log("Action triggered loaded"); _loaded.SetValue(true); };
        //_uiManager.onRuleChanged += (cell, rules) => _rulesToProcess.TryAdd(cell.Id, new Tuple<sRule, IEnumerable<sRule>>(cell, rules));
    }

    protected override void OnStartRunning()
    {
        //UnityEngine.Debug.Log("InitializeCellConfigurations");
        //InitializeCellConfigurations();
        var worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();
        using var prefabs = SystemAPI.GetBuffer<BakedCellPrefab>(worldEntity).AsNativeArray();
        _prefabMap = new NativeHashMap<FixedString32Bytes, Entity>(prefabs.Length, Allocator.Persistent);
        _cellLookup = new NativeHashMap<FixedString32Bytes, int>(prefabs.Length, Allocator.Persistent);
        int index = 0;
        foreach (var prefab in prefabs)
        {
            _prefabMap.Add(prefab.Name, prefab.Prefab);
            _cellLookup.Add(prefab.Name, index++);
        }

    }

    //private void InitializeCellConfigurations()
    //{
    //    //UnityEngine.Debug.Log("Initialize");
    //    _rulesToProcess.Clear();

    //    var entity = SystemAPI.GetSingletonEntity<WorldProperties>();
    //    var world = SystemAPI.GetAspectRO<WorldPropertiesAspect>(entity);

    //    var cellConfigurations = world.CellConfigurations;
    //    using var rules = world.CellRules;

    //    foreach (var config in cellConfigurations)
    //    {
    //        var cellConfig = new sConfig
    //        {
    //            //Id = config.Id.ToString(),
    //            Name = config.Name.ToString(),
    //            Count = config.NumberOfCells
    //        };

    //        var cellRules = new List<sRule>();
    //        foreach (var rule in rules)
    //        {
    //            if (rule.Id1 != config.Id) continue;
    //            var vsName = cellConfigurations.Single(r => r.Id == rule.Id2).Name;
    //            //UnityEngine.Debug.Log($"Rule Id{rule.Id1}: vs {rule.Id2}({vsName}), value:{rule.Amount}");
    //            cellRules.Add(new sRule
    //            {
    //                Id = rule.Id2.ToString(),
    //                Label = $"vs {vsName}",
    //                Value = rule.Amount
    //            });
    //        }

    //        _uiManager.AddCellConfig(cellConfig, cellRules);
    //}
    //}

    protected override void OnUpdate()
    {
        bool dummy = false;
        if (_loaded.SyncValue(ref dummy))
        {
            InitWorld();
        }

        var properties = SystemAPI.GetSingletonRW<WorldProperties>();
        _speed.SyncValue(ref properties.ValueRW.Speed);
        _strength.SyncValue(ref properties.ValueRW.Strength);
        _dimension.SyncValue(ref properties.ValueRW.Dimension);
        _influence.SyncValue(ref properties.ValueRW.Influence);

        if (_uiManager.Rules.IsDirty)
        {
            _uiManager.Rules.SyncValue();
            UpdateRules();
        }

        if (_uiManager.CellCount.IsDirty)
        {
            _uiManager.CellCount.SyncValue();
            UpdateCellCounts();
        }
    }

    private void UpdateCellCounts()
    {
        var worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();
        var cellsBuffer = SystemAPI.GetBuffer<CellConfigurationProperties>(worldEntity);
        foreach (var cell in _uiManager.CurrentConfiguration.cells)
        {
            var name = cell.cell.name;
            cellsBuffer.Add(new CellConfigurationProperties()
            {
                Id = _cellLookup[name],
                NumberOfCells = cell.Count,
                Prefab = _prefabMap[name]
            });
        }
    }

    private void UpdateRules()
    {
        var worldProperties = SystemAPI.GetSingletonRW<WorldProperties>();
        var worldRules = worldProperties.ValueRW.Rules;
        var configurationRules = _uiManager.CurrentConfiguration.rules;
        foreach (var rule in configurationRules)
        {
            var Id1 = _cellLookup[rule.Cell1.name];
            var Id2 = _cellLookup[rule.Cell2.name];
            int keyIndex = 32 * Id1 + Id2;
            worldRules[keyIndex] = rule.Amount;
        }
    }

    private void InitWorld()
    {
        UnityEngine.Debug.Log("INIT WORLD");

        UpdateCellCounts();

        UpdateRules();

        var cellRandom = SystemAPI.GetSingletonRW<CellRandom>();
        cellRandom.ValueRW.Value = Unity.Mathematics.Random.CreateFromIndex(_uiManager.CurrentConfiguration.RandomSeed);

        var worldProperties = SystemAPI.GetSingletonRW<WorldProperties>();
        var simulation = _uiManager.CurrentConfiguration;
        worldProperties.ValueRW.Speed = simulation.Speed;
        worldProperties.ValueRW.Strength = simulation.Strength;
        worldProperties.ValueRW.Dimension = _dimension.SyncValue();
        worldProperties.ValueRW.Scale = simulation.Scale;
        worldProperties.ValueRW.Influence = simulation.Influence;
    }

    private void UpdateRulesx()
    {
        //int cellId = int.Parse(cellRule.Id);

        // 1) Add into to CellsBuffer on how many cells
        // 2) Modify directly on WorldProperties.Rules the new rules
        // 3) Use this from UpdateAllRules()    



        //var worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();
        //var cellsBuffer = SystemAPI.GetBuffer<CellConfigurationProperties>(worldEntity);

        //var config = _uiManager.CurrentConfiguration;
        //var configurationRules = config.rules;
        //var index = 0;
        //var cellIdLookup = new NativeHashMap<FixedString32Bytes, int>(32, Allocator.TempJob);
        //foreach (var cell in config.cells)
        //{
        //    var name = cell.cell.name;
        //    cellIdLookup.Add(name, index);
        //    cellsBuffer.Add(new CellConfigurationProperties()
        //    {
        //        Id = cellId,
        //        NumberOfCells = cellRule.Value,
        //        Prefab = _prefabMap[name]
        //    });
        //    index++;
        //}
    }
}
