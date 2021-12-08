////////////////////////////////////////////////////////////
// File: SoundManager.cs
// Author: Charles Carter
// Date Created: 07/12/2021
// Last Edited By: Charles Carter
// Date Last Edited: 07/12/2021
// Brief: A manager for all sounds in the scene to go through
//////////////////////////////////////////////////////////// 

using UnityEngine;
using WFC.Rand;

namespace WFC
{
    public class SoundManager : MonoBehaviour
    {
        #region Variables

        // Singleton instance
        public static SoundManager instance = null;

        // Audio Sources
        [SerializeField]
        private AudioSource SFXSource;
        [SerializeField]
        private AudioSource MusicSource;

        // Custom Randomizer
        private Mersenne_Twister randSounds;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if(instance != null)
            {
                Destroy(gameObject);
            }
            else 
            {
                instance = this;
            }

            randSounds = new Mersenne_Twister();
        }

        #endregion

        #region Public Methods

        public void PlaySFX(AudioClip clip, float upperRange)
        {
            float current = 1 / (1 + Mathf.Exp(randSounds.ReturnRandom()));
            Mathf.Clamp(current, 0.2f, upperRange);

            SFXSource.clip = clip;
            SFXSource.pitch = current;

            SFXSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            SFXSource.clip = clip;
            SFXSource.Play();
        }

        #endregion
    }
}
