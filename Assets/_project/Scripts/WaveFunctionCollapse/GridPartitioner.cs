////////////////////////////////////////////////////////////
// File: GridPartitioner.cs
// Author: Charles Carter
// Date Created: 12/21/21
// Last Edited By: Charles Carter
// Date Last Edited: 12/21/21
// Brief: A script which takes a grid and input model compilers, and assigns sections of the grid to a compiler
//////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WFC.Editor;
using WFC.Rand;
using WFC.UI;

namespace WFC
{
    public class GridPartitioner
    {
        #region Variables

        //Necessary Variables
        private Grid gridChanging;
        private List<TilePreset> presets = new List<TilePreset>();
        private Mersenne_Twister twister;

        //Sectioning setup
        private List<Cell> FirstSection = new List<Cell>();
        private List<Cell> SecondSection = new List<Cell>();
        private List<Cell> ThirdSection = new List<Cell>();
        private List<List<Cell>> SectionsFromBiggestToSmallest = new List<List<Cell>>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Constructors
        /// </summary>
        public GridPartitioner()
        {
            //Empty constructor for local variables
        }

        public GridPartitioner(Grid gridInputted, List<TilePreset> presetsToUse, Mersenne_Twister mT)
        {
            gridChanging = gridInputted;
            presets = presetsToUse;
            twister = mT;
        }

        /// <summary>
        /// A function to split a grid into 3 parts and assign each part a tile type
        /// </summary>
        public Grid RunPartition()
        {
            //Guard clause to make sure there's something to use when partitioning
            if(gridChanging == null || presets.Count == 0)
                return null;

            if(presets.Count == 3)
            {
                Vector2 InitialPoint = GetFirstBounds();
                Vector2 SplitDir = GetSplitDirection();

                GetSections(InitialPoint, SplitDir);
                OrderSectionsBySize();
                AssignTilesToSections();
            }

            //Return the final result
            return gridChanging;
        }

        public int SectionIndex(Cell cellToCheck)
        {
            if(FirstSection.Contains(cellToCheck))
            {
                return 0;
            }
            else if(SecondSection.Contains(cellToCheck)) 
            {
                return 1;
            }

            return 2;
        }

        #endregion

        #region Private Methods

        private Vector2 GetFirstBounds()
        {
            Vector2 point = new Vector2();

            //Step, get intial point of split (the bottom right of the first section)
            int lowerBoundX = Mathf.FloorToInt(gridChanging.width * 0.25f);
            int lowerBoundY = Mathf.FloorToInt(gridChanging.height * 0.25f);

            int upperBoundX = Mathf.RoundToInt(gridChanging.width * 0.33f);
            int upperBoundY = Mathf.RoundToInt(gridChanging.height * 0.33f);

            point.x = twister.ReturnRandom(lowerBoundX, upperBoundX);
            point.y = twister.ReturnRandom(lowerBoundY, upperBoundY);     

            return point;
        }

        //Getting which way the second sector is from
        private Vector2 GetSplitDirection()
        {
            //To do the next step, the neighbours will be needed
            Vector2 direction = new Vector2();

            //Step, pick direction to go based on width/height.. and the next cell in that direction is the bottom left or top right of that sector
            //Width is bigger than height
            if(gridChanging.width > gridChanging.height)
            {
                direction = new Vector2(1, 0);
            }
            //Height is bigger than width
            else if(gridChanging.height > gridChanging.width)
            {
                direction = new Vector2(1, 0);
            }
            //Square grid
            else
            {
                //Randomly getting a direction
                bool up = twister.ReturnRandom(0, 1) == 1 ? true : false;

                if(up)
                {
                    direction = new Vector2(0, 1);
                }
                else
                {
                    direction = new Vector2(1, 0);
                }
            }

            return direction;
        }

        private void GetSections(Vector2 firstPoint, Vector2 direction)
        {
            //Step, go through the grid and get lists of each the areas
            for(int y = 0; y < gridChanging.height; y++)
            {
                for(int x = 0; x < gridChanging.width; x++)
                {
                    //If it's in the first box
                    if(x <= firstPoint.x && y <= firstPoint.y)
                    {
                        FirstSection.Add(gridChanging.GridCells[x, y]);
                    }
                    //or in the second
                    else if(x > firstPoint.x && direction.x > 0 || y > firstPoint.y && direction.y > 0)
                    {
                        SecondSection.Add(gridChanging.GridCells[x, y]);
                    }
                    //or in the remaining
                    else
                    {
                        ThirdSection.Add(gridChanging.GridCells[x, y]);
                    }
                }
            }
        }

        private void OrderSectionsBySize()
        {
            //Step, order the sections from biggest to smallest
            //If the first section is bigger than second section
            SectionsFromBiggestToSmallest.Add(FirstSection);
            SectionsFromBiggestToSmallest.Add(SecondSection);
            SectionsFromBiggestToSmallest.Add(ThirdSection);

            //Gets the list from smallest to biggest
            SectionsFromBiggestToSmallest = SectionsFromBiggestToSmallest.OrderBy(x => x.Count).ToList();

            //Reversing it
            SectionsFromBiggestToSmallest.Reverse();
        }

        private void AssignTilesToSections()
        {
            //Step, assign the cells in the biggest area to park, the next biggest to residential and the smallest to business
            for(int i = 0; i < SectionsFromBiggestToSmallest.Count; ++i)
            {
                for(int j = 0; j < SectionsFromBiggestToSmallest[i].Count; ++j)
                {
                    //The order the presets are put through should be the order we want (park being first etc)
                    SectionsFromBiggestToSmallest[i][j].possibleTiles = presets[i].presetTiles.allPossibleTiles;
                }
            }
        }

        #endregion
    }
}
