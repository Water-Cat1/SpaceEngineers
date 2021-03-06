﻿using System;

namespace VRageMath.PackedVector
{
    /// <summary>
    /// Packed vector type containing four 16-bit floating-point values.
    /// </summary>
    public struct HalfVector4 : IPackedVector<ulong>, IPackedVector, IEquatable<HalfVector4>
    {
        private ulong packedValue;

        /// <summary>
        /// Directly gets or sets the packed representation of the value.
        /// </summary>
        public ulong PackedValue
        {
            get
            {
                return this.packedValue;
            }
            set
            {
                this.packedValue = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the HalfVector4 class.
        /// </summary>
        /// <param name="x">Initial value for the x component.</param><param name="y">Initial value for the y component.</param><param name="z">Initial value for the z component.</param><param name="w">Initial value for the w component.</param>
        public HalfVector4(float x, float y, float z, float w)
        {
            this.packedValue = HalfVector4.PackHelper(x, y, z, w);
        }

        /// <summary>
        /// Initializes a new instance of the HalfVector4 structure.
        /// </summary>
        /// <param name="vector">A vector containing the initial values for the components of the HalfVector4 structure.</param>
        public HalfVector4(Vector4 vector)
        {
            this.packedValue = HalfVector4.PackHelper(vector.X, vector.Y, vector.Z, vector.W);
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="a">The object to the left of the equality operator.</param><param name="b">The object to the right of the equality operator.</param>
        public static bool operator ==(HalfVector4 a, HalfVector4 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="a">The object to the left of the equality operator.</param><param name="b">The object to the right of the equality operator.</param>
        public static bool operator !=(HalfVector4 a, HalfVector4 b)
        {
            return !a.Equals(b);
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            this.packedValue = HalfVector4.PackHelper(vector.X, vector.Y, vector.Z, vector.W);
        }

        private static ulong PackHelper(float vectorX, float vectorY, float vectorZ, float vectorW)
        {
            return (ulong)HalfUtils.Pack(vectorX) | (ulong)HalfUtils.Pack(vectorY) << 16 | (ulong)HalfUtils.Pack(vectorZ) << 32 | (ulong)HalfUtils.Pack(vectorW) << 48;
        }

        /// <summary>
        /// Expands the packed representation into a Vector4.
        /// </summary>
        public Vector4 ToVector4()
        {
            Vector4 vector4;
            vector4.X = HalfUtils.Unpack((ushort)this.packedValue);
            vector4.Y = HalfUtils.Unpack((ushort)(this.packedValue >> 16));
            vector4.Z = HalfUtils.Unpack((ushort)(this.packedValue >> 32));
            vector4.W = HalfUtils.Unpack((ushort)(this.packedValue >> 48));
            return vector4;
        }

        /// <summary>
        /// Returns a string representation of the current instance.
        /// </summary>
        public override string ToString()
        {
            return this.ToVector4().ToString();
        }

        /// <summary>
        /// Gets the hash code for the current instance.
        /// </summary>
        public override int GetHashCode()
        {
            return this.packedValue.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object with which to make the comparison.</param>
        public override bool Equals(object obj)
        {
            if (obj is HalfVector4)
                return this.Equals((HalfVector4)obj);
            else
                return false;
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="other">The object with which to make the comparison.</param>
        public bool Equals(HalfVector4 other)
        {
            return this.packedValue.Equals(other.packedValue);
        }
    }
}
