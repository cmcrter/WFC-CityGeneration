////////////////////////////////////////////////////////////
// File: InputModelEditor.cs
// Author: Charles Carter
// Date Created: 21/10/21   
// Last Edited By: Charles Carter
// Date Last Edited: 21/10/21
// Brief: A script to create an input model from using a transform in a scene
//////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using UnityEngine;

namespace WFC.Editor
{
    public class InputModelEditor : MonoBehaviour
    {
        #region Public Fields

        [SerializeField]
        private List<Cell> tiles;

        //The width and height for these will be the same
        [SerializeField]
        private int size;

        public InputModel modelGenerated;
        public Grid lastGeneratedGrid;

        [SerializeField]
        private bool patternFlipping = false;

        #endregion

        #region Public Methods

        [ContextMenu("Refresh Cells")]
        public void GetTiles()
        {
            tiles = new List<Cell>();
            size = (int)Mathf.Sqrt(transform.childCount);

            for(int i = 0; i < transform.childCount; ++i)
            {
                if(transform.GetChild(i).TryGetComponent(out Cell cell))
                {
                    tiles.Add(cell);
                }
            }
        }

        [ContextMenu("Input Model Generation")]
        public void GeneratedInputModelGrid()
        {
            Grid newGrid = new Grid(size, size);

            for(int x = 0; x < size; ++x)
            {
                for(int y = 0; y < size; ++y) 
                {
                    newGrid.GridCells[x, y] = tiles[x + y];
                }
            }

            lastGeneratedGrid = newGrid;

            //Formulating the input model based on the grid
            modelGenerated = new InputModel(newGrid, patternFlipping);
            modelGenerated.GeneratePatterns(size);
        }

        #endregion

        #region Private Methods
        #endregion
    }

}