using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Merge 13/TileDataSO")]
public class TileDataSO : ScriptableObject
{
    public List<TileData> tileDataList;
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