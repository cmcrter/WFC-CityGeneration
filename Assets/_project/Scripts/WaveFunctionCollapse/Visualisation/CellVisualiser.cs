////////////////////////////////////////////////////////////
// File: CellVisualiser.cs
// Author: Charles Carter
// Date Created: 21/10/21   
// Last Edited By: Charles Carter
// Date Last Edited: 03/12/21
// Brief: The script to show a cell on a GameObject
//////////////////////////////////////////////////////////// 

using UnityEngine;
using UnityEditor;
using TMPro;

namespace WFC.Editor
{
    public class CellVisualiser : MonoBehaviour
    {
        public Cell thisCell;
        public TextMeshProUGUI EntropyText;
        
        //The children of this script will always be the possible tiles, in a miniature form
        //private PotentialTile visualiser;

        public void UpdateVisuals()
        {
            
        }

        private void UpdateEntropyText()
        {
            EntropyText.text = thisCell.currentEntropy.ToString();
        }

        private void UpdatePotentialTiles()
        {
            //visualiser.UpdateChildren(thisCell.possibleTiles);
        }
    }
}