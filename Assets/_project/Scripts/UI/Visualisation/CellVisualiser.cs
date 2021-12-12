﻿////////////////////////////////////////////////////////////
// File: CellVisualiser.cs
// Author: Charles Carter
// Date Created: 21/10/21   
// Last Edited By: Charles Carter
// Date Last Edited: 07/12/21
// Brief: The script to show a cell on a GameObject
//////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using UnityEngine;

namespace WFC.UI
{
    public class CellVisualiser : MonoBehaviour
    {
        #region Variables

        [Header("Necessary Variables")]
        public Cell thisCell;
        public TextMesh EntropyText;

        //The children of this script will always be the possible tiles, in a miniature form
        //private PotentialTile visualiser;

        private List<Transform> potentialTilesTransforms;

        [Header("Prefabs for text / potential tiles parent")]
        [SerializeField]
        private GameObject goEntropyText;
        [SerializeField]
        private GameObject goPotentialTilesParent;

        [Header("Turning aspects on/off")]
        private bool bShowEntropy = true;
        private bool bShowPotentialTiles = true;

        [SerializeField]
        private List<Color> sectionColours;

        #endregion

        public void UpdateVisuals()
        {
            if(bShowEntropy)
            {
                UpdateEntropyText();
            }

            if(bShowPotentialTiles)
            {
                UpdatePotentialTiles();
            }
        }

        public void SetTextColour(int colourIndex)
        {
            if(EntropyText != null)
            {
                if(colourIndex < sectionColours.Count)
                {
                    EntropyText.color = sectionColours[colourIndex];
                }
            }
        }

        private void UpdateEntropyText()
        {
            if(EntropyText != null)
            {
                if(thisCell.currentEntropy > 0)
                {
                    EntropyText.text = thisCell.currentEntropy.ToString("F2");
                }
                else
                {
                    EntropyText.text = "0";
                }
            }
        }

        private void UpdatePotentialTiles()
        {
            //visualiser.UpdateChildren(thisCell.possibleTiles);
        }
    }
}