////////////////////////////////////////////////////////////
// File: RandTester.cs
// Author: Charles Carter
// Date Created: 13/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 13/10/21
// Brief: A script to visualise the random number generators made in Rand.cs
//////////////////////////////////////////////////////////// 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace WFC.Rand
{
    public class RandTester : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private List<TextMeshProUGUI> CounterTexts = new List<TextMeshProUGUI>();
        private int[] CounterInts;

        private Coroutine CoLCG;
        private Coroutine CoMT;
        private Coroutine CoXOR;

        [SerializeField]
        private int seed;

        #endregion

        #region Unity Methods

        void Start()
        {
            CounterInts = new int[CounterTexts.Count];
            ResetCounters();
        }

        #endregion

        #region Public Methods

        public void ResetCounters()
        {
            if(CoLCG != null)
            {
                StopCoroutine(CoLCG);
            }

            if(CoMT != null)
            {
                StopCoroutine(CoMT);
            }

            if(CoXOR != null)
            {
                StopCoroutine(CoXOR);
            }

            for (int i = 0; i < CounterTexts.Count; ++i)
            {
                CounterInts[i] = 0;
                CounterTexts[i].text = "0";
            }
        }

        public void RunLCG()
        {
            ResetCounters();
            CoLCG = StartCoroutine(Co_RunLCG());
        }

        public void RunMT()
        {
            ResetCounters();         
            CoMT = StartCoroutine(Co_RunMT());
        }

        public void RunXOR()
        {
            ResetCounters();
            CoXOR = StartCoroutine(Co_RunXOR());
        }

        #endregion

        #region Private Methods

        private IEnumerator Co_RunLCG()
        {
            LCG lcg = new LCG(seed);

            while(Time.timeScale == 1)
            {
                int counterToIncrement = lcg.ReturnRandom(10);
                counterToIncrement = Mathf.Abs(counterToIncrement);

                if(counterToIncrement < CounterInts.Length && counterToIncrement >= 0)
                {
                    CounterInts[counterToIncrement]++;
                    CounterTexts[counterToIncrement].text = CounterInts[counterToIncrement].ToString();
                }
                else if (Debug.isDebugBuild)
                {
                    Debug.Log(counterToIncrement);
                }

                yield return null;
            }

            CoLCG = null;
            yield return true;
        }

        private IEnumerator Co_RunMT()
        {
            Mersenne_Twister MT = new Mersenne_Twister(seed);

            while (Time.timeScale == 1)
            {
                int counterToIncrement = MT.ReturnRandom(10);
                counterToIncrement = Mathf.Abs(counterToIncrement);

                if (counterToIncrement < CounterInts.Length && counterToIncrement >= 0)
                {
                    CounterInts[counterToIncrement]++;
                    CounterTexts[counterToIncrement].text = CounterInts[counterToIncrement].ToString();
                }
                else if (Debug.isDebugBuild)
                {
                    Debug.Log(counterToIncrement);
                }

                yield return null;
            }

            CoMT = null;
            yield return true;
        }

        private IEnumerator Co_RunXOR()
        {
            //This version of XOR doesn't really need a seed
            XORShift XOR = new XORShift();

            while(Time.timeScale == 1)
            {
                int counterToIncrement = XOR.ReturnRandom(10);
                counterToIncrement = Mathf.Abs(counterToIncrement);

                if(counterToIncrement < CounterInts.Length && counterToIncrement >= 0)
                {
                    CounterInts[counterToIncrement]++;
                    CounterTexts[counterToIncrement].text = CounterInts[counterToIncrement].ToString();
                }
                else if (Debug.isDebugBuild)
                {
                    Debug.Log(counterToIncrement);
                }

                yield return null;
            }

            CoXOR = null;
            yield return true;
        }

        #endregion
    }
}