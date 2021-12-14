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
        private Dictionary<List<Cell>, int> sectionIndexes;

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

            sectionIndexes = new Dictionary<List<Cell>, int>();

            if(presets.Count == 3)
            {
                Vector2 InitialPoint = GetFirstBounds();
                Vector2 SplitDir = GetSplitDirection();

                GetSections(InitialPoint, SplitDir);
                ApplyIndexesToSections();
            }

            //Return the final result
            return gridChanging;
        }

        public int SectionIndex(Cell cellToCheck)
        {
            if(FirstSection.Contains(cellToCheck))
            {
                return sectionIndexes[FirstSection];
            }
            else if(SecondSection.Contains(cellToCheck)) 
            {
                return sectionIndexes[SecondSection];
            }

            return sectionIndexes[ThirdSection];
        }

        //Retrieving a tile list from a compiler given a x and y for the current grid
        public List<Tile> GetTilesBasedOnCoord(int CellX, int CellY)
        {
            int listToCheck = SectionIndex(gridChanging.GridCells[CellX, CellY]);
            List<Tile> returnList = new List<Tile>();

            for(int i = 0; i < presets[listToCheck].presetTiles.allPossibleTiles.Count; ++i) 
            {
                if(presets[listToCheck].presetTiles.allPossibleTiles[i] != null)
                {
                    returnList.Add(presets[listToCheck].presetTiles.allPossibleTiles[i]);
                }
            }

            return returnList;
        }

            #endregion

            #region Private Methods

            private Vector2 GetFirstBounds()
        {
            Vector2 point = new Vector2();

            //Step, get intial point of split (the bottom right of the first section)
            int lowerBoundX = Mathf.FloorToInt(gridChanging.width * 0.33f);
            int lowerBoundY = Mathf.FloorToInt(gridChanging.height * 0.33f);

            int upperBoundX = Mathf.RoundToInt(gridChanging.width * 0.45f);
            int upperBoundY = Mathf.RoundToInt(gridChanging.height * 0.45f);

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

        private void ApplyIndexesToSections()
        {
            List<List<Cell>> allSections = new List<List<Cell>>();

            //Step, order the sections from biggest to smallest
            //If the first section is bigger than second section
            allSections.Add(FirstSection);
            allSections.Add(SecondSection);
            allSections.Add(ThirdSection);

            //Gets the list from smallest to biggest
            allSections = allSections.OrderBy(x => x.Count).ToList();
            allSections.Reverse();

            for(int i = 0; i < allSections.Count; ++i)
            {
                sectionIndexes.Add(allSections[i], i);
            }
        }

        #endregion
    }
}
