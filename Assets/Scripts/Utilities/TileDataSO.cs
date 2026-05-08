using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Merge 13/TileDataSO")]
public class TileDataSO : ScriptableObject
{
    public List<TileData> tileDataList;

    public TileData GetTileData(TileType type)
    {
        return tileDataList.Find(t => t.tileType == type);
    }

    public bool TryGetNextTier(TileType current, out TileData nextData)
    {
        nextData = null;

        // Boom block cannot upgrade
        if (current == TileType.Boom_Block)
            return false;

        TileType nextType = current + 1;

        nextData = GetTileData(nextType);

        return nextData != null;
    }
}

[Serializable]
public class TileData
{
    public TileType tileType;
    public Sprite sprite;
}

public enum TileType
{
    Star_1,
    Star_2,
    Star_3,
    Star_4,
    Star_5,
    Star_6,
    Boom_Block
}