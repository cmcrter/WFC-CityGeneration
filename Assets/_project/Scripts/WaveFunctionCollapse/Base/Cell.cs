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
    //This is a monobehaviour so it can be easily referred to by editor/inspector scripts
    public class Cell : MonoBehaviour
    {
        #region Public Fields

        //The tile used (and also whether the tile has collapsed or not)
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

        public int calculateEntropyValue()
        {
            return possibleTiles.Count;
        }

        //Collapsing the cell by selecting a random tile out of the options to use
        public void CollapseCell(int randNumber)
        {
            //Using relative frequencies to get number to random from and what to do with random number

            int newRand = randNumber % possibleTiles.Count;

            tileUsed = possibleTiles[newRand];
        }

        //Assuming that cell is not collapsed yet
        public bool isCompatibleWith(Cell otherCell)
        {
 

            return false;
        }

        #endregion

        #region Private Methods
        #endregion
    }

}