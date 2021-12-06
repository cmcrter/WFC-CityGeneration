////////////////////////////////////////////////////////////
// File: WaveFunction
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 06/12/21
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
        [SerializeField]
        private GameObject cellPrefab;
        [SerializeField]
        private List<CellVisualiser> cellVisualisers;
        [SerializeField]
        private List<Transform> cellTransforms;

        //The overarching coroutine the algorithm will be running in
        private Coroutine CoGenerating;
        //Keeping a reference of the cell to most recently collapse
        [SerializeField]
        private Cell mostRecentlyCollapsed;
        private int mostRecentX, mostRecentY;

        [SerializeField]
        private List<Cell> mostRecentNeighbours;
        bool bConstraining = true;

        //The random number generator
        private Mersenne_Twister MTNumberGenerator;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            gridParent = gridParent ?? transform;
        }

        private void Start()
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
        }

        #endregion

        #region Public Methods

        [ContextMenu("Setup WFC")]
        public void UseInputModel()
        {
            InputModelEditor.GetTiles();
            InputModelEditor.GeneratedInputModelGrid();
        }

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

            //InstantiateGrid();
            Debug.Log("Algorithm Finished");

            CoGenerating = null;
            yield return true;
        }

        private IEnumerator Co_CreateGrid()
        {
            //Instancing the new grid and relevant lists
            OutputGrid = new Grid(width, height);
            cellTransforms = new List<Transform>();
            cellVisualisers = new List<CellVisualiser>();

            for(int x = 0; x < OutputGrid.width; x++)
            {
                for(int y = 0; y < OutputGrid.height; y++)
                {
                    //Debug.Log("Cell: " + x + "," + y + " being set");

                    //Setting up the cells' values
                    OutputGrid.GridCells[x, y].CellX = x;
                    OutputGrid.GridCells[x, y].CellY = y;

                    OutputGrid.GridCells[x, y].possibleTiles = new List<Tile>();

                    foreach(Tile tile in IModel.tilesUsed)
                    {
                        OutputGrid.GridCells[x, y].possibleTiles.Add(tile);
                    }

                    //Setting up the cells' objects in the Unity scene
                    GameObject cellGo = Instantiate(cellPrefab, new Vector3(x, 1, y), Quaternion.identity, gridParent);
                    Transform cellT = cellGo.transform;
                    cellTransforms.Add(cellT);

                    if(cellT.TryGetComponent(out CellVisualiser cVis))
                    {
                        cellVisualisers.Add(cVis);
                        cVis.thisCell = OutputGrid.GridCells[x, y];
                    }
                }
            }

            //Now all the cells are set up, calculate the based entropy value
            for(int x = 0; x < OutputGrid.width; x++)
            {
                for(int y = 0; y < OutputGrid.height; y++)
                {
                    OutputGrid.GridCells[x, y].currentEntropy = OutputGrid.GridCells[x, y].calculateEntropyValue(IModel);
                }
            }

            yield return true;
        }

        private IEnumerator Co_CollapseRandom()
        {
            //Getting a random cell
            mostRecentX = Mathf.Abs(MTNumberGenerator.ReturnRandom(width));
            mostRecentY = Mathf.Abs(MTNumberGenerator.ReturnRandom(height));

            //Collapsing it
            CollapsingNextCell(OutputGrid.GridCells[mostRecentX, mostRecentY]);
            mostRecentlyCollapsed = OutputGrid.GridCells[mostRecentX, mostRecentY];
            mostRecentNeighbours = OutputGrid.GetNeighbours(mostRecentX, mostRecentY);

            yield return true;
        }

        //This is the bulk of the algorithm since it's the iteration loop it goes through
        private IEnumerator Co_GridPropagation()
        {
            //Updating from the first collapsed cell
            int currentIterationCount = 0;
            yield return UpdateGridConstraints();

            //While not all cells are collapsed
            while(!isGridFullyCollapsed())
            {
                //Pick new cell to collapse (based on cells with lowest possibilities left, guess and record which parts it guessed in this step) 
                //Also known as the observe step
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
            bConstraining = true;

            while(bConstraining)
            {
                //Going through the grid, as soon as a cell is constrained, go through the grid again...
                CheckingGridForConstraints();
            }

            yield return true;
        }

        private void CheckingGridForConstraints()
        {
            for(int x = 0; x < OutputGrid.width; ++x)
            {
                for(int y = 0; y < OutputGrid.height; ++y)
                {
                    if(OutputGrid.GridCells[x, y].ApplyConstraintsBasedOnPotential(OutputGrid, IModel))
                    {
                        return;
                    }
                }
            }

            bConstraining = false;
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
                    if(!OutputGrid.GridCells[x, y].isCollapsed()) 
                    {
                        if(currentCellWithLowestEntropy == null)
                        {
                            currentCellWithLowestEntropy = OutputGrid.GridCells[x, y];
                        }
                
                        //Seeing if the entropy is lower but not zero
                        if(OutputGrid.GridCells[x, y].currentEntropy < currentCellWithLowestEntropy.currentEntropy)
                        {
                            currentCellWithLowestEntropy = OutputGrid.GridCells[x, y];
                        }
                    }
                }
            }

            mostRecentX = currentCellWithLowestEntropy.CellX;
            mostRecentY = currentCellWithLowestEntropy.CellY;

            //Returning out of the coroutine with the cell to use
            mostRecentlyCollapsed = currentCellWithLowestEntropy;
            yield return true;
        }

        private void CollapsingNextCell(Cell givenCell)
        {
            if(givenCell == null)
            {
                return;
            }

            //Collapse given cell
            if(givenCell.CollapseCell(MTNumberGenerator, IModel))
            {
                Instantiate(givenCell.tileUsed.Prefab, cellTransforms[(givenCell.CellX * width) + givenCell.CellY]);
            }
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
                    Instantiate(OutputGrid.GridCells[x, y].tileUsed.Prefab, cellTransforms[(x * width) + y]);
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
