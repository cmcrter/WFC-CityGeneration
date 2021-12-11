////////////////////////////////////////////////////////////
// File: InputModelCompiler.cs
// Author: Charles Carter
// Date Created: 07/12/2021
// Last Edited By: Charles Carter
// Date Last Edited: 07/12/2021
// Brief: A script to combine multiple InputModelEditors' input models into 1 generated input model
//////////////////////////////////////////////////////////// 

using System;
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

        void Awake()
        {
            //Going through and preparing the input models
            for(int i = 0; i < editorsUsed.Count; ++i)
            {
                editorsUsed[i].GetTiles();
                editorsUsed[i].GeneratedInputModelGrid();
            }
        }

        void Start()
        {
            GetAllPossibleTiles();
            GetRulesAndFrequencies();
        }

        #endregion

        #region Public Methods

        public void GetAllPossibleTiles()
        {
            //Go through the editors
            for(int i = 0; i < editorsUsed.Count; ++i)
            {
                allPossibleTiles = allPossibleTiles.Union(editorsUsed[i].modelGenerated.tilesUsed).ToList();
            }

            //Clearing the rules it may have already
            for(int i = 0; i < allPossibleTiles.Count; ++i)
            {
                allPossibleTiles[i].Frequency = 0;
                allPossibleTiles[i].CanGoNextTo = new List<AdjacencyRule>();
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
                    if(editorsUsed[i].modelGenerated.AllAdjacencyRules.ContainsKey(allPossibleTiles[j]))
                    {
                        allPossibleTiles[j].CanGoNextTo = allPossibleTiles[j].CanGoNextTo.Union(editorsUsed[i].modelGenerated.AllAdjacencyRules[allPossibleTiles[j]]).ToList();
                    }

                    if(editorsUsed[i].modelGenerated.FrequenciesOfTiles.ContainsKey(allPossibleTiles[j]))
                    {
                        allPossibleTiles[j].Frequency += editorsUsed[i].modelGenerated.FrequenciesOfTiles[allPossibleTiles[j]];
                    }
                }
            }
        }

        #endregion

        #region Private Methods
        #endregion
    }
}