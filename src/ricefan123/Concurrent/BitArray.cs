using System;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ricefan123.Concurrent.Collection
{
    public class ConcurrentBitArray : ICollection
    {
        private int[] array;

        const int BIT = 0b1;
        const int INT_SIZE = 32;
        const int INT_BYTES = 4;
        const int IDX_MASK = INT_SIZE - 1;

        public int Count { get; private set; }

        public int Length => Count;

        public bool IsSynchronized => true;

        public bool IsReadOnly => false;

        public object SyncRoot => null;

        public ConcurrentBitArray(int bitLength)
            : this(bitLength, false)
        {
        }

        public ConcurrentBitArray(int[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length > Int32.MaxValue / INT_SIZE)
                throw new ArgumentException("data overflown.", nameof(data));

            Init(data.Length * INT_SIZE);
            Buffer.BlockCopy(data, 0, array, 0, data.Length * INT_BYTES);
        }

        public ConcurrentBitArray(bool[] flags)
        {
            if (flags == null)
                throw new ArgumentNullException(nameof(flags));
            
            Init(flags.Length);

            for (int i = 0; i != flags.Length; ++i)
            {
                Set(i, flags[i]);
            }
        }

        public ConcurrentBitArray(int bitLength, bool defaultValue)
        {
            if (bitLength < 0)
                throw new ArgumentOutOfRangeException(nameof(bitLength), bitLength, "int cannot be negative");
            Init(bitLength);
            SetAll(defaultValue);
        }

        public ConcurrentBitArray(ConcurrentBitArray bits)
        {
            if (bits == null)
                throw new ArgumentNullException(nameof(bits));
            Init(bits.Length);
            Buffer.BlockCopy(bits.array, 0, array, 0, array.Length * INT_BYTES);
        }

        public bool this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        public bool Get(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            var (arrayIndex, subIndex) = GetIndices(index);
            var result = (AtomicRead(arrayIndex) & (BIT << subIndex)) != 0;
            return result;
        }

        public void Set(int index, bool value)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            var (arrayIndex, subIndex) = GetIndices(index);
            if (value)
                AtomicWriteOn(arrayIndex, subIndex);
            else
                AtomicWriteOff(arrayIndex, subIndex);
        }

        public void SetAll(bool value)
        {
            int slot = value ? ~0b0 : 0;
            if (array.Length == 0)
                return;
            for (int i = 0; i != (Length - 1) / INT_SIZE + 1; ++i)
            {
                array[i] = slot;
            }
        }

        private void Init(int bitLength)
        {
            array = new int[LengthRequired(bitLength, INT_SIZE)];
            Count = bitLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LengthRequired(int bitLength, int div)
        {
            return bitLength > 0 ? (bitLength - 1) / div + 1 : 0;
        }

        private static (int, int) GetIndices(int index)
        {
            return (index / INT_SIZE, index % INT_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AtomicRead(int arrayIndex)
            => Volatile.Read(ref array[arrayIndex]);

        private void AtomicWriteOn(int arrayIndex, int subIndex)
        {
            int slot, value;
            do 
            {
                slot = AtomicRead(arrayIndex);
                value = slot | (BIT << subIndex);
            }
            while (Interlocked.CompareExchange(ref array[arrayIndex], value, slot) != slot);
        }

        private void AtomicWriteOff(int arrayIndex, int subIndex)
        {
            int slot, value;
            do 
            {
                slot = AtomicRead(arrayIndex);
                value = slot & ~(BIT << subIndex);
            }
            while (Interlocked.CompareExchange(ref array[arrayIndex], value, slot) != slot);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i != Count; ++i)
                yield return Get(i);
        }
    }
}
