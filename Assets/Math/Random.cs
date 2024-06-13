using UnityEditor;
using UnityEngine;

namespace LSMath
{
    public class Random
    {
        IRngImplement impl = new MT19937();

        public ulong Seed
        {
            get => impl.GetSeed();
            set => impl.SetSeed(value);
        }

        /// <summary>
        /// Returns a random number reside in [0.0, 1.0)
        /// </summary>
        /// <returns></returns>
        public float Norm01()
        {
            const float DIV = 0x10_0000;
            return (Int31() % DIV) / DIV;
        }
        /// <summary>
        /// Returns a random number reside in [<paramref name="from"/>, <paramref name="to"/>)
        /// </summary>
        /// <returns></returns>
        public int Range(int from, int to)
        {
            if (from == to) return from;
            return (Int31() % (to - from)) + from;
        }

        /// <summary>
        /// Returns a random number reside in [0, <paramref name="to"/>)
        /// </summary>
        /// <returns></returns>
        public int Range(int to)
        {
            return Range(0, to);
        }

        /// <summary>
        /// Returns a random number reside in [from, to)
        /// </summary>
        /// <returns></returns>
        public float Range(float from, float to)
        {
            return ((Int31() >> 20) % (to - from)) + from;
        }

        int Int31()
        {
            return (int)(impl.UInt64() >> 33);
        }
    }

    interface IRngImplement
    {
        void SetSeed(ulong seed);
        ulong GetSeed();

        ulong UInt64();
    }

    /// <summary>
    /// This code was posted by arithma http://www.lebgeeks.com/forums/viewtopic.php?pid=60482#p60482
    /// </summary>
    class MT19937 : IRngImplement
    {
        /* Period parameters */
        private const int N = 312;
        private const int M = 156;
        private const ulong MATRIX_A = 0xB5026F5AA96619E9; /* constant vector a */
        private const ulong UPPER_MASK = 0xFFFFFFFF80000000; /* most significant w-r bits */
        private const ulong LOWER_MASK = 0x7FFFFFFF; /* least significant r bits */

        /* Tempering parameters */
        private const ulong TEMPERING_MASK_B = 0x9d2c5680;
        private const ulong TEMPERING_MASK_C = 0xefc60000;

        private static ulong TEMPERING_SHIFT_U(ulong y) { return (y >> 29); }
        private static ulong TEMPERING_SHIFT_S(ulong y) { return (y << 17); }
        private static ulong TEMPERING_SHIFT_T(ulong y) { return (y << 37); }
        private static ulong TEMPERING_SHIFT_L(ulong y) { return (y >> 43); }

        //static unsigned long mt[N]; /* the array for the state vector  */
        private ulong[] mt = new ulong[N];

        // static int mti=N+1; /* mti==N+1 means mt[N] is not initialized */
        private uint mti = N + 1; /* mti==N+1 means mt[N] is not initialized */

        public MT19937()
        {
            SetSeed(5489); /* a default initial seed is used   */
        }
        public ulong GetSeed() => mt[0];
        /* initializing the array with a NONZERO seed */
        public void SetSeed(ulong seed)
        {
            /* setting initial seeds to mt[N] using         */
            /* the generator Line 25 of Table 1 in          */
            /* [KNUTH 1981, The Art of Computer Programming */
            /*    Vol. 2 (2nd Ed.), pp102]                  */

            mt[0] = seed;
            for (mti = 1; mti < N; mti++)
            {
                mt[mti] = (6364136223846793005 * (mt[mti - 1] ^ (mt[mti - 1] >> 62)) + mti);
            }
        }

        /* initialize by an array with array-length */
        /* init_key is the array for initializing keys */
        /* key_length is its length */
        public void SetSeed(ulong[] init_key)
        {
            ulong i, j, k;
            ulong key_length = (ulong)init_key.Length;
            SetSeed(19650218UL);
            i = 1; j = 0;
            k = (N > key_length ? N : key_length);
            for (; k > 0; k--)
            {
                mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 62)) * 3935559000370003845UL)) + init_key[j] + j; /* non linear */
                i++; j++;
                if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                if (j >= key_length) j = 0;
            }
            for (k = N - 1; k > 0; k--)
            {
                mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 62)) * 2862933555777941757UL)) - i; /* non linear */
                i++;
                if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
            }

            mt[0] = 1UL << 63; /* MSB is 1; assuring non-zero initial array */
        }

        private static ulong[/* 2 */] mag01 = { 0x0, MATRIX_A };
        /* generating reals */
        /* unsigned long */
        /* for integer generation */
        public ulong UInt64()
        {
            ulong y;

            /* mag01[x] = x * MATRIX_A  for x=0,1 */
            if (mti >= N) /* generate N words at one time */
            {
                short kk;

                // if (mti == N + 1) /* if sgenrand() has not been called, */
                // {
                //     SetSeed(5489); /* a default initial seed is used   */
                // }

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
                }

                for (; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1];
                }

                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];

                mti = 0;
            }

            y = mt[mti++];
            y ^= (y >> 29) & 0x5555555555555555;
            y ^= (y << 17) & 0x71D67FFFEDA60000;
            y ^= (y << 37) & 0xFFF7EEE000000000;
            y ^= (y >> 43);

            return y;
        }

        /* generates a random number on [0, 2^63-1]-interval */
        public long Int63()
        {
            return (long)(this.UInt64() >> 1);
        }

        /* generates a random number on [0,1]-real-interval */
        public double Real1()
        {
            return (this.UInt64() >> 11) * (1.0 / 9007199254740991.0);
        }

        /* generates a random number on [0,1)-real-interval */
        public double Real2()
        {
            return (double)(this.UInt64() >> 11) * (1.0 / 9007199254740992.0);
        }

        /* generates a random number on (0,1)-real-interval */
        public double Real3()
        {
            return ((double)(this.UInt64() >> 12) + 0.5) * (1.0 / 4503599627370496.0);
        }
    }
}