////////////////////////////////////////////////////////////
// File: InputModel
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 21/10/21
// Brief: The script which holds information about the input model and can generate the patterns
//////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;

namespace WFC
{
    [Serializable]
    public class InputModel
    {
        #region Variables

        public Grid Model;
        public List<Tile> tilesUsed = new List<Tile>();

        //Linked Dictionaries
        public Dictionary<Tile, int> FrequenciesOfTiles = new Dictionary<Tile, int>();
        public Dictionary<int, Tile> Frequencies = new Dictionary<int, Tile>();

        public Dictionary<Tile, List<Tile>> AdjacencyRules = new Dictionary<Tile, List<Tile>>();

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
            for(int x = 0; x < Model.height; ++x)
            {
                for(int y = 0; y < Model.width; ++y) 
                {
                    Cell[] neighbours = Model.GetNeighbours(x, y);

                    if(!AdjacencyRules.ContainsKey(Model.GridCells[x, y].tileUsed))
                    {
                        AdjacencyRules.Add(Model.GridCells[x, y].tileUsed, new List<Tile>());
                    }

                    for (int i = 0; i < neighbours.Length; ++i)
                    {
                        if(!AdjacencyRules[Model.GridCells[x, y].tileUsed].Contains(neighbours[i].tileUsed))
                        {
                            AdjacencyRules[Model.GridCells[x, y].tileUsed].Add(neighbours[i].tileUsed);
                        }
                    }
                }
            }
        }

        public bool IsAdjacentAllowed(Tile possibleTile, Tile otherTile, out List<Tile> tilesToRemove)
        {
            tilesToRemove = new List<Tile>();
            bool bCanPlace = false;

            if(AdjacencyRules.TryGetValue(possibleTile, out List<Tile> values))
            {
                for(int i = 0; i < AdjacencyRules[possibleTile].Count; ++i)
                {
                    if(AdjacencyRules[possibleTile].Contains(otherTile))
                    {
                        bCanPlace = true;
                    }
                    else
                    {
                        tilesToRemove.Add(AdjacencyRules[possibleTile][i]);
                    }
                }
            }

            return bCanPlace;
        }


        public void GenerateListOfPotentialTiles()
        {
            for(int x = 0; x < Model.height; ++x)
            {
                for(int y = 0; y < Model.width; ++y)
                {
                    if(!tilesUsed.Contains(Model.GridCells[x, y].tileUsed))
                    {
                        tilesUsed.Add(Model.GridCells[x, y].tileUsed);
                        FrequenciesOfTiles.Add(Model.GridCells[x, y].tileUsed, 1);
                        Frequencies.Add(1, Model.GridCells[x, y].tileUsed);
                    }
                }
            }
        }

        public void CalculateRelativeFrequency()
        {
            for(int x = 0; x < Model.height; ++x)
            {
                for(int y = 0; y < Model.width; ++y)
                {
                    FrequenciesOfTiles.TryGetValue(Model.GridCells[x, y].tileUsed, out int currentFreq);
                    currentFreq++;
                    FrequenciesOfTiles[Model.GridCells[x, y].tileUsed] = currentFreq;
                }
            }
        }

        public int GetSumOfTileWeights(List<Tile> tilesToWeigh)
        {
            int totalWeight = 0;

            for(int i = 0; i < tilesToWeigh.Count; ++i) 
            {
                totalWeight += FrequenciesOfTiles[Frequencies[i]];
            }

            return totalWeight;
        }

        #endregion

        #region Private Methods
        #endregion
    }

}