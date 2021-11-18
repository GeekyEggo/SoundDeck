namespace SoundDeck.Core.Capture
{
    using System;

    /// <summary>
    /// Provides a circular buffer capable of storing elements in a fixed sized buffer.
    /// </summary>
    /// <typeparam name="T">The type of elements in the buffer.</typeparam>
    public class CircularBuffer<T>
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
        /// </summary>
        /// <param name="capacity">The number of elements the <see cref="CircularBuffer{T}"/> can store.</param>
        public CircularBuffer(int capacity)
            => this.Buffer = new T[capacity];

        /// <summary>
        /// Gets the capacity of the <see cref="CircularBuffer{T}"/>.
        /// </summary>
        public int Capacity => this.Buffer.Length;

        /// <summary>
        /// Gets the number of elements in the <see cref="CircularBuffer{T}"/>.
        /// </summary>
        public int Length { get; private set; } = 0;

        /// <summary>
        /// Gets or sets the current position of the <see cref="CircularBuffer{T}"/>.
        /// </summary>
        private int Position { get; set; } = 0;

        /// <summary>
        /// Gets or sets the array that contains the elements of the <see cref="CircularBuffer{T}"/>.
        /// </summary>
        private T[] Buffer { get; set; }

        /// <summary>
        /// Removes all elements from <see cref="CircularBuffer{T}"/>.
        /// </summary>
        public void Clear()
        {
            lock (this._syncRoot)
            {
                Array.Clear(this.Buffer, 0, this.Buffer.Length);
                this.Length = 0;
                this.Position = 0;
            }
        }

        /// <summary>
        /// Reads from the <see cref="CircularBuffer{T}"/> and copies the items to the <paramref name="target"/>; the position of the <see cref="CircularBuffer{T}"/> is unchanged.
        /// </summary>
        /// <param name="target">The target array at which to write data to.</param>
        /// <param name="offset">The offset to read the <see cref="CircularBuffer{T}"/> from.</param>
        /// <param name="count">The count of items to read.</param>
        /// <returns>The number of items read.</returns>
        public int Read(T[] target, int offset, int count)
        {
            lock (this._syncRoot)
            {
                if (this.Capacity == 0)
                {
                    return 0;
                }

                // Determine how many items can be read, and where they should be read from; this is relative to whether this instance's position has overflowed.
                count = Math.Min(count, this.Length - offset);
                offset = this.Length < this.Buffer.Length ? offset : this.Position + offset;
                offset %= this.Buffer.Length;

                // Read as much as possible before reaching the end of this instance.
                var readToEnd = Math.Min(count, this.Buffer.Length - offset);
                Array.Copy(this.Buffer, offset, target, 0, readToEnd);

                // Ensure the count is fulfilled, otherwise overflow back to zero, and read the rest.
                if (readToEnd != count)
                {
                    Array.Copy(this.Buffer, 0, target, readToEnd, count - readToEnd);
                }

                return count;
            }
        }

        /// <summary>
        /// Sets the capacity of the <see cref="CircularBuffer{T}"/>.
        /// </summary>
        /// <param name="capacity">The number of elements the <see cref="CircularBuffer{T}"/> can store.</param>
        public void SetCapacity(int capacity)
        {
            lock (this._syncRoot)
            {
                if (this.Buffer.Length != capacity)
                {
                    // Construct the new buffer, and determine the offset by the number of items currently stored within this instance relative to the new capacity.
                    var newBuffer = new T[capacity];
                    var offset = this.Length < capacity ? 0 : this.Length - capacity;

                    // Copy as much data as possible from this instance to the new buffer, and update everything.
                    this.Read(newBuffer, offset, capacity);

                    this.Buffer = newBuffer;
                    this.Length = Math.Min(this.Length, capacity);
                    this.Position = 0;
                }
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="source"/> to the <see cref="CircularBuffer{T}"/>; when the number of items to write exceeds the <see cref="Capacity"/>, the tail-end of the <paramref name="source"/> is written.
        /// </summary>
        /// <param name="source">The source array that contains the data.</param>
        public void Write(T[] source)
            => this.Write(source, 0, source.Length);

        /// <summary>
        /// Writes the specified <paramref name="source"/> to the <see cref="CircularBuffer{T}"/>; when the number of items to write exceeds the <see cref="Capacity"/>, the tail-end of the <paramref name="source"/> is written.
        /// </summary>
        /// <param name="source">The source array that contains the data.</param>
        /// <param name="offset">The offset of the <paramref name="source"/> at which to begin reading from.</param>
        /// <param name="count">The count of items to write.</param>
        public void Write(T[] source, int offset, int count)
        {
            lock (this._syncRoot)
            {
                if (this.Capacity == 0)
                {
                    return;
                }

                // When the number of items to write exceeds the capacity, reduce to prevent unncessary writes.
                count = Math.Min(count, source.Length - offset);
                if (count >= this.Buffer.Length)
                {
                    this.Position = 0;

                    offset += count - this.Buffer.Length;
                    count = this.Buffer.Length;
                }

                // Write as much as possible before reaching the end of this instance.
                var writeToEnd = Math.Min(count, this.Buffer.Length - this.Position);
                Array.Copy(source, offset, this.Buffer, this.Position, writeToEnd);

                if (writeToEnd == count)
                {
                    // When everything was written successfully without overflowing, update the position.
                    this.Position += writeToEnd;
                    this.Position %= this.Buffer.Length;
                }
                else
                {
                    // We were unable to write all of the data before reaching the end of the buffer, so write the remaining data at position zero.
                    Array.Copy(source, writeToEnd, this.Buffer, 0, count - writeToEnd);
                    this.Position = count - writeToEnd;
                }

                this.Length = Math.Min(this.Buffer.Length, this.Length + count);
            }
        }
    }
}
