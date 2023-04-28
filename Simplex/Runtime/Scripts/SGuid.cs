using System;

using UnityEngine;


namespace Simplex
{
    [Serializable]
    public struct SGuid
    {
        [SerializeField] private int a;
        [SerializeField] private short b;
        [SerializeField] private short c;
        [SerializeField] private byte d;
        [SerializeField] private byte e;
        [SerializeField] private byte f;
        [SerializeField] private byte g;
        [SerializeField] private byte h;
        [SerializeField] private byte i;
        [SerializeField] private byte j;
        [SerializeField] private byte k;


        public SGuid(string guid) : this(new Guid(guid).ToByteArray()) { }
        public SGuid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
            this.g = g;
            this.h = h;
            this.i = i;
            this.j = j;
            this.k = k;
        }
        public SGuid(byte[] bytes)
        {
            if (bytes.IsEmpty()) throw new ArgumentException("Null or empty bytes array").Overwrite($"Failed generating {typeof(SGuid):type} from {bytes:ref}");

            this.a = (bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];
            this.b = (short)((bytes[5] << 8) | bytes[4]);
            this.c = (short)((bytes[7] << 8) | bytes[6]);
            this.d = bytes[8];
            this.e = bytes[9];
            this.f = bytes[10];
            this.g = bytes[11];
            this.h = bytes[12];
            this.i = bytes[13];
            this.j = bytes[14];
            this.k = bytes[15];
        }

        public override string ToString() => new Guid(a, b, c, d, e, f, g, h, i, j, k).ToString();
        public string ToString(string format) => new Guid(a, b, c, d, e, f, g, h, i, j, k).ToString(format);
        public override bool Equals(object obj) => obj is SGuid guid && Equals(guid);
        public bool Equals(SGuid guid)
        {
            if (guid.a != a) return false;
            if (guid.b != b) return false;
            if (guid.c != c) return false;
            if (guid.d != d) return false;
            if (guid.e != e) return false;
            if (guid.f != f) return false;
            if (guid.g != g) return false;
            if (guid.h != h) return false;
            if (guid.i != i) return false;
            if (guid.j != j) return false;
            if (guid.k != k) return false;

            return true;
        }
        public override int GetHashCode() => a ^ ((b << 16) | (ushort)c) ^ ((f << 24) | k);

        public static bool operator ==(SGuid a, SGuid b) => a.Equals(b);
        public static bool operator !=(SGuid a, SGuid b) => !a.Equals(b);

        public static implicit operator Guid(SGuid guid) => new Guid(guid.a, guid.b, guid.c, guid.d, guid.e, guid.f, guid.g, guid.h, guid.i, guid.j, guid.k);
        public static implicit operator SGuid(Guid guid) => new SGuid(guid.ToByteArray());
    }
}