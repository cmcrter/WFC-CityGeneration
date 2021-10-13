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

        [Header("Randomness Customization")]
        [SerializeField]
        private int seed;

        [Header("Visualisation Variables")]
        [SerializeField]
        private List<TextMeshProUGUI> CounterTexts = new List<TextMeshProUGUI>();
        [SerializeField]
        private GameObject cameraObj;
        [SerializeField]
        private List<Transform> InitialCounterPlacementParents = new List<Transform>();
        [SerializeField]
        private List<GameObject> LatestCounterPlacements = new List<GameObject>();
        private int[] CounterInts;

        private Coroutine CoLCG;
        private Coroutine CoMT;
        private Coroutine CoXOR;

        private int biggestCounter = 0;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            cameraObj = cameraObj ?? Camera.main.gameObject;
        }

        void Start()
        {
            CounterInts = new int[CounterTexts.Count];
            ResetCounters();
            cameraObj.transform.position = new Vector3(0, LatestCounterPlacements[0].transform.position.y, 5.5f);
        }

        #endregion

        #region Public Methods

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

        private void ResetCounters()
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

            for(int i = 0; i < InitialCounterPlacementParents.Count; ++i)
            {
                Transform firstChild = InitialCounterPlacementParents[i].GetChild(0);

                foreach(Transform child in InitialCounterPlacementParents[i])
                {
                    if(child != firstChild)
                    {
                        Destroy(child.gameObject);
                    }
                }

                LatestCounterPlacements[i] = firstChild.gameObject;
            }

            biggestCounter = 0;

            for(int i = 0; i < CounterTexts.Count; ++i)
            {
                CounterInts[i] = 0;
                CounterTexts[i].text = "0";
            }
        }

        private void IncrementCounter(int counterToIncrement)
        {
            counterToIncrement = Mathf.Abs(counterToIncrement);

            if(counterToIncrement < CounterInts.Length && counterToIncrement >= 0)
            {
                CounterInts[counterToIncrement]++;
                CounterTexts[counterToIncrement].text = CounterInts[counterToIncrement].ToString();

                //Adjusting height of tower
                GameObject go = GameObject.Instantiate(LatestCounterPlacements[counterToIncrement], InitialCounterPlacementParents[counterToIncrement]);
                go.transform.position = LatestCounterPlacements[counterToIncrement].transform.position + new Vector3(0, LatestCounterPlacements[counterToIncrement].transform.localScale.y, 0);
                go.name = CounterInts[counterToIncrement].ToString();
                LatestCounterPlacements[counterToIncrement] = go;

                //Putting the camera in a good position to see the top towers
                int checkVal = CounterInts[0];
                for (int i = 0; i < LatestCounterPlacements.Count; ++i)
                {
                    if (CounterInts[i] > checkVal)
                    {
                        checkVal = CounterInts[i];
                        biggestCounter = i;
                    }
                }

                cameraObj.transform.position = new Vector3(0, LatestCounterPlacements[biggestCounter].transform.position.y, 5.5f);
            }
            else if(Debug.isDebugBuild)
            {
                Debug.Log(counterToIncrement);
            }
        }

        private IEnumerator Co_RunLCG()
        {
            LCG lcg = new LCG(seed);

            while(Time.timeScale == 1)
            {
                int counterToIncrement = lcg.ReturnRandom(10);
                counterToIncrement = Mathf.Abs(counterToIncrement);
                IncrementCounter(counterToIncrement);
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
                IncrementCounter(counterToIncrement);
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
                IncrementCounter(counterToIncrement);
                yield return null;
            }

            CoXOR = null;
            yield return true;
        }

        #endregion
    }
}