using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
public partial class ManagedCodeBridge : SystemBase
{
    private UIManager _uiManager;
    private Entity _worldEntity;
    private NativeHashMap<FixedString32Bytes, Entity> _prefabMap;
    private NativeHashMap<FixedString32Bytes, int> _cellLookup;

    protected override void OnStartRunning()
    {
        var worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();
        _uiManager = GameObject.FindObjectOfType<UIManager>();
        World.EntityManager.AddComponentData(worldEntity, new CellRules { Value = new NativeArray<float>(32 * 32, Allocator.Persistent) });
        using var prefabs = SystemAPI.GetBuffer<BakedCellPrefab>(worldEntity).AsNativeArray();
        _prefabMap = new NativeHashMap<FixedString32Bytes, Entity>(prefabs.Length, Allocator.Persistent);
        _cellLookup = new NativeHashMap<FixedString32Bytes, int>(prefabs.Length, Allocator.Persistent);
        int index = 0;
        foreach (var prefab in prefabs)
        {
            var key = prefab.Name;
            _prefabMap.Add(key, prefab.Prefab);
            _cellLookup.Add(key, index++);
        }
    }

    protected override void OnUpdate()
    {
        _worldEntity = SystemAPI.GetSingletonEntity<WorldProperties>();

        if (!_uiManager.IsDirty) { return; }

        if (_uiManager.SettingsLoaded.CheckIfDirtyAndThenClean())
        {
            InitWorld();
        }

        if (_uiManager.Properties.CheckIfDirtyAndThenClean())
        {
            UpdateProperties();
        }

        if (_uiManager.Rules.CheckIfDirtyAndThenClean())
        {
            UpdateRules();
        }

        if (_uiManager.CellCount.CheckIfDirtyAndThenClean())
        {
            UpdateCellCounts();
        }
    }

    private void UpdateProperties()
    {
        var properties = SystemAPI.GetSingletonRW<WorldProperties>();
        var cellRandom = SystemAPI.GetSingletonRW<CellRandom>();
        var configuration = _uiManager.CurrentConfiguration;

        Debug.Log($"configuration {configuration.name} loaded");
        properties.ValueRW.Strength = configuration.Strength;
        properties.ValueRW.Influence = configuration.Influence;
        properties.ValueRW.Scale = configuration.Scale;
        properties.ValueRW.Dimension = _uiManager.Dimension;
        cellRandom.ValueRW.Value = Unity.Mathematics.Random.CreateFromIndex(_uiManager.CurrentConfiguration.RandomSeed);
    }

    private void UpdateCellCounts(bool clearAll = false)
    {
        var cellsBuffer = SystemAPI.GetBuffer<CellConfigurationProperties>(_worldEntity);
        foreach (var cell in _uiManager.CurrentConfiguration?.cells)
        {
            var name = cell.cell.name;
            cellsBuffer.Add(new CellConfigurationProperties()
            {
                Id = _cellLookup[name],
                NumberOfCells = cell.Count,
                Prefab = _prefabMap[name]
            });
        }
        if (!clearAll) return;

        var addedIds = _uiManager.CurrentConfiguration.cells.Select(cell => _cellLookup[cell.cell.name]);
        using var allIds = _cellLookup.GetValueArray(Allocator.Temp);
        var IdsToClear = allIds.Except(addedIds);
        foreach (var Id in IdsToClear)
        {
            cellsBuffer.Add(new CellConfigurationProperties() { Id = Id, NumberOfCells = 0 });
        }
    }

    private void UpdateRules(bool clearAll = false)
    {
        var worldRules = SystemAPI.GetSingletonRW<CellRules>().ValueRW.Value;    
        if (clearAll)
        {
            for (int keyIndex = 0; keyIndex < 32 * 32; keyIndex++)
            {
                worldRules[keyIndex] = 0;
            }
        }
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
        UpdateCellCounts(clearAll: true);
        UpdateRules(clearAll: true);
        UpdateProperties();
    }
}
