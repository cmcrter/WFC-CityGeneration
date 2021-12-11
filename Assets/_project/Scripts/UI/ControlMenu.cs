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
        #region Variables

        [Header("The main part of the functionality")]
        [SerializeField]
        private WaveFunction waveFunction;
        [SerializeField]
        private SoundManager soundManager;

        //Main Functionality
        [SerializeField]
        private Button PlayButton;
        [SerializeField]
        private Toggle PauseToggle;
        [SerializeField]
        private Button RestartButton;

        [Header("Customization Variables")]
        //The customization aspects
        [SerializeField]
        private TMPro.TMP_InputField seedInput;
        [SerializeField]
        private Toggle PropagationToggle;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            
        }

        private void Start()
        {

        }

        #endregion

        #region Public Methods

        public void RunProgram()
        {
            PlayButton.interactable = false;
            PauseToggle.interactable = true;

            if(waveFunction)
            {
                if(seedInput.text != "")
                {
                    int.TryParse(seedInput.text, out int newSeed);
                    waveFunction.SetSeed(newSeed);
                }

                waveFunction.RunAlgorithm();
            }
        }

        public void Restart()
        {
            PauseToggle.isOn = false;

            if(waveFunction)
            {
                if(seedInput.text != "")
                {
                    int.TryParse(seedInput.text, out int newSeed);
                    waveFunction.SetSeed(newSeed);
                }

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

        public void BruteForceChanged(bool isOn)
        {
            if(waveFunction)
            {
                waveFunction.SetBruteForce(isOn);
            }
        }

        public void EntropyShownChanged(bool isOn)
        {
            if(waveFunction)
            {
                waveFunction.SetEntropyShown(isOn);
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