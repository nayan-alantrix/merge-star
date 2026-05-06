using System.Collections.Generic;
using UnityEngine;

public class BlockSpawnManager : MonoBehaviour
{
    public static BlockSpawnManager Instance { get; private set; }

    [SerializeField] private Block blockPrefab;
    [SerializeField] private TileDataSO tileDataSO;
    [SerializeField] private int minTiles = 1;
    [SerializeField] private int maxTiles = 2;

    private Block currentBlock;

    void Awake() => Instance = this;
    void Start() => SpawnBlock();

    public void SpawnBlock()
    {
        if (currentBlock != null)
            Destroy(currentBlock.gameObject);

        int count = Random.Range(minTiles, maxTiles + 1);
        var selected = new List<TileData>();

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, tileDataSO.tileDataList.Count);
            selected.Add(tileDataSO.tileDataList[idx]);
        }

        currentBlock = Instantiate(blockPrefab, transform);
        currentBlock.Initialize(selected);
    }
}