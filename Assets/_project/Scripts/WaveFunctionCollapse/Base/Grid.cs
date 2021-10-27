////////////////////////////////////////////////////////////
// File: Grid.cs
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 27/10/21
// Brief: A representation of a grid of values
//////////////////////////////////////////////////////////// 

using System;
using UnityEngine;

namespace WFC
{
    [Serializable]
    public class Grid
    {
        #region Variables

        //A 2D Array of cells that make up the grid
        public Cell[,] GridCells;

        //The dimensions of the grid
        public int width, height;

        #endregion

        #region Public Methods

        public Grid(int newWidth, int newHeight)
        {
            width = newWidth;
            height = newHeight;

            GridCells = new Cell[width, height];
        }

        //To be used in conjunction with the DIRECTION enum
        public Cell[] GetNeighbours(int CellX, int CellY)
        {
            //Getting the moore neighbourhood
            Cell[] neighbours = new Cell[4];
            int neighbourCount = 0;

            for(int i = -1; i <= 1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    // skip center cell
                    if(i == j)
                        continue;

                    // skip rows out of range.
                    if((i + CellX) < 0 || (i + CellX >= height))
                        continue;

                    // skip columns out of range.
                    if((j + CellY) < 0 || (j + CellY >= width))
                        continue;

                    //This is a viable neighbour
                    if(neighbourCount != neighbours.Length) 
                    {
                        neighbours[neighbourCount] = GridCells[i + CellX, j + CellY];
                        neighbourCount++;
                    }
                }
            }

            return neighbours;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}