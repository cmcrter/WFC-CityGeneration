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

            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    GridCells[x, y] = new Cell();
                }
            }

        }

        //To be used in conjunction with the DIRECTION enum
        public Cell[] GetNeighbours(int CellX, int CellY)
        {
            //Getting the van Neumann neighbourhood
            Cell[] neighbours = new Cell[4];
            int neighbourCount = 0;

            for(int i = -1; i <= 1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    //A guard clause for several conditions wherein it's not a valid neighbour
                    // skip center cell, skip rows out of range, skip columns out of range
                    if(i == j || (i + CellX) < 0 || (i + CellX >= height) || (j + CellY) < 0 || (j + CellY >= width))
                    {
                        continue;
                    }

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