using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ricefan123.Concurrent.Collection;

namespace ricefan123.Concurrent.Tests
{
    public static class ConcurrentBitArray_GetSetTests
    {
        private const int BitsPerByte = 8;
        private const int BitsPerInt32 = 32;

        public static IEnumerable<object[]> Get_Set_Data()
        {
            foreach (int size in new[] { 0, 1, BitsPerByte, BitsPerByte * 2, BitsPerInt32, BitsPerInt32 * 2 })
            {
                foreach (bool def in new[] { true, false })
                {
                    yield return new object[] { def, Enumerable.Repeat(true, size).ToArray() };
                    yield return new object[] { def, Enumerable.Repeat(false, size).ToArray() };
                    yield return new object[] { def, Enumerable.Range(0, size).Select(i => i % 2 == 1).ToArray() };
                }
            }
        }

        [Theory]
        [MemberData(nameof(Get_Set_Data))]
        public static void Get_Set(bool def, bool[] newValues)
        {
            ConcurrentBitArray bitArray = new ConcurrentBitArray(newValues.Length, def);
            for (int i = 0; i < newValues.Length; i++)
            {
                bitArray.Set(i, newValues[i]);
                Assert.Equal(newValues[i], bitArray[i]);
                Assert.Equal(newValues[i], bitArray.Get(i));
            }
        }

        [Fact]
        public static void Get_InvalidIndex_ThrowsArgumentOutOfRangeException()
        {
            ConcurrentBitArray bitArray = new ConcurrentBitArray(4);
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bitArray.Get(-1));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bitArray.Get(bitArray.Length));

            Assert.Throws<ArgumentOutOfRangeException>("index", () => bitArray[-1]);
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bitArray[bitArray.Length]);
        }

        [Fact]
        public static void Set_InvalidIndex_ThrowsArgumentOutOfRangeException()
        {
            ConcurrentBitArray bitArray = new ConcurrentBitArray(4);
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bitArray.Set(-1, true));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bitArray.Set(bitArray.Length, true));

            Assert.Throws<ArgumentOutOfRangeException>("index", () => bitArray[-1] = true);
            Assert.Throws<ArgumentOutOfRangeException>("index", () => bitArray[bitArray.Length] = true);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(BitsPerByte, true)]
        [InlineData(BitsPerByte, false)]
        [InlineData(BitsPerByte + 1, true)]
        [InlineData(BitsPerByte + 1, false)]
        [InlineData(BitsPerInt32, true)]
        [InlineData(BitsPerInt32, false)]
        [InlineData(BitsPerInt32 + 1, true)]
        [InlineData(BitsPerInt32 + 1, false)]
        public static void SetAll(int size, bool defaultValue)
        {
            ConcurrentBitArray bitArray = new ConcurrentBitArray(size, defaultValue);
            bitArray.SetAll(!defaultValue);
            for (int i = 0; i < bitArray.Length; i++)
            {
                Assert.Equal(!defaultValue, bitArray[i]);
                Assert.Equal(!defaultValue, bitArray.Get(i));
            }

            bitArray.SetAll(defaultValue);
            for (int i = 0; i < bitArray.Length; i++)
            {
                Assert.Equal(defaultValue, bitArray[i]);
                Assert.Equal(defaultValue, bitArray.Get(i));
            }
        }

        public static IEnumerable<object[]> GetEnumerator_Data()
        {
            foreach (int size in new[] { 0, 1, BitsPerByte, BitsPerByte + 1, BitsPerInt32, BitsPerInt32 + 1 })
            {
                foreach (bool lead in new[] { true, false })
                {
                    yield return new object[] { Enumerable.Range(0, size).Select(i => lead ^ (i % 2 == 0)).ToArray() };
                }
            }
        }

        public static IEnumerable<object[]> Length_Set_Data()
        {
            int[] sizes = { 1, BitsPerByte, BitsPerByte + 1, BitsPerInt32, BitsPerInt32 + 1 };
            foreach (int original in sizes.Concat(new[] { 16384 }))
            {
                foreach (int n in sizes)
                {
                    yield return new object[] { original, n };
                }
            }
        }

        [Fact]
        public static void SyncRoot()
        {
            ICollection bitArray = new ConcurrentBitArray(10);
            Assert.Same(bitArray.SyncRoot, bitArray.SyncRoot);
            Assert.Same(bitArray.SyncRoot, ((ICollection)new ConcurrentBitArray(10)).SyncRoot);
        }
    }
}