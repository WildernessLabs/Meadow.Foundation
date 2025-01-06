using System;
using System.Text;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// This FIFO buffer is implemented as a circular buffer. (Standard computer science stuff)
/// It operates on bytes. The only special method is WriteString,
/// which writes a string to the buffer after converting it with Encoding.ASCII.GetBytes().
/// </summary>
public class FifoBuffer
{
    private readonly byte[] _buffer;
    private int _nextRead;
    private int _nextWrite;
    private int _count;

    /// <summary>
    /// The number of bytes that can be stored in the buffer.
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    /// The number of bytes currently stored in the buffer. Increases with Write() and decreases with Read().
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Create a FIFO byte buffer with a specified capacity.
    /// The buffer is circular, so it will reuse the allocated space.
    /// Write operations will consume space in the buffer, and read operations will free up space.
    /// If you try to write more data than its capacity, it will throw an error.
    /// </summary>
    /// <param name="capacity">The size of the circular buffer, containing bytes.</param>
    /// <exception cref="ArgumentException"></exception>
    public FifoBuffer(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentException("Capacity must be greater than 0.", nameof(capacity));

        _buffer = new byte[capacity];
        _nextRead = 0;  // The next read position.
        _nextWrite = 0; // The next write position.
        _count = 0;     // The number of items in the buffer.
    }

    /// <summary>
    /// Clear/empty the FIFO buffer.
    /// </summary>
    public void Clear()
    {
        _nextRead = 0;  // The read position.
        _nextWrite = 0; // The write position.
        _count = 0;     // The number of items in the buffer.
    }

    /// <summary>
    /// Add new item to the _nestWrite position in the buffer.
    /// Then move the _nextWrite position. If it goes beyond the capacity, it will wrap around to 0.
    /// If you try to write more data than the capacity, it will throw an error.
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Write(byte item)
    {
        if (_count == Capacity)
            throw new InvalidOperationException("The buffer is full.");

        _buffer[_nextWrite] = item;
        _nextWrite = (_nextWrite + 1) % Capacity;   // Move _nextWrite forward and possibly wrap.
        _count++;
    }

    /// <summary>
    /// Method to remove and return the oldest item. The one at the _nextRead position.
    /// The _nextRead position will be moved. If it goes beyond the capacity, it will wrap around to 0.
    /// If you try to read from an empty buffer, it will throw an error.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public byte Read()
    {
        if (_count == 0)
            throw new InvalidOperationException("The buffer is empty.");

        var item = _buffer[_nextRead];
        _nextRead = (_nextRead + 1) % Capacity;   // Move _nextRead forward and possibly wrap.
        _count--;
        return item;
    }

    /// <summary>
    /// Peek at element at index, relative to the _nextRead position.
    /// The byte at relative index 0 is the oldest one, so looking at [0] is the same as Peek().
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public byte this[int index]
    {
        get
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _buffer[(_nextRead + index) % Capacity];
        }
    }

    /// <summary>
    /// Method to just return the oldest item. The one at the _nextRead position. (Withoit removing it)
    /// If the buffer is empty, it will throw an error.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public byte Peek()
    {
        if (_count == 0)
            throw new InvalidOperationException("The buffer is empty.");

        return _buffer[_nextRead];
    }

    /// <summary>
    /// Bonus method to add a string to the buffer.
    /// Each character in the string will be converted to a byte and written to the buffer.
    /// This method can be usefull if you need to tag some data written to the buffer with a string.
    /// </summary>
    /// <param name="text"></param>
    public void WriteString(string text)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        foreach (var c in text)
        {
            Write((byte)c);
        }
    }
}
