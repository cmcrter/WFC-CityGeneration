////////////////////////////////////////////////////////////
// File: Cell.cs
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 27/10/21
// Brief: A representation of one spot on the 2D Grid
//////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    [Serializable]
    //This is a class which will hold the data for each cell
    public class Cell
    {
        #region Public Fields

        //The tile used (and also whether the tile has collapsed or not)
        public Tile tileUsed;

        //Each cell will know the possible tiles that could've be instantiated there from the start
        public List<Tile> AllpossibleTiles = new List<Tile>();
        //Each cell will know the possible tiles that could be instantiated there
        public List<Tile> possibleTiles = new List<Tile>();

        public float currentEntropy = 1f;
        public int CellX;
        public int CellY;

        #endregion

        #region Unity Methods
        #endregion

        #region Public Methods

        public Cell()
        {
        }

        public bool isCollapsed()
        {
            return (tileUsed != null);
        }

        public float calculateEntropyValue(InputModel model)
        {
            int sum_of_weights = 0;
            float sum_of_weight_log_weights = 0;
            
            for(int i = 0; i < possibleTiles.Count; ++i)
            {
                int weight = model.FrequenciesOfTiles[possibleTiles[i]];
                sum_of_weights += weight;
                sum_of_weight_log_weights += weight * Mathf.Log(weight);
            }

            return Mathf.Log(sum_of_weights) - (sum_of_weight_log_weights / sum_of_weights);
        }

        //Collapsing the cell by selecting a random tile out of the options to use
        public void CollapseCell(int randNumber, InputModel model)
        {
            //Using relative frequencies to get number to random from and what to do with random number
            //Some of options appear more than other, so increasing the amount of choices for the random to hit those options by the frequency 
            int newRand = randNumber % model.GetSumOfTileWeights(possibleTiles);

            foreach (Tile tile in possibleTiles)
            {
                //Using how often they appear next to the other tiles in the pattern
                int weight = model.FrequenciesOfTiles[tile];

                if(newRand >= weight)
                {
                    newRand -= weight;
                }
                else
                {
                    tileUsed = possibleTiles[possibleTiles.Count];
                }
            }
        }

        public void UpdateConstraints(Cell[] neighbours, InputModel model, out List<Tile> CellConstrainedTiles)
        {
            CellConstrainedTiles = new List<Tile>();

            //Go through the neighbours
            for(int i = 0; i < neighbours.Length; ++i)
            {
                if(i == 0 || i == 2 || i == 5 || i == 7)
                {
                    continue;
                }

                //If a certain tile of the possible tiles never appears next to them in the pattern
                //Remove that possible tile from possible tiles
                if(neighbours[i].tileUsed)
                {
                    for(int j = 0; j < possibleTiles.Count; ++j)
                    {
                        if(neighbours[i].tileUsed)
                        {
                            if(model.IsAdjacentAllowed(possibleTiles[j], neighbours[i].tileUsed, out List<Tile> ConstrainedTiles))
                            {
                                CellConstrainedTiles = ConstrainedTiles;

                                if(ConstrainedTiles.Count > 0)
                                {
                                    UpdatePossibleTiles(ConstrainedTiles);
                                }
                            }
                        }
                        else
                        {
                            for(int k = 0; k < neighbours[i].possibleTiles.Count; ++k)
                            {
                                if(model.IsAdjacentAllowed(possibleTiles[j], neighbours[i].possibleTiles[k], out List<Tile> ConstrainedTiles))
                                {
                                    CellConstrainedTiles = ConstrainedTiles;

                                    if(ConstrainedTiles.Count > 0)
                                    {
                                        UpdatePossibleTiles(ConstrainedTiles);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            currentEntropy = calculateEntropyValue(model);
        }

        public void ApplyConstraintsBasedOnPotential()
        {

        }

        #endregion

        #region Private Methods

        private void UpdatePossibleTiles(List<Tile> tilesToRemove)
        {
            foreach(Tile tile in tilesToRemove)
            {
                possibleTiles.Remove(tile);
            }
        }

        #endregion
    }

}