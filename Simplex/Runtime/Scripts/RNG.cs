using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

using UnityEngine;


namespace Simplex
{
    public class RNG
    {
        public static RNG Generic = new RNG();
        public static RNG Seeded = new RNG();

        public readonly System.Random random;
        public readonly int seed;


        public RNG() : this(UnityEngine.Random.Range(0, int.MaxValue)) { }
        public RNG(string seed)
        {
            if (string.IsNullOrEmpty(seed))
            {
                ConsoleUtilities.Warn($"RNG should not be instantiated with null or empty seed");
                this.seed = UnityEngine.Random.Range(0, int.MaxValue);
            }
            else
            {
                using SHA256 sha256 = SHA256.Create();
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(seed));
                this.seed = BitConverter.ToInt32(hash, 0);
            }

            this.random = new System.Random(this.seed);
        }
        public RNG(int seed)
        {
            this.seed = seed;
            this.random = new System.Random(this.seed);
        }

        public bool Bool() => random.Next(2) == 0;
        public int Int(int min, int max) => random.Next(min, max);
        public float Float(float min, float max) => (float)random.NextDouble() * (max - min) + min;
        public double Double(double min, double max) => random.NextDouble() * (max - min) + min;

        public T From<T>(T[] array) => (array.IsEmpty()) ? throw new IndexOutOfRangeException("Cannot get random item from empty array") : array[random.Next(array.Length)];
        public T From<T>(List<T> list) => (list.IsEmpty()) ? throw new IndexOutOfRangeException("Cannot get random item from empty list") : list[random.Next(list.Count)];
    }
}