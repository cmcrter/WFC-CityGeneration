////////////////////////////////////////////////////////////
// File: Tile.cs
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 27/10/21
// Brief: A tile that can be placed down
//////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    [SerializeField]
    [CreateAssetMenu(fileName = "Tile", menuName = "ScriptableObjects/TileObject", order = 1)]
    public class Tile : ScriptableObject
    {
        //This is the actual model that can be put down as the tile
        public GameObject Prefab;
        public List<Tile> CanGoNextTo;
    }
}
