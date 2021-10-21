////////////////////////////////////////////////////////////
// File: WaveFunction
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 14/10/21
// Brief: A script to run through the wave function collapse algorithm
//////////////////////////////////////////////////////////// 

using System.Collections;
using UnityEngine;
using WFC.Editor;
using WFC.Rand;

namespace WFC
{
    public class WaveFunction : MonoBehaviour
    {
        #region Variables

        //The grid that this outputs
        [SerializeField]
        private Grid OutputGrid;
        
        //The input model creator and the input model it creates
        [SerializeField]
        private InputModelEditor InputModelEditor;
        [SerializeField]
        private InputModel InputModel;

        //The grids' dimensions used
        [SerializeField]
        private int width;
        [SerializeField]
        private int height;

        //The seed which generates each choice the algorithm will pick
        [SerializeField]
        private int seed;
        //How many times the algorithm will try
        [SerializeField]
        private int iterationLimit;
        //Will generate over time instead of instantly
        [SerializeField]
        private bool incremental = false;
        [SerializeField]
        private float generationSpeed = 0.1f;

        //The overarching coroutine the algorithm will be running in
        private Coroutine CoGenerating;
        //Keeping a reference of the cell to most recently collapse
        Cell mostRecentlyCollapsed = null;

        //The random number generator
        Mersenne_Twister MTNumberGenerator;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if(InputModelEditor)
            {
                InputModel = InputModelEditor.modelGenerated;
            }
            else
            {
                if(Debug.isDebugBuild) 
                {
                    Debug.Log("No input model selected");
                }
                enabled = false;
            }
        }

        #endregion

        #region Public Methods

        public void RunAlgorithm()
        {
            if (CoGenerating != null)
            {
                StopCoroutine(CoGenerating);
            }

            MTNumberGenerator = new Mersenne_Twister(seed);
            CoGenerating = StartCoroutine(Co_GenerateGrid());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// These coroutines are the steps of the algorithm playing out
        /// </summary>
        private IEnumerator Co_GenerateGrid()
        {
            //Creating the grid
            yield return StartCoroutine(Co_CreateGrid());

            //Split it into city sections and set possible tiles from it
            yield return true;

            //Choosing the initial cells' tiles (per section?)
            yield return StartCoroutine(Co_CollapseRandom());

            //Propagation through the grid (return when it's a success)
            yield return StartCoroutine(Co_GridPropagation());

            CoGenerating = null;
            yield return true;
        }

        private IEnumerator Co_CreateGrid()
        {
            OutputGrid = new Grid(width, height);
            yield return true;
        }

        private IEnumerator Co_CollapseRandom()
        {
            //Getting a random cell
            int RandWidth = MTNumberGenerator.ReturnRandom(width);
            int RandHeight = MTNumberGenerator.ReturnRandom(height);

            //Collapsing it
            OutputGrid.GridCells[RandWidth, RandHeight].CollapseCell(MTNumberGenerator.ReturnRandom());

            //Updating the grid constraints before the general propagation begins
            UpdateGridConstraints();

            yield return true;
        }

        //This is the bulk of the algorithm since it's the iteration loop it goes through
        private IEnumerator Co_GridPropagation()
        {
            int currentIterationCount = 0;

            //While not all cells are collapsed
            while (!isGridFullyCollapsed())
            {
                //Pick new cell to collapse (based on cells with lowest possibilities left, guess and record which parts it guessed in this step) 
                yield return SearchForCellToCollapse();

                //Collapse them
                CollapsingNextCell(mostRecentlyCollapsed);

                //Update the constraints (this is the actual propagation step)
                yield return UpdateGridConstraints();

                //Does this work? If yes, repeat, if no, backtrack
                if(!isMapPossible())
                {
                    if(currentIterationCount <= iterationLimit)
                    {
                        Backtrack();
                        currentIterationCount++;
                    }
                    else
                    {
                        yield return false;
                    }
                }
            

                if(incremental)
                {
                    yield return new WaitForSeconds(generationSpeed);
                }

                yield return null;
            }

            yield return true;
        }

        /// <summary>
        /// Some of the functions that fill in the coroutines
        /// </summary>
        private bool isGridFullyCollapsed()
        {
            for(int x = 0; x < OutputGrid.width; ++x)
            {
                for(int y = 0; y < OutputGrid.height; ++y)
                {
                    if(OutputGrid.GridCells[x, y].tileUsed == null) 
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private IEnumerator UpdateGridConstraints()
        {
            //Looking at neighbours (using the Moore neighbourhood) of most recently collapsed cell, removing the impossible tiles from their list, essentially this is the actual propagation function
            //If this causes the neighbours of these neighbours to change, remove impossible tiles from them, creating a cascade of applying constraints
            yield return true;
        }

        //Assuming there's no cells with zero entropy
        private IEnumerator SearchForCellToCollapse()
        {
            //Look for cells with lowest entropy

            //Returning out of the coroutine
            mostRecentlyCollapsed = null;
            yield return true;
        }

        private void CollapsingNextCell(Cell givenCell)
        {
            //Collapse given cell
            givenCell.CollapseCell(MTNumberGenerator.ReturnRandom());
        }

        private bool isMapPossible()
        {
            //Checking to see if it's a failure (there's no possible values for the cells left)
            for(int x = 0; x < OutputGrid.width; ++x)
            {
                for(int y = 0; y < OutputGrid.height; ++y)
                {
                    if(OutputGrid.GridCells[x, y].possibleTiles.Count == 0)
                    {
                        return false;
                    }
                }
            }


            //The map could still be made
            return true;
        }

        private void Backtrack()
        {
            //Going through the history (previous times of searching for a cell to collapse?)
        }

        #endregion
    }
}
