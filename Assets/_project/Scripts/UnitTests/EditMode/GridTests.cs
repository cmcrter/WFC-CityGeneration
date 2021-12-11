////////////////////////////////////////////////////////////
// File: GridTests.cs
// Author: Charles Carter
// Date Created: 11/12/21   
// Last Edited By: Charles Carter
// Date Last Edited: 11/12/21
// Brief: Some simple tests to make sure basic functions are working
//////////////////////////////////////////////////////////// 

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WFC;

namespace WFC.Tests
{
    public class GridTests
    {
        #region Example Methods

        // A Test behaves as an ordinary method
        [Test]
        public void GridTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator GridTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        #endregion

        #region Test Methods

        //Making a reference grid
        Grid ExampleGridCreation()
        {
            Grid gridToReturn = new Grid(3, 3);
            return gridToReturn;
        }

        [Test]
        public void NeighboursTest()
        {
            Grid gridToUse = ExampleGridCreation();

            //Getting the neighbours of the centre cell, the grid should be 0, 1, 2
                                                                          //3, 4, 5
                                                                          //6, 7, 8
            //So the x and y would be 1 and 1, since they start from 0
            Tuple<List<Cell>, List<Vector2>> CentreNeighbourhood = gridToUse.GetNeighbours(1, 1);
            Tuple<List<Cell>, List<Vector2>> TopLeftNeighbourhood = gridToUse.GetNeighbours(0, 0);
            Tuple<List<Cell>, List<Vector2>> TopRightNeighbourhood = gridToUse.GetNeighbours(2, 0);
            Tuple<List<Cell>, List<Vector2>> BottomLeftNeighbourhood = gridToUse.GetNeighbours(0, 2);
            Tuple<List<Cell>, List<Vector2>> BottomRightNeighbourhood = gridToUse.GetNeighbours(2, 2);

            Assert.AreEqual(CentreNeighbourhood.Item1.Count, 4);
            Assert.AreEqual(TopLeftNeighbourhood.Item1.Count, 2);
            Assert.AreEqual(TopRightNeighbourhood.Item1.Count, 2);
            Assert.AreEqual(BottomLeftNeighbourhood.Item1.Count, 2);
            Assert.AreEqual(BottomRightNeighbourhood.Item1.Count, 2);
        }

        #endregion
    }
}
