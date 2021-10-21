////////////////////////////////////////////////////////////
// File: Pattern
// Author: Charles Carter
// Date Created: 21/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 21/10/21
// Brief: The script which contains information of a specific pattern
//////////////////////////////////////////////////////////// 

using System.Collections.Generic;

namespace WFC
{
    public class Pattern
    {
        private List<Tile> tilesUsed = new List<Tile>();

        public void AddTile(Tile tile)
        {
            tilesUsed.Add(tile);
        }

        public List<Tile> GetTiles()
        {
            return tilesUsed;
        }
    }
}