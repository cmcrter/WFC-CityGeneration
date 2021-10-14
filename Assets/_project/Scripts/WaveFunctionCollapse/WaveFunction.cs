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
using WFC.Rand;

namespace WFC
{
    public class WaveFunction : MonoBehaviour
    {
        #region Variables

        //The grid that this outputs
        [SerializeField]
        private Grid OutputGrid;

        //The grids' dimensions
        [SerializeField]
        int width;
        [SerializeField]
        int height;

        //This is the size of chunks to take from the input grid to create the rules (A value of 2 would mean 2 by 2 on the grid)
        [SerializeField]
        private int N;
        //The seed which generates each choice the algorithm will pick
        [SerializeField]
        private int seed;
        //How many times the algorithm will try
        [SerializeField]
        private int iterationLimit;
        //Whether the algorithm will consider rotations of tiles to be possible tiles
        [SerializeField]
        private bool includeRotations = false;
        //Will generate over time instead of instantly
        [SerializeField]
        private bool incremental = false;
        [SerializeField]
        private float generationSpeed = 0.1f;

        //The overarching coroutine the algorithm will be running in
        private Coroutine CoGenerating;

        //The random number generator
        Mersenne_Twister MTNumberGenerator;


        #endregion

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {
            //Getting patterns from InputGrid
            InputModel.Input.GeneratePatterns(N);
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
            yield return StartCoroutine(Co_GridPropogation());

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
            OutputGrid.GridCells[RandWidth, RandHeight].CollapseCell();
            yield return true;
        }

        //This is the bulk of the algorithm since it's the iteration loop it goes through
        private IEnumerator Co_GridPropogation()
        {
            //While not all cells are collapsed

            //Update the constraints (this is the actual propagation step)

            //Pick new cells to collapse (based on cells with lowest possibilities left or guess and record which parts it guessed) 

            //Collapse them

            //Does this work? If yes, repeat, if no, backtrack

            yield return true;
        }

        #endregion
    }
}
