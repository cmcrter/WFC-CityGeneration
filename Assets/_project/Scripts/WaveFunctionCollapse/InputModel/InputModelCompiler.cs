////////////////////////////////////////////////////////////
// File: InputModelCompiler.cs
// Author: Charles Carter
// Date Created: 07/12/2021
// Last Edited By: Charles Carter
// Date Last Edited: 14/12/2021
// Brief: A script to combine multiple InputModelEditors' input models into 1 generated input model
//////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC.Editor
{
    public class InputModelCompiler : MonoBehaviour
    {
        #region Variables

        //The algorithm script that will use the compliler
        [SerializeField]
        private WaveFunction waveFunction;
        //All the input models that the compiler will use
        [SerializeField]
        private List<InputModelEditor> editorsUsed;

        //Each cell will know the possible tiles that could've be instantiated there from the start
        public List<Tile> allPossibleTiles;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            allPossibleTiles = new List<Tile>();

            //Going through and preparing the input models
            for(int i = 0; i < editorsUsed.Count; ++i)
            {
                editorsUsed[i].GetTiles();
                editorsUsed[i].GeneratedInputModelGrid();
            }

            GetAllPossibleTiles();
            GetRulesAndFrequencies();
        }

        private void OnDisable()
        {
            //Clearing up the tiles for the next time it's run (doing it this way means that it works, but does make the adjacency search less efficient) 
            for(int j = 0; j < allPossibleTiles.Count; ++j)
            {
                allPossibleTiles[j].CanGoNextTo = new List<AdjacencyRule>();
                allPossibleTiles[j].Frequency = 0;
            }
        }

        #endregion

        #region Public Methods

        [ContextMenu("Load Compiler")]
        public void LoadComp()
        {
            GetAllPossibleTiles();
            GetRulesAndFrequencies();
        }

        public void GetAllPossibleTiles()
        {
            //Go through the editors
            for(int i = 0; i < editorsUsed.Count; ++i)
            {
                if(editorsUsed[i].modelGenerated.tilesUsed != null)
                {
                    //Union function adds the non-duplicate values to the list as an enumerable which I'm then casting to a list 
                    allPossibleTiles = allPossibleTiles.Union(editorsUsed[i].modelGenerated.tilesUsed).ToList();
                }
            }
        }

        public void GetRulesAndFrequencies()
        {
            //Add their rules onto the tiles
            for(int j = 0; j < allPossibleTiles.Count; ++j)
            {
               //Go through the editors
                for(int i = 0; i < editorsUsed.Count; ++i)
                {
                    if(editorsUsed[i].modelGenerated.AllAdjacencyRules != null)
                    {
                        //The union function means there'll be no duplicates
                        if(editorsUsed[i].modelGenerated.AllAdjacencyRules.ContainsKey(allPossibleTiles[j]))
                        {
                            allPossibleTiles[j].CanGoNextTo = allPossibleTiles[j].CanGoNextTo.Union(editorsUsed[i].modelGenerated.AllAdjacencyRules[allPossibleTiles[j]]).ToList();
                        }
                    }

                    if(editorsUsed[i].modelGenerated.FrequenciesOfTiles != null)
                    {
                        if(editorsUsed[i].modelGenerated.FrequenciesOfTiles.ContainsKey(allPossibleTiles[j]))
                        {
                            allPossibleTiles[j].Frequency += editorsUsed[i].modelGenerated.FrequenciesOfTiles[allPossibleTiles[j]];
                        }
                    }
                }
            }
        }

        #endregion
    }
}