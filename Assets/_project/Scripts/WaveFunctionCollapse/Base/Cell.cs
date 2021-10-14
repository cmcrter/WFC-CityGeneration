////////////////////////////////////////////////////////////
// File: Cell.cs
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 14/10/21
// Brief: A representation of one spot on the 2D Grid
//////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;

namespace WFC
{
    [Serializable]
    public class Cell
    {
        #region Public Fields

        public Tile tileUsed;

        //Each cell will know the possible tiles that could be instantiated here
        public List<Tile> possibleTiles = new List<Tile>();

        #endregion

        #region Public Methods

        public bool isCollapsed()
        {
            return (tileUsed != null);
        }

        public int entropy()
        {
            return possibleTiles.Count;
        }


        public void CollapseCell()
        {
        
        }

        #endregion

        #region Private Methods
        #endregion
    }

}