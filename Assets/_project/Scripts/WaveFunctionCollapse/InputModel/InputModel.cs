////////////////////////////////////////////////////////////
// File: InputModel
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 03/12/21
// Brief: The script which holds information about the input model and can generate the patterns
//////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    [Serializable]
    public class InputModel
    {
        #region Variables

        public Grid Model;
        public List<Tile> tilesUsed;

        //Linked Dictionaries for the adjacency rule and the frenquencies of tiles in the input model
        public Dictionary<Tile, int> FrequenciesOfTiles => TileFreq;
        private Dictionary<Tile, int> TileFreq;

        public Dictionary<Tile, List<Tile>> AllAdjacencyRules => AdjacencyRules;
        private Dictionary<Tile, List<Tile>> AdjacencyRules;

        private bool includeFlipping;

        #endregion

        #region Public Methods

        public InputModel(Grid modelGrid, bool flipping)
        {
            Model = modelGrid;
            includeFlipping = flipping;
        }

        public void GenerateAdjacencyRules()
        {
            AdjacencyRules = new Dictionary<Tile, List<Tile>>();

            //Going through and seeing what tiles are next to what based on the input model
            for(int x = 0; x < Model.height; ++x)
            {
                for(int y = 0; y < Model.width; ++y) 
                {
                    List<Cell> neighbours = Model.GetNeighbours(x, y);

                    if(!AdjacencyRules.ContainsKey(Model.GridCells[x, y].tileUsed))
                    {
                        AdjacencyRules.Add(Model.GridCells[x, y].tileUsed, new List<Tile>());
                    }

                    for (int i = 0; i < neighbours.Count; ++i)
                    {
                        if(neighbours[i] != null)
                        {
                            if(!AdjacencyRules[Model.GridCells[x, y].tileUsed].Contains(neighbours[i].tileUsed))
                            {
                                AdjacencyRules[Model.GridCells[x, y].tileUsed].Add(neighbours[i].tileUsed);
                            }
                        }
                    }
                }
            }

        }

        //Goes through the current grid and adds each unique tile type to the overall tiles used list
        public void GenerateListOfPotentialTiles()
        {
            tilesUsed = new List<Tile>();

            for(int x = 0; x < Model.height; x++)
            {
                for(int y = 0; y < Model.width; y++)
                {
                    if(!tilesUsed.Contains(Model.GridCells[x, y].tileUsed))
                    {
                        tilesUsed.Add(Model.GridCells[x, y].tileUsed);
                    }
                }
            }
        }

        //Goes through the current grid and counts the amount of times each tile appears, then sets that tiles' scriptable object to reflect that
        public void CalculateRelativeFrequency()
        {
            TileFreq = new Dictionary<Tile, int>();

            for(int x = 0; x < Model.height; ++x)
            {
                for(int y = 0; y < Model.width; ++y)
                {
                    if(!FrequenciesOfTiles.ContainsKey(Model.GridCells[x, y].tileUsed))
                    {
                        TileFreq.Add(Model.GridCells[x, y].tileUsed, 1);
                    }
                    else
                    {
                        TileFreq[Model.GridCells[x, y].tileUsed]++;
                    }
                }
            }
        }

        #endregion

        #region Private Methods
        #endregion
    }
}