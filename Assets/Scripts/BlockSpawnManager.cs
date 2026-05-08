using System.Collections.Generic;
using UnityEngine;

public class BlockSpawnManager : MonoBehaviour
{
    public static BlockSpawnManager Instance { get; private set; }

    [SerializeField] private Block blockPrefab;
    [SerializeField] private TileDataSO tileDataSO;
    [SerializeField] private Transform[] spawnSlots; // 3 slots in BottomPanel

    private Block[] currentBlocks;
    private int spawnCount = 0;

    void Awake()
    {
        Instance = this;
        currentBlocks = new Block[spawnSlots.Length];
    }

    void Start()
{
    Debug.Log($"SpawnSlots count: {spawnSlots.Length}, BlockPrefab: {blockPrefab}, TileData: {tileDataSO}");
    SpawnAllSlots();
}
    // Called when ALL slots are empty — refill all at once
    public void TryRefillSlots()
    {
        bool allEmpty = true;
        foreach (var b in currentBlocks)
        {
            if (b != null) { allEmpty = false; break; }
        }

        if (allEmpty) SpawnAllSlots();
    }

    private void SpawnAllSlots()
{
    if (spawnSlots == null || spawnSlots.Length == 0)
    {
        Debug.LogError("SpawnSlots are not assigned in BlockSpawnManager!");
        return;
    }

    for (int i = 0; i < spawnSlots.Length; i++)
        SpawnAtSlot(i);
}

    private void SpawnAtSlot(int index)
    {
        if (currentBlocks[index] != null)
            Destroy(currentBlocks[index].gameObject);

        spawnCount++;

        int count = Random.Range(1, 3); // 1 or 2 tiles per block
        var selected = new List<TileData>();

        for (int i = 0; i < count; i++)
            selected.Add(GetWeightedRandomTile());

        Block block = Instantiate(blockPrefab, spawnSlots[index]);
        block.Initialize(selected);
        currentBlocks[index] = block;
    }

    // Called by GridController after a block is placed
    public void OnBlockPlaced(Block block)
    {
        for (int i = 0; i < currentBlocks.Length; i++)
        {
            if (currentBlocks[i] == block)
            {
                currentBlocks[i] = null;
                break;
            }
        }

        TryRefillSlots();
    }

    // Keep SpawnBlock for compatibility but now spawns all
    public void SpawnBlock() => TryRefillSlots();

    // -------------------------------------------------------
    // Weighted Spawn
    // -------------------------------------------------------

    private TileData GetWeightedRandomTile()
    {
        List<(TileData data, float weight)> pool = BuildWeightedPool();

        float totalWeight = 0f;
        foreach (var (_, w) in pool) totalWeight += w;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var (data, weight) in pool)
        {
            cumulative += weight;
            if (roll <= cumulative) return data;
        }

        return pool[0].data;
    }

    private List<(TileData data, float weight)> BuildWeightedPool()
    {
        var pool = new List<(TileData, float)>();

        foreach (TileData tileData in tileDataSO.tileDataList)
        {
            if (tileData.tileType == TileType.Boom_Block) continue;

            float weight = GetTileWeight(tileData.tileType);
            if (weight > 0f) pool.Add((tileData, weight));
        }

        return pool;
    }

    private float GetTileWeight(TileType type)
    {
        switch (type)
        {
            case TileType.Star_1:
                if      (spawnCount <= 20) return 60f;
                else if (spawnCount <= 50) return 45f;
                else if (spawnCount <= 90) return 30f;
                else                       return 20f;

            case TileType.Star_2:
                if      (spawnCount <= 20) return 30f;
                else if (spawnCount <= 50) return 35f;
                else if (spawnCount <= 90) return 30f;
                else                       return 25f;

            case TileType.Star_3:
                if      (spawnCount <= 20) return 10f;
                else if (spawnCount <= 50) return 25f;
                else if (spawnCount <= 90) return 25f;
                else                       return 20f;

            case TileType.Star_4:
                if      (spawnCount <= 20) return 0f;
                else if (spawnCount <= 50) return 10f;
                else if (spawnCount <= 90) return 20f;
                else                       return 18f;

            case TileType.Star_5:
                if      (spawnCount <= 50) return 0f;
                else if (spawnCount <= 90) return 8f;
                else                       return 12f;

            case TileType.Star_6:
                if      (spawnCount <= 90) return 0f;
                else                       return 5f;

            default: return 0f;
        }
    }
}