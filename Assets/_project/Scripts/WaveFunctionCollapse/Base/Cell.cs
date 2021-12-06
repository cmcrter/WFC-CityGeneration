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
using WFC.Rand;

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

        //Calculating Shannon's Entropy
        public float calculateEntropyValue(InputModel model)
        {
            float sum_of_weights = 0;
            float sum_of_weight_log_weights = 0;
            float weightSum = model.GetSumOfTileWeights(possibleTiles);

            for(int i = 0; i < possibleTiles.Count; ++i)
            {
                float weight = possibleTiles[i].Frequency / weightSum;

                sum_of_weights += weight;
                sum_of_weight_log_weights += weight * Mathf.Log(weight);
            }

            return (Mathf.Log(sum_of_weights) - (sum_of_weight_log_weights / sum_of_weights));
        }

        //Collapsing the cell by selecting a random tile out of the options to use (passing through the number generator so there's more randomness)
        public bool CollapseCell(Mersenne_Twister twister, InputModel model)
        {
            //Using relative frequencies to get number to random from and what to do with random number
            //Some of options appear more than other, so increasing the amount of choices for the random to hit those options by the frequency 
            if(possibleTiles.Count == 0)
            {
                Debug.Log("No possible tiles when collapsing");
                return false;
            }

            //Going through and getting all the weights of the possible tiles
            int allTileWeights = model.GetSumOfTileWeights(possibleTiles);
            //Getting a positive number between 0 and the sum of the weights
            int newRand = Mathf.Abs(twister.ReturnRandom()) % allTileWeights;
            int tileIndex = 0;

            //Going through the possible tiles and removing the weights until the correct one is chosen
            foreach (Tile tile in possibleTiles)
            {
                //Using how often they appear next to the other tiles in the pattern
                int weight = tile.Frequency;

                if(newRand >= weight)
                {
                    newRand -= weight;
                    tileIndex++;
                }
                else
                {
                    //This is the tile currently used
                    tileUsed = possibleTiles[tileIndex];

                    possibleTiles.Clear();
                    possibleTiles.Add(tileUsed);
                    return true;
                }
            }

            return false;
        }

        public void UpdateConstraints(Cell[] neighbours, InputModel model)
        {
            List<Tile> CellConstrainedTiles = new List<Tile>();

            //Go through the neighbours (this array doesn't include the current cell)
            for(int i = 0; i < neighbours.Length; ++i)
            {
                if(i == 0 || i == 2 || i == 5 || i == 7)
                {
                    continue;
                }

                //If a certain tile of the possible tiles never appears next to them in the pattern
                //Remove that possible tile from possible tiles
                if(neighbours[i] != null)
                {
                    for(int j = 0; j < possibleTiles.Count; ++j)
                    {
                        if(neighbours[i].tileUsed)
                        {
                            if(!model.IsAdjacentAllowed(possibleTiles[j], neighbours[i].tileUsed) && !CellConstrainedTiles.Contains(neighbours[i].tileUsed))
                            {
                                CellConstrainedTiles.Add(neighbours[i].tileUsed);
                            }
                        }
                        else
                        {
                            //Checking against the neighbours' possible tiles
                            for(int k = 0; k < neighbours[i].possibleTiles.Count; ++k)
                            {
                                if(!model.IsAdjacentAllowed(possibleTiles[j], neighbours[i].possibleTiles[k]) && !CellConstrainedTiles.Contains(neighbours[i].possibleTiles[k]))
                                {
                                    CellConstrainedTiles.Add(neighbours[i].possibleTiles[k]);
                                }
                            }
                        }
                    }
                }
            }

            UpdatePossibleTiles(CellConstrainedTiles);

            if(possibleTiles.Count > 0)
            {
                currentEntropy = calculateEntropyValue(model);
            }
            else
            {
                currentEntropy = 0;
            }
        }

        public bool ApplyConstraintsBasedOnPotential(Grid gridCellisIn, InputModel model)
        {
            List<Cell> neighbours = gridCellisIn.GetNeighbours(CellX, CellY);
            List<Tile> impossibleTiles = new List<Tile>();

            bool tileRemoved = false;            

            //Go through the neighbours (this array doesn't include the current cell)
            for(int i = 0; i < neighbours.Count; ++i)
            {
                //If a certain tile of the possible tiles never appears next to them in the pattern
                //Remove that possible tile from possible tiles
                if(neighbours[i] != null)
                {
                    for(int j = 0; j < possibleTiles.Count; ++j)
                    {
                        if(neighbours[i].tileUsed)
                        {
                            if(!model.IsAdjacentAllowed(possibleTiles[j], neighbours[i].tileUsed))
                            {
                                impossibleTiles.Add(possibleTiles[j]);
                                tileRemoved = true;
                            }
                        }
                    }
                }
            }

            UpdatePossibleTiles(impossibleTiles);
            currentEntropy = calculateEntropyValue(model);

            return tileRemoved;
        }

        #endregion

        #region Private Methods

        private void UpdatePossibleTiles(List<Tile> tilesToRemove)
        {
            foreach(Tile tile in tilesToRemove)
            {
                if(possibleTiles.Contains(tile))
                {
                    possibleTiles.Remove(tile);
                }
            }

            if(tilesToRemove.Count > 2)
            {
                Debug.Log("More than 2 tiles being removed");
            }
        }

        #endregion
    }

}