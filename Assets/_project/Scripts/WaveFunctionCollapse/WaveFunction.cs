////////////////////////////////////////////////////////////
// File: WaveFunction
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 27/10/21
// Brief: A script to run through the wave function collapse algorithm
//////////////////////////////////////////////////////////// 

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WFC.Editor;
using WFC.Rand;

namespace WFC
{
    public class WaveFunction : MonoBehaviour
    {
        #region Variables

        [Header("Base Algorithm Variables")]
        //The grid that this outputs
        [SerializeField]
        private Grid OutputGrid;
        
        //The input model creator and the input model it creates
        [SerializeField]
        private InputModelEditor InputModelEditor;
        [SerializeField]
        private InputModel IModel;

        //The grids' dimensions used
        [SerializeField]
        private int width;
        [SerializeField]
        private int height;

        [Header("Algorithm Generation Customization")]
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

        //ToDo: Link the hierarchy to the visualization objects
        [SerializeField]
        private Transform gridParent;

        //The overarching coroutine the algorithm will be running in
        private Coroutine CoGenerating;
        //Keeping a reference of the cell to most recently collapse
        private Cell mostRecentlyCollapsed = null;
        private int mostRecentX, mostRecentY;

        //The random number generator
        private Mersenne_Twister MTNumberGenerator;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if(InputModelEditor)
            {
                IModel = InputModelEditor.modelGenerated;
            }
            else
            {
                if(Debug.isDebugBuild) 
                {
                    Debug.Log("No input model selected");
                }
                enabled = false;
            }

            gridParent = gridParent ?? transform;
        }

        #endregion

        #region Public Methods

        [ContextMenu("Run WFC")]
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

            InstantiateGrid();

            CoGenerating = null;
            yield return true;
        }

        private IEnumerator Co_CreateGrid()
        {
            OutputGrid = new Grid(width, height);

            for(int x = 0; x < OutputGrid.width; ++x)
            {
                for(int y = 0; y < OutputGrid.height; ++y)
                {
                    OutputGrid.GridCells[x, y].CellX = x;
                    OutputGrid.GridCells[x, y].CellY = y;
                }
            }

            yield return true;
        }

        private IEnumerator Co_CollapseRandom()
        {
            //Getting a random cell
            mostRecentX = MTNumberGenerator.ReturnRandom(width);
            mostRecentY = MTNumberGenerator.ReturnRandom(height);

            //Collapsing it
            OutputGrid.GridCells[mostRecentX, mostRecentY].CollapseCell(MTNumberGenerator.ReturnRandom(), IModel);

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
                        //Start again completely?
                        yield return false;
                    }
                }

                //Waiting for a specific amount of time (may help with the visualisation aspect)
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
            //Looking at neighbours (using the von Neumann neighbourhood) of most recently collapsed cell, removing the impossible tiles from their list, this is the actual "propagation" in the propagation function
            //If this causes the neighbours of these neighbours to change, remove impossible tiles from them, creating a cascade of applying constraints
            bool propagating = true;
            Cell[] firstNeighbours = OutputGrid.GetNeighbours(mostRecentX, mostRecentY);
            List<Cell> Neighbours = firstNeighbours.ToList();
            List<Cell> alreadyPropagated = new List<Cell>();

            alreadyPropagated.Add(mostRecentlyCollapsed);
            OutputGrid.GridCells[mostRecentX, mostRecentY].UpdateConstraints(firstNeighbours, IModel, out List<Tile> initialConstrainedTiles);

            if(initialConstrainedTiles.Count == 0 )
            {
                yield return true;
            }

            //Whilst it's propagating
            while(propagating)
            {
                //if there are neighbours to look at, look at them and propagate
                if(Neighbours.Count > 0)
                {
                    for(int i = 0; i < Neighbours.Count; ++i)
                    {
                        //Updating the constraints on the current neighbour
                        Neighbours[i].UpdateConstraints(OutputGrid.GetNeighbours(mostRecentX, mostRecentY), IModel, out List<Tile> ConstrainedTiles);
                        alreadyPropagated.Add(Neighbours[i]);

                        //Look through the neighbours of these neighbours
                        Cell[] theseNeighbours = OutputGrid.GetNeighbours(Neighbours[i].CellX, Neighbours[i].CellY);

                        for(int j = 0; i < theseNeighbours.Length; ++j)
                        {
                            //Don't propagate on already checked tiles
                            if(alreadyPropagated.Contains(theseNeighbours[j]))
                            {
                                continue;
                            }

                            Neighbours.Add(theseNeighbours[i]);

                            //If they propagate (?), add their neighbours to list
                            //Else, continue
                        }

                        Neighbours.RemoveAt(i);
                        --i;
                    }
                }
                else
                {
                    propagating = false;
                }
            }
            
            //TODO: Look at using a stack to do this?

            yield return true;
        }

        //Assuming there's no cells with zero entropy
        private IEnumerator SearchForCellToCollapse()
        {
            //Look for cells with lowest entropy
            Cell currentCellWithLowestEntropy = null;

            for(int x = 0; x < OutputGrid.width; ++x)
            {
                for(int y = 0; y < OutputGrid.height; ++y)
                {
                    //There's no tile selected here
                    if(OutputGrid.GridCells[x, y].tileUsed == null) 
                    {
                        //There's not a currently chosen cell
                        if(currentCellWithLowestEntropy == null)
{
                            currentCellWithLowestEntropy = OutputGrid.GridCells[x, y];
                            mostRecentX = x;
                            mostRecentY = y;

                            continue;
                        }

                        //Seeing if the entropy is lower
                        if(OutputGrid.GridCells[x, y].currentEntropy < currentCellWithLowestEntropy.currentEntropy)
                        {
                            currentCellWithLowestEntropy = OutputGrid.GridCells[x, y];
                            mostRecentX = x;
                            mostRecentY = y;
                        }
                    }
                }
            }

            //Returning out of the coroutine with the cell to use
            mostRecentlyCollapsed = currentCellWithLowestEntropy;

            yield return true;
        }

        private void CollapsingNextCell(Cell givenCell)
        {
            //Collapse given cell
            givenCell.CollapseCell(MTNumberGenerator.ReturnRandom(), IModel);
        }

        private bool isMapPossible()
        {
            //Checking to see if it's a failure (there's no possible values for the cells left)
            for(int x = 0; x < OutputGrid.width; ++x)
            {
                for(int y = 0; y < OutputGrid.height; ++y)
                {
                    //Even if a tile is selected, the possible tiles will be 1 or above
                    if(OutputGrid.GridCells[x, y].possibleTiles.Count == 0)
                    {
                        return false;
                    }
                }
            }

            //The map could still be made
            return true;
        }

        private void InstantiateGrid()
        {
            for(int x = 0; x < OutputGrid.width; ++x)
            {
                for(int y = 0; y < OutputGrid.height; ++y)
                {
                    Instantiate(OutputGrid.GridCells[x, y].tileUsed.Prefab, new Vector3(x, 0, y), Quaternion.identity, gridParent);
                }
            }
        }

        private void Backtrack()
        {
            //Going through the history (previous times of searching for a cell to collapse?)
        }

        #endregion
    }
}
