using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ricefan123.Concurrent.Collection;

namespace ricefan123.Concurrent.Tests
{
    public static class ConcurrentBitArray_CtorTests
    {
        private const int BitsPerByte = 8;
        private const int BitsPerInt32 = 32;

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(BitsPerByte)]
        [InlineData(BitsPerByte * 2)]
        [InlineData(BitsPerInt32)]
        [InlineData(BitsPerInt32 * 2)]
        [InlineData(200)]
        [InlineData(65551)]
        public static void Ctor_Int(int length)
        {
            ConcurrentBitArray bitArray = new ConcurrentBitArray(length);
            Assert.Equal(length, bitArray.Length);
            for (int i = 0; i < bitArray.Length; i++)
            {
                Assert.False(bitArray[i]);
                Assert.False(bitArray.Get(i));
            }
            ICollection collection = bitArray;
            Assert.Equal(length, collection.Count);
            Assert.True(collection.IsSynchronized);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(BitsPerByte, true)]
        [InlineData(BitsPerByte, false)]
        [InlineData(BitsPerByte * 2, true)]
        [InlineData(BitsPerByte * 2, false)]
        [InlineData(BitsPerInt32, true)]
        [InlineData(BitsPerInt32, false)]
        [InlineData(BitsPerInt32 * 2, true)]
        [InlineData(BitsPerInt32 * 2, false)]
        [InlineData(200, true)]
        [InlineData(200, false)]
        [InlineData(65551, true)]
        [InlineData(65551, false)]
        public static void Ctor_Int_Bool(int length, bool defaultValue)
        {
            ConcurrentBitArray bitArray = new ConcurrentBitArray(length, defaultValue);
            Assert.Equal(length, bitArray.Length);
            for (int i = 0; i < bitArray.Length; i++)
            {
                Assert.Equal(defaultValue, bitArray[i]);
                Assert.Equal(defaultValue, bitArray.Get(i));
            }
            ICollection collection = bitArray;
            Assert.Equal(length, collection.Count);
            Assert.True(collection.IsSynchronized);
        }

        [Fact]
        public static void Ctor_Int_NegativeLength_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("bitLength", () => new ConcurrentBitArray(-1));
            Assert.Throws<ArgumentOutOfRangeException>("bitLength", () => new ConcurrentBitArray(-1, false));
        }

        public static IEnumerable<object[]> Ctor_BoolArray_TestData()
        {
            yield return new object[] { new bool[0] };
            foreach (int size in new[] { 1, BitsPerByte, BitsPerByte * 2, BitsPerInt32, BitsPerInt32 * 2 })
            {
                yield return new object[] { Enumerable.Repeat(true, size).ToArray() };
                yield return new object[] { Enumerable.Repeat(false, size).ToArray() };
                yield return new object[] { Enumerable.Range(0, size).Select(x => x % 2 == 0).ToArray() };
            }
        }

        [Theory]
        [MemberData(nameof(Ctor_BoolArray_TestData))]
        public static void Ctor_BoolArray(bool[] values)
        {
            ConcurrentBitArray bitArray = new ConcurrentBitArray(values);
            Assert.Equal(values.Length, bitArray.Length);
            for (int i = 0; i < bitArray.Length; i++)
            {
                Assert.Equal(values[i], bitArray[i]);
                Assert.Equal(values[i], bitArray.Get(i));
            }
            ICollection collection = bitArray;
            Assert.Equal(values.Length, collection.Count);
            Assert.True(collection.IsSynchronized);
        }

        [Fact]
        public static void Ctor_NullBoolArray_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("flags", () => new ConcurrentBitArray((bool[])null));
        }

        public static IEnumerable<object[]> Ctor_ConcurrentBitArray_TestData()
        {
            yield return new object[] { "bool[](empty)", new ConcurrentBitArray(new bool[0]) };
            yield return new object[] { "int[](empty)", new ConcurrentBitArray(new int[0]) };

            foreach (int size in new[] { 1, BitsPerByte, BitsPerByte * 2, BitsPerInt32, BitsPerInt32 * 2 })
            {
                yield return new object[] { "length", new ConcurrentBitArray(size) };
                yield return new object[] { "length|default(true)", new ConcurrentBitArray(size, true) };
                yield return new object[] { "length|default(false)", new ConcurrentBitArray(size, false) };
                yield return new object[] { "bool[](all)", new ConcurrentBitArray(Enumerable.Repeat(true, size).ToArray()) };
                yield return new object[] { "bool[](none)", new ConcurrentBitArray(Enumerable.Repeat(false, size).ToArray()) };
                yield return new object[] { "bool[](alternating)", new ConcurrentBitArray(Enumerable.Range(0, size).Select(x => x % 2 == 0).ToArray()) };
                if (size >= BitsPerInt32)
                {
                    yield return new object[] { "int[](all)", new ConcurrentBitArray(Enumerable.Repeat(unchecked((int)0xffffffff), size / BitsPerInt32).ToArray()) };
                    yield return new object[] { "int[](none)", new ConcurrentBitArray(Enumerable.Repeat(0x00000000, size / BitsPerInt32).ToArray()) };
                    yield return new object[] { "int[](alternating)", new ConcurrentBitArray(Enumerable.Repeat(unchecked((int)0xaaaaaaaa), size / BitsPerInt32).ToArray()) };
                }
            }
        }

        [Theory]
        [MemberData(nameof(Ctor_ConcurrentBitArray_TestData))]
        public static void Ctor_ConcurrentBitArray(string label, ConcurrentBitArray bits)
        {
            ConcurrentBitArray bitArray = new ConcurrentBitArray(bits);
            Assert.Equal(bits.Length, bitArray.Length);
            for (int i = 0; i < bitArray.Length; i++)
            {
                Assert.Equal(bits[i], bitArray[i]);
                Assert.Equal(bits[i], bitArray.Get(i));
            }
            ICollection collection = bitArray;
            Assert.Equal(bits.Length, collection.Count);
            Assert.True(collection.IsSynchronized);
        }

        [Fact]
        public static void Ctor_NullConcurrentBitArray_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("bits", () => new ConcurrentBitArray((ConcurrentBitArray)null));
        }

        public static IEnumerable<object[]> Ctor_IntArray_TestData()
        {
            yield return new object[] { new int[0], new bool[0] };
            foreach (int size in new[] { 1, 10 })
            {
                yield return new object[] { Enumerable.Repeat(unchecked((int)0xffffffff), size).ToArray(), Enumerable.Repeat(true, size * BitsPerInt32).ToArray() };
                yield return new object[] { Enumerable.Repeat(0x00000000, size).ToArray(), Enumerable.Repeat(false, size * BitsPerInt32).ToArray() };
                yield return new object[] { Enumerable.Repeat(unchecked((int)0xaaaaaaaa), size).ToArray(), Enumerable.Range(0, size * BitsPerInt32).Select(i => i % 2 == 1).ToArray() };
            }
        }

        [Theory]
        [MemberData(nameof(Ctor_IntArray_TestData))]
        public static void Ctor_IntArray(int[] array, bool[] expected)
        {
            ConcurrentBitArray bitArray = new ConcurrentBitArray(array);
            Assert.Equal(expected.Length, bitArray.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], bitArray[i]);
                Assert.Equal(expected[i], bitArray.Get(i));
            }
            ICollection collection = bitArray;
            Assert.Equal(expected.Length, collection.Count);
            Assert.True(collection.IsSynchronized);
        }

        [Fact]
        public static void Ctor_NullIntArray_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("data", () => new ConcurrentBitArray((int[])null));
        }

        [Fact]
        public static void Ctor_LargeIntArrayOverflowingConcurrentBitArray_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("data", () => new ConcurrentBitArray(new int[int.MaxValue / BitsPerInt32 + 1 ]));
        }

        [Fact]
        public static void Ctor_Simple_Method_Tests()
        {
            int length = 0;
            ConcurrentBitArray bitArray = new ConcurrentBitArray(length);

            Assert.Null(bitArray.SyncRoot);
            Assert.True(bitArray.IsSynchronized);
            Assert.False(bitArray.IsReadOnly);
        }
    }
}