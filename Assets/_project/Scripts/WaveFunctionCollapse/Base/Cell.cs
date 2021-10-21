////////////////////////////////////////////////////////////
// File: Cell.cs
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 21/10/21
// Brief: A representation of one spot on the 2D Grid
//////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public class Cell : MonoBehaviour
    {
        #region Public Fields

        //The tile used
        public Tile tileUsed;

        //Each cell will know the possible tiles that could be instantiated there
        public List<Tile> possibleTiles = new List<Tile>();

        #endregion

        #region Unity Methods
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

        //Collapsing a random cell
        public void CollapseCell(int randNumber)
        {
            int newRand = randNumber % possibleTiles.Count;

            tileUsed = possibleTiles[newRand];
        }

        #endregion

        #region Private Methods
        #endregion
    }

}