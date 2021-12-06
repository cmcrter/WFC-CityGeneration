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
        private Dictionary<Tile, int> FrequenciesOfTiles;
        private Dictionary<Tile, List<Tile>> AdjacencyRules;

        private bool includeFlipping;

        public int AllTileWeights = 0;
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

            //A way for me to see the adjacency rules
            for(int i = 0; i < tilesUsed.Count; ++i)
            {
                tilesUsed[i].CanGoNextTo = AdjacencyRules[tilesUsed[i]];
            }
        }

        //Checking the tiles to see if they are within eachothers' adjacency rules
        public bool IsAdjacentAllowed(Tile possibleTile, Tile otherTile)
        {
            bool bCanPlace = false;

            if(possibleTile.CanGoNextTo.Contains(otherTile) && otherTile.CanGoNextTo.Contains(possibleTile))
            {
                bCanPlace = true;
            }

            return bCanPlace;
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
            FrequenciesOfTiles = new Dictionary<Tile, int>();

            for(int x = 0; x < Model.height; ++x)
            {
                for(int y = 0; y < Model.width; ++y)
                {
                    if(!FrequenciesOfTiles.ContainsKey(Model.GridCells[x, y].tileUsed))
                    {
                        FrequenciesOfTiles.Add(Model.GridCells[x, y].tileUsed, 1);
                    }
                    else
                    {
                        FrequenciesOfTiles[Model.GridCells[x, y].tileUsed]++;
                    }
                }
            }

            for(int i = 0; i < tilesUsed.Count; ++i)
            {
                tilesUsed[i].Frequency = FrequenciesOfTiles[tilesUsed[i]];
            }
        }

        //Going through and adding up each tiles' weights based on a given list
        public int GetSumOfTileWeights(List<Tile> tilesToWeigh)
        {
            int totalWeight = 0;

            for(int i = 0; i < tilesToWeigh.Count; ++i) 
            {
                totalWeight += tilesToWeigh[i].Frequency;
            }

            return totalWeight;
        }

        //Getting all the tiles' weights to one int
        public int GetSumOfAllTileWeights()
        {
            int totalWeight = 0;

            for(int i = 0; i < tilesUsed.Count; ++i)
            {
                totalWeight += tilesUsed[i].Frequency;
            }

            return totalWeight;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}