////////////////////////////////////////////////////////////
// File: Grid.cs
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 14/10/21
// Brief: A representation of a grid of values
//////////////////////////////////////////////////////////// 

using System;

namespace WFC
{
    [Serializable]
    public class Grid
    {
        #region Variables

        //A 2D Array of cells that make up the grid
        public Cell[,] GridCells;
        public int width, height;

        #endregion

        #region Public Methods

        public Grid(int newWidth, int newHeight)
        {
            width = newWidth;
            height = newHeight;

            GridCells = new Cell[width, height];
        }

        #endregion

        #region Private Methods
        #endregion
    }
}