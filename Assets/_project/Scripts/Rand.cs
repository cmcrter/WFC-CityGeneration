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

        Mersenne_Twister(int seed)
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
            int i;
            UInt32 current;

            for (i = 0; i < diff; i++)
            {
                current = ((State [i] & 0x80000000) | (State[i + 1] & 0x7fffffff));
                State[i] = (State[i + Period] ^ (current >> 1) ^ ((current & 1) * BitMask));
            }

            // remaining words (except the very last one)
            for (; i < Size - 1; i++)
            {
                current = (State[i] & 0x80000000) | (State[i + 1] & 0x7fffffff);
                State[i] = (State [i - diff] ^ ( current >> 1 ) ^ ( ( current & 1 ) * BitMask));
            }

            // last word is computed pretty much the same way, but i + 1 must wrap around to 0
            current = (State[i] & 0x80000000) | (State[0] & 0x7fffffff);
            State [i] = (State[Period - 1] ^ (current >> 1) ^ ((current & 1) * BitMask));

            // word used for next random number
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

        //The mathematical function to return the random number
        protected override int Noise()
        {
            //This is an XORShift in it's most basic form with a period of 2^128 -1
            t = x ^ ( x << 11 );
            x = y; y = z; z = w;
            return w = w ^ ( w >> 19 ) ^ ( t ^ ( t >> 8 ) );
        }
    }
}
