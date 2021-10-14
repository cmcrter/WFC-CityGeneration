////////////////////////////////////////////////////////////
// File: InputModel
// Author: Charles Carter
// Date Created: 14/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 14/10/21
// Brief: The script which serializes the user's example grid
//////////////////////////////////////////////////////////// 

using UnityEngine;

namespace WFC
{
    public class InputModel : MonoBehaviour
    {
        #region Variables

        public static InputModel Input;

        [SerializeField]
        private Grid CurrentGrid;
        public Grid Model;

        #endregion

        #region UnityMethods

        private void Awake()
        {
            Input = this;
            Model = CurrentGrid;
        }

        #endregion

        #region Public Methods

        //This essentially records each cells' neighbours and makes patterns out of them
        public void GeneratePatterns(int N)
        {

        }

        #endregion

        #region Private Methods
        #endregion
    }

}