////////////////////////////////////////////////////////////
// File: WaveFunction
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 14/12/21
// Brief: A script to run through the wave function collapse algorithm
//////////////////////////////////////////////////////////// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WFC.Editor;
using WFC.Rand;
using WFC.UI;

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

        //[Header("Backtracking Variables")]
        ////Storing some states for backtracking
        //[SerializeField]
        //private List<Grid> GridStates;
        ////These are the tiles last selected from the previous cell, which can be multiple due to backtracking to try multiple
        //[SerializeField]
        //private List<Tile> LastSelectedTiles;

        //The seed which generates each choice the algorithm will pick
        private int initialSeed;
        [Header("Algorithm Generation Customization")]
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
        private int currentIterationCount;

        [SerializeField]
        private Transform gridParent;
        [SerializeField]
        private GameObject cellPrefab;
        [SerializeField]
        private List<CellVisualiser> cellVisualisers = new List<CellVisualiser>();
        [SerializeField]
        private List<Transform> cellTransforms = new List<Transform>();

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
        [SerializeField]
        private bool bPauseEditorOnFailure = false;
        [SerializeField]
        private bool bPaused = false;
        private bool bEntropyShown = true;

        //Whether the grid needs to be sectioned or not based on the preset
        [SerializeField]
        private bool bSectioned = false;
        //Should always be park, residential, business
        [SerializeField]
        private List<TilePreset> sectionPresets;
        //The random number generator used (potential for UI to change which type used)
        private Mersenne_Twister MTNumberGenerator;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            gridParent = gridParent ?? transform;
            ModelCompiler = ModelCompiler ?? FindObjectOfType<InputModelCompiler>();
        }

        private void Start()
        {
            initialSeed = seed;
        }

        #endregion

        #region Public Methods

        //Originally was going to have the wave function be able to pick input model compilers/editors
        //[ContextMenu("ReSetup WFC")]
        //public void UseInputModel()
        //{
        //    InputModelEditor.GetTiles();
        //    InputModelEditor.GeneratedInputModelGrid();
        //}

        [ContextMenu("Pause WFC")]
        public void PauseAlgorithm()
        {
            bPaused = !bPaused;
        }

        [ContextMenu("Restart WFC")]
        public void RestartAlgorithm()
        {
            seed = initialSeed;
            RunAlgorithm();
        }

        [ContextMenu("Run WFC with next seed")]
        public void RerunAlgorithm()
        {
            //This is the current form of backtracking
            currentIterationCount++;
            seed++;

            RunAlgorithm();
        }

        [ContextMenu("Run WFC")]
        public void RunAlgorithm()
        {
            if(CoGenerating != null)
            {
                //Due to the way nested coroutines work, stopping all the coroutines here make sure there's no overlap
                StopAllCoroutines();
                CoGenerating = null;
            }

            MTNumberGenerator = new Mersenne_Twister(seed);
            CoGenerating = StartCoroutine(Co_GenerateGrid());
        }

        /// <summary>
        /// Utility functions for other scripts to customize visuals / parts of the algorithm
        /// </summary>
        public void SetSeed(int newSeed)
        {
            seed = newSeed;
        }

        public int GetSeed()
        {
            return seed;
        }

        public void SetBruteForce(bool isFast)
        {
            bBruteForce = !isFast;
        }

        public void SetEntropyShown(bool isShown)
        {
            if(cellVisualisers == null || cellVisualisers.Count == 0 || CoGenerating == null)
            {
                return;
            }

            bEntropyShown = isShown;

            for(int i = 0; i < cellVisualisers.Count; ++i)
            {
                cellVisualisers[i].EntropyText.gameObject.SetActive(isShown);
            }
        }

        //Setting the preset to use, which will have a knock on effect if multiple are used
        public void SetPreset(InputModelCompiler newCompiler)
        {
            ModelCompiler = newCompiler;
            bSectioned = false;
        }

        public void SetPreset(List<TilePreset> newCompilers)
        {
            sectionPresets = new List<TilePreset>();

            foreach(TilePreset preset in newCompilers)
            {
                sectionPresets.Add(preset);
            }

            bSectioned = true;
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

            //Choosing the initial cells' tiles (per section?)
            yield return StartCoroutine(Co_CollapseRandom());

            yield return new WaitForSeconds(generationSpeed);

            //Propagation through the grid (return when it's a success)
            yield return StartCoroutine(Co_GridPropagation());

            if(Debug.isDebugBuild)
            {
                Debug.Log("Algorithm Finished on seed: " + seed);
            }

            CoGenerating = null;
        }

        private IEnumerator Co_CreateGrid()
        {
            //Clearing any previous grid and creating the new one
            yield return StartCoroutine(Co_ClearGrid());
            yield return StartCoroutine(Co_CreateGridObjects());
        }

        /// <summary>
        /// Collapsing a random cell in the grid
        /// </summary>
        private IEnumerator Co_CollapseRandom()
        {
            //Getting a random cell
            mostRecentX = Mathf.Abs(MTNumberGenerator.ReturnRandom(width));
            mostRecentY = Mathf.Abs(MTNumberGenerator.ReturnRandom(height));

            //Collapsing it
            yield return StartCoroutine(Co_CollapsingNextCell(OutputGrid.GridCells[mostRecentX, mostRecentY]));
            mostRecentlyCollapsed = OutputGrid.GridCells[mostRecentX, mostRecentY];

            //Updating from the first collapsed cell
            if(bBruteForce)
            {
                yield return StartCoroutine(Co_BruteForceUpdateGridConstraints());
            }
            else
            {
                yield return StartCoroutine(Co_EfficientUpdateGridConstraints());
            }
        }

        //This is the bulk of the algorithm since it's the iteration loop it goes through
        private IEnumerator Co_GridPropagation()
        {
            //While not all cells are collapsed
            while(!isGridFullyCollapsed())
            {
                //Can the current grid work? If yes, continue, if no, backtrack
                if(!isMapPossible())
                {
                    //Visually catching everything up to be able to potentially see where it went wrong
                    yield return StartCoroutine(Co_CalculateAllEntropys());
                    yield return StartCoroutine(Co_UpdatingAllVisuals());

                    if(Debug.isDebugBuild)
                    {
                        Debug.Log("Grid not possible anymore", this);
                    }

                    if(bPauseEditorOnFailure && Debug.isDebugBuild)
                    {
                        Debug.Break();
                        yield return null;
                    }

                    RerunAlgorithm();
                }

                while(bPaused)
                {
                    yield return null;
                }


                //Pick new cell to collapse (based on cells with lowest possibilities left, guess and record which parts it guessed in this step) 
                //Also known as the observe step
                yield return StartCoroutine(Co_SearchForCellToCollapse());

                //Collapse them
                yield return StartCoroutine(Co_CollapsingNextCell(mostRecentlyCollapsed));

                yield return new WaitForSeconds(generationSpeed);

                //Update the constraints (this is the actual propagation step)
                if(bBruteForce)
                {
                    yield return StartCoroutine(Co_BruteForceUpdateGridConstraints());
                }
                else
                {
                    yield return StartCoroutine(Co_EfficientUpdateGridConstraints());
                }

                //Waiting for a specific amount of time (may help with the visualisation aspect)
                if(incremental)
                {
                    yield return new WaitForSeconds(generationSpeed);
                }

                yield return null;
            }

            //GridStates.Add(OutputGrid);
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
            //Loop through the visualisers and update them (quicker than looping through based on the grid)
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
                        cellVisualisers[(y * width) + x].UpdateVisuals();
                        return;
                    }
                }
            }

            bConstraining = false;
        }

        /// <summary>
        /// This is the more efficient way of propagating, is only finished once all the needed cells apply their constraints
        /// </summary>
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
                            //Updating current cell's values
                            propagatedCells[currentCell.CellX, currentCell.CellY] = true;
                            cellVisualisers[(currentCell.CellY * width) + currentCell.CellX].UpdateVisuals();

                            //Getting the neighbours of the cell just constrained
                            List<Cell> currentNeighbours = OutputGrid.GetNeighbours(currentCell.CellX, currentCell.CellY).Item1;
                            Neighbours.Pop();

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

            if(currentCellWithLowestEntropy == null)
            {
                yield return false;
            }

            mostRecentX = currentCellWithLowestEntropy.CellX;
            mostRecentY = currentCellWithLowestEntropy.CellY;

            //Returning out of the coroutine with the cell to use
            mostRecentlyCollapsed = currentCellWithLowestEntropy;

            yield return true;
        }

        private IEnumerator Co_CollapsingNextCell(Cell givenCell)
        {
            yield return new WaitForSeconds(generationSpeed);

            //Collapse given cell
            if(givenCell.CollapseCell(MTNumberGenerator) && (givenCell.CellY * width) + givenCell.CellX <  cellTransforms.Count)
            {
                //LastSelectedTiles.Add(givenCell.tileUsed);
                Instantiate(givenCell.tileUsed.Prefab, cellTransforms[(givenCell.CellY * width) + givenCell.CellX]);
                cellVisualisers[(givenCell.CellY * width) + givenCell.CellX].UpdateVisuals();
                yield return true;
            }

            yield return false;
        }

        private bool isMapPossible()
        {
            //Checking to see if it's a failure (there's no possible values for the cells left)
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    //Even if a tile is selected, the possible tiles will be 1 or above
                    if(OutputGrid.GridCells[x, y].possibleTiles.Count == 0 && !OutputGrid.GridCells[x, y].isCollapsed())
                    {
                        return false;
                    }
                }
            }

            //The map could still be made
            return true;
        }

        private IEnumerator Co_CreateGridObjects()
        {
            //Instancing the new grid and relevant lists
            OutputGrid = new Grid(width, height);
            cellTransforms = new List<Transform>();
            cellVisualisers = new List<CellVisualiser>();

            GridPartitioner partitioner = new GridPartitioner(OutputGrid, sectionPresets, MTNumberGenerator);

            //The grid needs to be put into sections
            if(bSectioned)
            {
               partitioner.RunPartition();
            }

            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    OutputGrid.GridCells[x, y].possibleTiles = new List<Tile>();

                    //Debug.Log("Cell: " + x + "," + y + " being set");
                    if(bSectioned)
                    {
                        List<Tile> thesePossibleTiles = partitioner.GetTilesBasedOnCoord(x, y);
                        OutputGrid.GridCells[x, y].sectionIndex = partitioner.SectionIndex(OutputGrid.GridCells[x, y]);

                        foreach(Tile tile in thesePossibleTiles)
                        {
                            OutputGrid.GridCells[x, y].possibleTiles.Add(tile);
                        }
                    }
                    else
                    {
                        foreach(Tile tile in ModelCompiler.allPossibleTiles)
                        {
                            OutputGrid.GridCells[x, y].possibleTiles.Add(tile);
                        }
                    }

                    //Setting up the cells' objects in the Unity scene
                    GameObject cellGo = Instantiate(cellPrefab, new Vector3(x, 1, -y), Quaternion.identity, gridParent);
                    Transform cellT = cellGo.transform;
                    cellTransforms.Add(cellT);

                    //Getting the cells' visualiser and setting it
                    if(cellT.TryGetComponent(out CellVisualiser cVis))
                    {
                        cellVisualisers.Add(cVis);
                        cVis.cellToVisualise = OutputGrid.GridCells[x, y];

                        if(bSectioned)
                        {
                            cVis.SetTextColour(partitioner.SectionIndex(OutputGrid.GridCells[x, y]));
                        }

                        cVis.cellToVisualise.currentEntropy = cVis.cellToVisualise.calculateEntropyValue();
                        cVis.EntropyText.gameObject.SetActive(bEntropyShown);
                        cVis.UpdateVisuals();
                    }
                }
            }

            yield return true;
        }

        private IEnumerator Co_ClearGrid()
        {
            //Only clearing the grid if it needs to be cleared
            if(cellTransforms != null)
            {
                for(int i = 0; i < cellTransforms.Count; ++i)
                {
                    if(cellTransforms[i] != null)
                    {
                        Destroy(cellTransforms[i].gameObject);
                    }
                }

                cellTransforms.Clear();
                cellVisualisers.Clear();
            }

            yield return true;
        }

        /// <summary>
        /// Going through the grid and recalculating all their entropies
        /// </summary>
        private IEnumerator Co_CalculateAllEntropys()
        {
            //Using it sparingly
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    OutputGrid.GridCells[x, y].currentEntropy = OutputGrid.GridCells[x, y].calculateEntropyValue();
                }
            }

            yield return true;
        }

        //This was back when I wanted to calculate all the values first, then spawn the grid at the end
        //private void InstantiateGrid()
        //{
        //    for(int y = 0; y < height; y++)
        //    {
        //        for(int x = 0; x < width; x++)
        //        {
        //            Instantiate(OutputGrid.GridCells[x, y].tileUsed.Prefab, cellTransforms[(x * width) + y]);
        //        }
        //    }
        //}

        //Thought this might be needed for backtracking
        //private void ReimplementGrid()
        //{
        //    for(int i = 0; i < cellTransforms.Count; ++i)
        //    {
        //        Destroy(cellTransforms[i].gameObject);
        //    }

        //    for(int y = 0; y < height; y++)
        //    {
        //        for(int x = 0; x < width; x++)
        //        {
        //            //Setting up the cells' objects in the Unity scene
        //            GameObject cellGo = Instantiate(cellPrefab, new Vector3(x, 1, -y), Quaternion.identity, gridParent);
        //            Transform cellT = cellGo.transform;
        //            cellTransforms.Add(cellT);

        //            if(cellT.TryGetComponent(out CellVisualiser cVis))
        //            {
        //                cellVisualisers.Add(cVis);
        //                cVis.thisCell = OutputGrid.GridCells[x, y];
        //            }

        //            if(OutputGrid.GridCells[x, y].tileUsed)
        //            {
        //                Instantiate(OutputGrid.GridCells[x, y].tileUsed.Prefab, cellTransforms[(OutputGrid.GridCells[x, y].CellY * width) + OutputGrid.GridCells[x, y].CellX]);
        //            }
        //        }
        //    }
        //}

        #endregion
    }
}
