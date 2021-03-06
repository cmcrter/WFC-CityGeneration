////////////////////////////////////////////////////////////
// File: ControlMenu.cs
// Author: Charles Carter
// Date Created: 10/11/21
// Last Edited By: Charles Carter
// Date Last Edited: 14/12/21
// Brief: The user interface for running the program
//////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WFC;
using WFC.Editor;

namespace WFC.UI
{
    #region Other Data Structures

    //Enum to set up IDs
    public enum PresetIDs
    {
        ParkPreset = 0,
        ResidentialPreset = 1,
        BusinessPreset = 2,
    }

    //A class to more easily compact the presets
    [Serializable]
    public class TilePreset
    {
        //Although I'm not using it, keeping a constructor for future potential
        public TilePreset(Toggle toggle, InputModelCompiler compiler, PresetIDs ID)
        {
            presetID = ID;
            presetTiles = compiler;
            presetToggle = toggle;
        }

        public PresetIDs presetID;
        public Toggle presetToggle;
        public InputModelCompiler presetTiles;
    }

    #endregion

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

        //Keeping track if it's ran before
        private bool bRan;

        [Header("Customization Variables")]
        //The customization aspects
        [SerializeField]
        private TMPro.TMP_InputField seedInput;
        [SerializeField]
        private Toggle PropagationToggle;

        //Presets (which rely on the inspector being set up currently)
        [SerializeField]
        private Toggle cityToggle;
        [SerializeField]
        private List<TilePreset> PresetOptions;

        //The compiler which fills in the adjacency rules between the sections
        //To improve this, have more tiles that can transition the areas, specifically to go between difficult tiles (This would require a designer)
        [SerializeField]
        private InputModelCompiler seamsCompiler;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            //Making sure there's one compiler active if others turn off (when possible)
            bool bMissingCompiler = false;
            bool bToggleOn = false;
            int iPresetFailCounter = 0;

            //Going through and automatically setting up the toggles if the compiler isn't set
            for(int i = 0; i < PresetOptions.Count; ++i)
            {
                if(PresetOptions[i].presetTiles == null)
                {
                    if(PresetOptions[i].presetToggle != null)
                    {
                        iPresetFailCounter++;
                        PresetOptions[i].presetToggle.interactable = false;
                        bMissingCompiler = true;
                    }
                }
                else if(!bToggleOn)
                {
                    PresetOptions[i].presetTiles.LoadComp();

                    if(bMissingCompiler && PresetOptions[i].presetToggle != null)
                    {
                        PresetOptions[i].presetToggle.isOn = true;
                        ForcePreset(i);
                        bToggleOn = true;
                    }
                }
                else
                {
                    PresetOptions[i].presetTiles.LoadComp();
                }
            }

            //Checking if the city toggle is fine to be on
            if(iPresetFailCounter < 1)
            {
                cityToggle.isOn = true;
                CityPreset(true);
            }
            else
            {
                cityToggle.interactable = false;
            }

            //Preparing for potential city
            if(seamsCompiler != null)
            {
                seamsCompiler.LoadComp();
            }

            //Setting the text of the input field
            if(waveFunction)
            {
                seedInput.text = waveFunction.GetSeed().ToString();
            }
            
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The functions that are ran from the User Interface
        /// </summary>
        public void RunProgram()
        {           
            PauseToggle.interactable = true;

            if(waveFunction)
            {
                int.TryParse(seedInput.text, out int newSeed);

                if(waveFunction.GetSeed() != newSeed || !bRan)
                {
                    waveFunction.SetSeed(newSeed);

                    if(Debug.isDebugBuild)
                    {
                        Debug.Log("Ran with seed: " + newSeed);
                    }
                }
                else
                {
                    return;
                }

                EntropyShownChanged(true);
                waveFunction.RunAlgorithm();
                bRan = true;
            }
        }

        //Rerun is from the next seed, restart is from the first seed used, and restart with seed is from what is currently set as the seed
        public void Rerun()
        {
            PauseToggle.isOn = false;

            if(waveFunction)
            {
                EntropyShownChanged(true);
                waveFunction.RerunAlgorithm();
            }
        }

        public void Restart()
        {
            PauseToggle.isOn = false;

            if(waveFunction)
            {
                EntropyShownChanged(true);
                waveFunction.RestartAlgorithm();
            }
        }

        public void RestartSeed()
        {
            PauseToggle.isOn = false;

            if(waveFunction)
            {
                EntropyShownChanged(true);
                waveFunction.RunAlgorithm();
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

        /// <summary>
        /// Preset Functions
        /// </summary>

        //The city preset is all the other presets combined, where the grid needs to be partitioned
        public void CityPreset(bool isOn)
        {
            if(isOn && waveFunction)
            {
                CheckPresets();

                if(PresetOptions.Count > 0)
                {
                    waveFunction.SetPreset(PresetOptions);
                }
                else if (cityToggle != null)
                {
                    cityToggle.interactable = false;
                }
            }
        }

        //The other presets just need to pass through their relevant tilesets
        public void ParkPreset(bool isOn)
        {
            if(isOn && waveFunction && PresetOptions[(int)PresetIDs.ParkPreset] != null)
            {
                if(PresetOptions[(int)PresetIDs.ParkPreset].presetTiles != null)
                {
                    PresetOptions[(int)PresetIDs.ParkPreset].presetTiles.LoadComp();
                    waveFunction.SetPreset(PresetOptions[(int)PresetIDs.ParkPreset].presetTiles);
                }
            }
        }

        public void BusinessPreset(bool isOn)
        {
            if(isOn && waveFunction && PresetOptions[(int)PresetIDs.BusinessPreset] != null)
            {
                if(PresetOptions[(int)PresetIDs.BusinessPreset].presetTiles != null)
                {
                    PresetOptions[(int)PresetIDs.BusinessPreset].presetTiles.LoadComp();
                    waveFunction.SetPreset(PresetOptions[(int)PresetIDs.BusinessPreset].presetTiles);
                }
            }
        }

        public void ResidentialPreset(bool isOn)
        {
            if(isOn && waveFunction && PresetOptions[(int)PresetIDs.ResidentialPreset] != null)
            {
                if(PresetOptions[(int)PresetIDs.ResidentialPreset].presetTiles != null)
                {
                    PresetOptions[(int)PresetIDs.ResidentialPreset].presetTiles.LoadComp();
                    waveFunction.SetPreset(PresetOptions[(int)PresetIDs.ResidentialPreset].presetTiles);
                }
            }
        }

        #endregion

        #region Private Methods

        private void CheckPresets()
        {
            for(int i = 0; i < PresetOptions.Count; ++i)
            {
                if(PresetOptions[i].presetTiles == null)
                {
                    PresetOptions.RemoveAt(i);
                    CheckPresets();
                }
            }
        }

        private void ForcePreset(int ID)
        {
            if(waveFunction)
            {
                if(PresetOptions[ID].presetTiles != null)
                {
                    waveFunction.SetPreset(PresetOptions[ID].presetTiles);
                }
            }
        }

        #endregion
    }
}