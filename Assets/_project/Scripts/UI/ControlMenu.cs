////////////////////////////////////////////////////////////
// File: ControlMenu.cs
// Author: Charles Carter
// Date Created: 10/11/21
// Last Edited By: Charles Carter
// Date Last Edited: 11/12/21
// Brief: The user interface for running the program
//////////////////////////////////////////////////////////// 

using UnityEngine;
using UnityEngine.UI;
using WFC;

namespace WFC.UI
{
    public class ControlMenu : MonoBehaviour
    {
        #region Public Fields

        [SerializeField]
        private WaveFunction waveFunction;

        [SerializeField]
        private Button PlayButton;
        [SerializeField]
        private Toggle PauseButton;
        [SerializeField]
        private Button RestartButton;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            
        }

        private void Start()
        {

        }

        private void Update()
        {

        }

        #endregion

        #region Public Methods

        public void RunProgram()
        {
            PlayButton.interactable = false;
            PauseButton.interactable = true;

            if(waveFunction)
            {
                waveFunction.RunAlgorithm();
            }
        }

        public void Restart()
        {
            if(waveFunction)
            {
                waveFunction.RestartAlgorithm();
            }
        }

        public void Pause()
        {
            if(waveFunction)
            {
                waveFunction.PauseAlgorithm();
            }
        }

        public void OpenGithub()
        {
            Application.OpenURL("https://github.com/cmcrter/WFC-CityGeneration");
        }

        public void Close()
        {
            Application.Quit();
        }

        #endregion


        #region Private Methods

        #endregion
    }

}