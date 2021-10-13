////////////////////////////////////////////////////////////
// File: Rand.cs
// Author: Charles Carter
// Date Created: 13/10/21
// Last Edited By: Charles Carter
// Date Last Edited: 13/10/21
// Brief: A class to contain all the necessary random number generators
//////////////////////////////////////////////////////////// 

using System;

namespace WFC.Rand
{
    [Serializable]
    //This will be the default class I use to return numbers
    public class Rand
    {
        #region Variables

        //The seed is set with a default
        protected int seed= 133321;

        #endregion

        #region Constructors

        public Rand()
        {

        }

        //Generally I want a seed
        public Rand(int newSeed)
        {
            seed = newSeed;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Setting the seed to retrieve a random number from
        /// </summary>
        public void SetSeed(int newSeed)
        {
            seed = newSeed;
        }

        /// <summary>
        /// Gets a random number
        /// </summary>
        public int ReturnRandom()
        {
            return Noise();
        }

        /// <summary>
        /// Gets a random number within a given maximum
        /// </summary>
        public int ReturnRandom(int MaxNumber)
        {
            return Noise() % MaxNumber;
        }

        #endregion

        #region Protected Methods

        //The mathematical function to return the random number
        protected virtual int Noise()
        {
            return seed;
        }

        #endregion
    }

    public class Mersenne_Twister : Rand
    {
        //State Size
        const int Size = 624;
        //Shift Size
        const int Period = 397;
        //Difference
        const int diff = Size - Period;
        //Mask Bits
        const int MaskSize = 30;
        //XORMask
        const UInt32 BitMask = 0x9908b0df;

        /// internal state
        UInt32[] State = new UInt32[Size];
        int next;

        public Mersenne_Twister()
        {
            //Setting the values up for the first states
            State[0] = (UInt32)seed;
            for(int i = 1; i < Size; i++)
            {
                UInt32 v = (uint)(1812433253UL * (State[i - 1] ^ (State[i - 1] >> MaskSize)));
                State[i] = (uint)(v + i);
            }

            //Starting the chain
            Twist();
        }

        public Mersenne_Twister(int seed)
        {
            State[0] = (UInt32)seed;
            for (int i = 1; i < Size; i++)
            {
                UInt32 v = (uint)(1812433253UL * (State [i - 1] ^ (State [i - 1] >> MaskSize)));
                State [i] = (uint)(v + i);
            }

            //Starting the chain
            Twist();
        }

        //The mathematical function to return the random number
        protected override int Noise()
        {
            //Is it necessary to twist?
            if (next >= Size)
            {
                Twist();
            }

            //Get next state
            UInt32 x = State[next++];

            //Doing a small XORShift on the state to make it seem more random
            x ^= x >> 11;
            x ^= ( x << 7 ) & 0x9d2c5680;
            x ^= ( x << 15 ) & 0xefc60000;
            x ^= x >> 18;

            return (int)x;
        }

        //Computing the states again
        private void Twist()
        {
            int i = 0;
            UInt32 current;

            //The first half of states
            for (i = 0; i < diff; i++)
            {
                current = ((State [i] & 0x80000000) | (State[i + 1] & 0x7fffffff));
                State[i] = (State[i + Period] ^ (current >> 1) ^ ((current & 1) * BitMask));
            }

            //Remaining States
            for (int j = i; j < Size - 1; j++)
            {
                current = (State[j] & 0x80000000) | (State[j + 1] & 0x7fffffff);
                State[j] = (State [j - diff] ^ ( current >> 1 ) ^ ( ( current & 1 ) * BitMask));
            }

            i = State.Length - 1;

            //The last state is done seperately which will wrap around
            current = (State[i] & 0x80000000) | (State[0] & 0x7fffffff);
            State [i] = (State[Period - 1] ^ (current >> 1) ^ ((current & 1) * BitMask));

            //Starting the cycle again
            next = 0;
        }
    }

    //A simple XORShift class
    public class XORShift : Rand
    {
        int x = 123456789;
        int y = 362436069;
        int z = 521288629;
        int w = 88675123;
        int t;

        public XORShift()
        {
            
        }

        //The mathematical function to return the random number
        protected override int Noise()
        {
            //This is an XORShift in it's most basic form with a period of 2^128 -1
            t = x ^ ( x << 11 );
            x = y; y = z; z = w;
            
            return w = w ^ (w >> 19) ^ (t ^ (t >> 8));
        }
    }

    //A simple Linear Congruential Generator
    public class LCG : Rand
    {
        public LCG(int newSeed)
        {
            seed = newSeed;
        }

        protected override int Noise()
        {
            //This is the Microsoft way of doing the LCG
            return ((seed = 214013 * seed + 2531011) & int.MaxValue) >> 16;
        }
    } 
}
