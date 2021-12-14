////////////////////////////////////////////////////////////
// File: Tile.cs
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 08/12/21
// Brief: A tile that can be placed down
//////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    [Serializable]
    public struct AdjacencyRule
    {
        //Adjacency rules just need to know the other tile and where they can be placed
        public AdjacencyRule(Tile newTile, Vector2 newDir)
        {
            tile = newTile;
            direction = newDir;
        }

        public Tile tile;
        public Vector2 direction;
    }

    [SerializeField]
    [CreateAssetMenu(fileName = "Tile", menuName = "ScriptableObjects/TileObject", order = 1)]
    public class Tile : ScriptableObject
    {
        //This is the actual model that can be put down as the tile
        public GameObject Prefab;
        public List<AdjacencyRule> CanGoNextTo;
        public int Frequency;
    }
}
