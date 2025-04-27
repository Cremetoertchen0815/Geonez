#region LICENSE

//-----------------------------------------------------------------------------
// For the purpose of making video games, educational projects or gamification,
// GeonBit is distributed under the MIT license and is totally free to use.
// To use this source code or GeonBit as a whole for other purposes, please seek 
// permission from the library author, Ronen Ness.
// 
// Copyright (c) 2017 Ronen Ness [ronenness@gmail.com].
// Do not remove this license notice.
//-----------------------------------------------------------------------------

#endregion

#region File Description

//-----------------------------------------------------------------------------
// Implements basic resizeable array.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------

#endregion

using System.Buffers;

namespace Nez.GeonBit;

/// <summary>
///     An array you can add elements to, but still access the internal array object.
///     Internal array needs to be returned manually or via <see cref="Return" />
/// </summary>
/// <typeparam name="T">Type to store in array.</typeparam>
internal class ResizableRentedArray<T>
{
    /// <summary>
    ///     Items array.
    /// </summary>
    private T[] m_array;

    /// <summary>
    ///     Items count.
    /// </summary>
    private int m_count;

    /// <summary>
    ///     Create the resizable array with default starting size.
    /// </summary>
    /// <param name="initialCapacity">Optional initial starting size.</param>
    public ResizableRentedArray(int? initialCapacity = null)
    {
        m_array = ArrayPool<T>.Shared.Rent(initialCapacity ?? 4);
    }

    /// <summary>
    ///     Get the internal array.
    /// </summary>
    public T[] InternalArray => m_array;

    /// <summary>
    ///     Get array real size.
    /// </summary>
    public int Count => m_count;

    /// <summary>
    ///     Clear the array.
    /// </summary>
    public void Clear()
    {
        m_count = 0;
        for (var i = 0; i < m_array.Length; i++) m_array[i] = default;
    }

    /// <summary>
    ///     Returns the internal array to the array pool.
    /// </summary>
    public void Return()
    {
        ArrayPool<T>.Shared.Return(m_array);
        m_array = null;
    }

    /// <summary>
    ///     Remove the extra buffer from array and resize it to actual size.
    /// </summary>
    public void Trim()
    {
        Resize(ref m_array, m_count);
    }

    /// <summary>
    ///     Add element to array.
    /// </summary>
    /// <param name="element">Element to add.</param>
    public void Add(T element)
    {
        // check if need to enlarge array
        if (m_count == m_array.Length) Resize(ref m_array, m_array.Length * 2);

        // add to array and increase count
        m_array[m_count++] = element;
    }

    /// <summary>
    ///     Add range of values to array.
    /// </summary>
    /// <param name="values"></param>
    public void AddRange(T[] values)
    {
        foreach (var val in values) Add(val);
    }

    private void Resize(ref T[] old, int nuSize)
    {
        var nuArr = ArrayPool<T>.Shared.Rent(nuSize);
        for (var i = 0; i < old.Length; i++) nuArr[i] = old[i];
        ArrayPool<T>.Shared.Return(old);
        old = nuArr;
    }
}