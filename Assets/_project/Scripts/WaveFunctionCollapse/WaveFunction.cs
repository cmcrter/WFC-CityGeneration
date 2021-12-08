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

        //The input model compiler, which is the sum of selected input models
        [SerializeField]
        private InputModelCompiler ModelCompiler;

        //The grids' dimensions used
        [SerializeField]
        private int width;
        [SerializeField]
        private int height;

        [Header("Backtracking Variables")]
        //Storing some states for backtracking
        [SerializeField]
        private List<Grid> GridStates;
        [SerializeField]
        private Tile LastSelectedTile;

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

        private bool bConstraining = true;
        //An option for propagation through a less efficient method
        [SerializeField]
        private bool bBruteForce = false;

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
            ModelCompiler = InputModelCompiler.instance;
        }

        #endregion

        #region Public Methods

        //[ContextMenu("ReSetup WFC")]
        //public void UseInputModel()
        //{
        //    InputModelEditor.GetTiles();
        //    InputModelEditor.GeneratedInputModelGrid();
        //}

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

            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    //Debug.Log("Cell: " + x + "," + y + " being set");

                    foreach (Tile tile in ModelCompiler.allPossibleTiles)
                    {
                        OutputGrid.GridCells[x, y].possibleTiles.Add(tile);
                    }                    

                    //Setting up the cells' objects in the Unity scene
                    GameObject cellGo = Instantiate(cellPrefab, new Vector3(x, 1, -y), Quaternion.identity, gridParent);
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
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    OutputGrid.GridCells[x, y].currentEntropy = OutputGrid.GridCells[x, y].calculateEntropyValue();
                }
            }

            yield return Co_UpdatingAllVisuals();

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

            yield return true;
        }

        //This is the bulk of the algorithm since it's the iteration loop it goes through
        private IEnumerator Co_GridPropagation()
        {
            //Updating from the first collapsed cell
            int currentIterationCount = 0;

            if(bBruteForce)
            {
                yield return Co_BruteForceUpdateGridConstraints();
            }
            else
            {
                yield return Co_EfficientUpdateGridConstraints();
            }

            //While not all cells are collapsed
            while(!isGridFullyCollapsed())
            {
                yield return Co_UpdatingAllVisuals();

                //Pick new cell to collapse (based on cells with lowest possibilities left, guess and record which parts it guessed in this step) 
                //Also known as the observe step
                yield return Co_SearchForCellToCollapse();

                //Collapse them
                CollapsingNextCell(mostRecentlyCollapsed);

                //Update the constraints (this is the actual propagation step)
                if(bBruteForce)
                {
                    yield return Co_BruteForceUpdateGridConstraints();
                }
                else
                {
                    yield return Co_EfficientUpdateGridConstraints();
                }

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

            cellVisualisers[(mostRecentX * width) + mostRecentY].UpdateVisuals();

            yield return true;
        }

        /// <summary>
        /// Some of the functions that fill in the coroutines
        /// </summary>
        private bool isGridFullyCollapsed()
        {
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    if(OutputGrid.GridCells[x, y].tileUsed == null) 
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private IEnumerator Co_UpdatingAllVisuals()
        {
            for(int i = 0; i < cellVisualisers.Count; ++i)
            {
                cellVisualisers[i].UpdateVisuals();
            }

            yield return true;
        }

        private IEnumerator Co_BruteForceUpdateGridConstraints()
        {         
            bConstraining = true;

            while(bConstraining)
            {
                //Going through the grid, as soon as a cell is constrained, go through the grid again...
                CheckingGridForConstraints();
            }

            yield return true;
        }

        //A function to go through the grid until constraints are needed to be applied to a cell
        private void CheckingGridForConstraints()
        {
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    if(OutputGrid.GridCells[x, y].ApplyConstraintsBasedOnPotential(OutputGrid))
                    {
                        return;
                    }
                }
            }

            bConstraining = false;
        }

        private IEnumerator Co_EfficientUpdateGridConstraints()
        {
            //Looking at neighbours (using the von Neumann neighbourhood) of most recently collapsed cell, removing the impossible tiles from their list, this is the actual "propagation" in the propagation function
            //If this causes the neighbours of these neighbours to change, remove impossible tiles from them, creating a cascade of applying constraints
            bConstraining = true;

            Stack<Cell> Neighbours = new Stack<Cell>();
            bool[,] propagatedCells = new bool[width, height];
            propagatedCells[mostRecentlyCollapsed.CellX, mostRecentlyCollapsed.CellY] = true;

            //Starting by adding the relevant initial cells to the stack
            foreach(Cell cell in OutputGrid.GetNeighbours(mostRecentlyCollapsed.CellX, mostRecentlyCollapsed.CellY).Item1) 
            {
                if(!cell.tileUsed)
                {
                    Neighbours.Push(cell);
                }
            }

            while(bConstraining)
            {
                //if there are neighbours to look at, look at them and propagate
                if(Neighbours.Count > 0)
                {
                    //Getting the top cell
                    Cell currentCell = Neighbours.Peek();

                    //If this cell isn't selected yet and not propagated to yet
                    if(currentCell.tileUsed == null && propagatedCells[currentCell.CellX, currentCell.CellY] == false)
                    {
                        //Applying constraints to this cell
                        if(currentCell.ApplyConstraintsBasedOnPotential(OutputGrid))
                        {
                            //Getting the neighbours of the cell just constrained
                            List<Cell> currentNeighbours = OutputGrid.GetNeighbours(currentCell.CellX, currentCell.CellY).Item1;
                            Neighbours.Pop();
                            propagatedCells[currentCell.CellX, currentCell.CellY] = true;

                            //Going through these neighbours and seeing if they needed to be added to the list
                            foreach(Cell neighbour in currentNeighbours)
                            {
                                if(neighbour.tileUsed == null && propagatedCells[neighbour.CellX, neighbour.CellY] == false)
                                {
                                    Neighbours.Push(neighbour);
                                }
                            }
                        }
                        else
                        {
                            Neighbours.Pop();
                        }
                    }
                    else
                    {
                        Neighbours.Pop();
                        continue;
                    }
                }
                else
                {
                    bConstraining = false;
                }
            }

            yield return true;
        }

        //Assuming there's no cells with zero entropy
        private IEnumerator Co_SearchForCellToCollapse()
        {
            //Look for cells with lowest entropy
            Cell currentCellWithLowestEntropy = null;

            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
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
            if(givenCell.CollapseCell(MTNumberGenerator))
            {
                Instantiate(givenCell.tileUsed.Prefab, cellTransforms[(givenCell.CellY * width) + givenCell.CellX]);
            }
        }

        private bool isMapPossible()
        {
            //Checking to see if it's a failure (there's no possible values for the cells left)
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
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
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
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
