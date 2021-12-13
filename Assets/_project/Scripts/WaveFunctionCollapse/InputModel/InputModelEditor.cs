////////////////////////////////////////////////////////////
// File: InputModelEditor.cs
// Author: Charles Carter
// Date Created: 21/10/21   
// Last Edited By: Charles Carter
// Date Last Edited: 08/12/21
// Brief: A script to create an input model from using a transform in a scene
//////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using UnityEngine;
using WFC.Editor;

namespace WFC.Editor
{
    public class InputModelEditor : MonoBehaviour
    {
        #region Public Fields

        [SerializeField]
        private List<ModelCell> cellsInModel = new List<ModelCell>();

        //The width and height
        [SerializeField]
        private int modelwidth;
        [SerializeField]
        private int modelheight;

        public InputModel modelGenerated;
        public Grid lastGeneratedGrid;

        [SerializeField]
        private bool bAddToTiles = false;
        [SerializeField]
        private bool bAddToRules = false;
        [SerializeField]
        private bool bAddToFreq = false;

        #endregion

        #region Public Methods

        [ContextMenu("Refresh Cells")]
        public void GetTiles()
        {
            cellsInModel.Clear();

            //Making sure the size is set correctly if not set in the inspector
            if(modelwidth == 0 || modelheight == 0)
            {
                Debug.LogError("Input model editor not set correctly", this);
                return;
            }

            //Looping through the hierarchy and getting the cells
            for(int i = 0; i < transform.childCount; ++i)
            {
                //All of these children should have the cell component on them
                if(transform.GetChild(i).TryGetComponent(out ModelCell cell))
                {
                    cellsInModel.Add(cell);
                }
            }
        }

        [ContextMenu("Input Model Generation")]
        public void GeneratedInputModelGrid()
        {
            if(modelwidth == 0 || modelheight == 0) 
            {
                return;
            }

            Grid newGrid = new Grid(modelwidth, modelheight);

            //Going through the grid and setting it to the tiles in the transform's list
            for(int y = 0; y < modelheight; y++)
            {
                for(int x = 0; x < modelwidth; x++) 
                {
                    if(cellsInModel[(y * modelwidth) + x].modelTile == null)
                    {
                        Debug.LogError("Model Grid not correct: " + y + " " + x + " ");
                    }

                    cellsInModel[(y * modelwidth) + x].modelTile.CellX = x;
                    cellsInModel[(y * modelwidth) + x].modelTile.CellY = y;

                    newGrid.GridCells[x, y] = cellsInModel[(y * modelwidth) + x].modelTile;
                }
            }

            //Formulating the input model based on the grid
            modelGenerated = new InputModel(newGrid);

            //Different input models have different intents, not all will want to add to different variables
            modelGenerated.GenerateListOfPotentialTiles();
            modelGenerated.CalculateRelativeFrequency();
            modelGenerated.GenerateAdjacencyRules();

            lastGeneratedGrid = modelGenerated.Model;
        }

        #endregion

        #region Private Methods
        #endregion
    }

}