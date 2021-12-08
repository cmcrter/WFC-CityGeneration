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
        public Dictionary<Tile, int> FrequenciesOfTiles;
        public Dictionary<Tile, List<AdjacencyRule>> AllAdjacencyRules;

        #endregion

        #region Public Methods

        public InputModel(Grid modelGrid)
        {
            Model = modelGrid;
        }

        public void GenerateAdjacencyRules()
        {
            AllAdjacencyRules = new Dictionary<Tile, List<AdjacencyRule>>();

            //Going through and seeing what tiles are next to what based on the input model
            for(int x = 0; x < Model.height; ++x)
            {
                for(int y = 0; y < Model.width; ++y) 
                {
                    List<Cell> neighbours = Model.GetNeighbours(x, y).Item1;
                    List<Vector2> neighboursDirections = Model.GetNeighbours(x, y).Item2;

                    for(int i = 0; i < neighbours.Count; ++i)
                    {
                        Vector2 direction = neighboursDirections[i];

                        if(direction.x != 0 && direction.y != 0)
                        {
                            Debug.Log("Incorrect Rule");
                            continue;
                        }

                        if(neighbours[i] != null)
                        {
                            AdjacencyRule rule = new AdjacencyRule(neighbours[i].tileUsed, direction);

                            if(!AllAdjacencyRules.ContainsKey(Model.GridCells[x, y].tileUsed))
                            {
                                List<AdjacencyRule> list = new List<AdjacencyRule>();
                                list.Add(rule);
                                AllAdjacencyRules.Add(Model.GridCells[x, y].tileUsed, list);
                            }
                            else
                            {
                                if(!AllAdjacencyRules[Model.GridCells[x, y].tileUsed].Contains(rule))
                                {
                                    AllAdjacencyRules[Model.GridCells[x, y].tileUsed].Add(rule);
                                }
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
        }

        #endregion

        #region Private Methods
        #endregion
    }
}