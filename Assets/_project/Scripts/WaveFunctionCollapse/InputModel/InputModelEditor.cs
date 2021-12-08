////////////////////////////////////////////////////////////
// File: InputModelEditor.cs
// Author: Charles Carter
// Date Created: 21/10/21   
// Last Edited By: Charles Carter
// Date Last Edited: 27/10/21
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

        //The width and height for these will be the same
        [SerializeField]
        private int size;

        public InputModel modelGenerated;
        public Grid lastGeneratedGrid;

        #endregion

        #region Public Methods

        [ContextMenu("Refresh Cells")]
        public void GetTiles()
        {
            cellsInModel.Clear();

            //Making sure the size is set correctly if not set in the inspector
            if(size == 0)
            {
                size = (int)Mathf.Sqrt(transform.childCount);
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
            if(size == 0) 
            {
                return;
            }

            Grid newGrid = new Grid(size, size);

            //Going through the grid and setting it to the tiles in the transform's list
            for(int x = 0; x < size; ++x)
            {
                for(int y = 0; y < size; ++y) 
                {
                    cellsInModel[(y * size) + x].modelTile.CellX = x;
                    cellsInModel[(y * size) + x].modelTile.CellY = y;

                    newGrid.GridCells[x, y] = cellsInModel[(y * size) + x].modelTile;
                }
            }

            //Formulating the input model based on the grid
            modelGenerated = new InputModel(newGrid);
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