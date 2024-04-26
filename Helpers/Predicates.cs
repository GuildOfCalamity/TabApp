using System;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;

namespace TabApp.Helpers;

/** This module is required for DependencyObjectExtensions.cs **/

#region [Interface]
/// <summary>
/// An interface representing a predicate for items of a given type.
/// </summary>
/// <typeparam name="T">The type of items to match.</typeparam>
internal interface IPredicate<in T> where T : class
{
    /// <summary>
    /// Performs a match with the current predicate over a target <typeparamref name="T"/> instance.
    /// </summary>
    /// <param name="element">The input element to match.</param>
    /// <returns>Whether the match evaluation was successful.</returns>
    bool Match(T element);
}
#endregion

#region [Structs]
/// <summary>
/// An <see cref="IPredicate{T}"/> type matching all instances of a given type.
/// </summary>
/// <typeparam name="T">The type of items to match.</typeparam>
internal readonly struct PredicateByAny<T> : IPredicate<T> where T : class
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Match(T element)
    {
        return true;
    }
}

/// <summary>
/// An <see cref="IPredicate{T}"/> type matching items of a given type.
/// </summary>
internal readonly struct PredicateByType : IPredicate<object>
{
    /// <summary>
    /// The type of element to match.
    /// </summary>
    private readonly Type type;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateByType"/> struct.
    /// </summary>
    /// <param name="type">The type of element to match.</param>
    public PredicateByType(Type type)
    {
        this.type = type;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Match(object element)
    {
        return element.GetType() == this.type;
    }
}

/// <summary>
/// An <see cref="IPredicate{T}"/> type matching items of a given type.
/// </summary>
/// <typeparam name="T">The type of items to match.</typeparam>
internal readonly struct PredicateByFunc<T> : IPredicate<T> where T : class
{
    /// <summary>
    /// The predicatee to use to match items.
    /// </summary>
    private readonly Func<T, bool> predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateByFunc{T}"/> struct.
    /// </summary>
    /// <param name="predicate">The predicatee to use to match items.</param>
    public PredicateByFunc(Func<T, bool> predicate)
    {
        this.predicate = predicate;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Match(T element)
    {
        return this.predicate(element);
    }
}

/// <summary>
/// An <see cref="IPredicate{T}"/> type matching items of a given type.
/// </summary>
/// <typeparam name="T">The type of items to match.</typeparam>
/// <typeparam name="TState">The type of state to use when matching items.</typeparam>
internal readonly struct PredicateByFunc<T, TState> : IPredicate<T> where T : class
{
    /// <summary>
    /// The state to give as input to <see name="predicate"/>.
    /// </summary>
    private readonly TState state;

    /// <summary>
    /// The predicatee to use to match items.
    /// </summary>
    private readonly Func<T, TState, bool> predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateByFunc{T, TState}"/> struct.
    /// </summary>
    /// <param name="state">The state to give as input to <paramref name="predicate"/>.</param>
    /// <param name="predicate">The predicatee to use to match items.</param>
    public PredicateByFunc(TState state, Func<T, TState, bool> predicate)
    {
        this.state = state;
        this.predicate = predicate;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Match(T element)
    {
        return this.predicate(element, state);
    }
}

/// <summary>
/// An <see cref="IPredicate{T}"/> type matching <see cref="FrameworkElement"/> instances by name.
/// </summary>
internal readonly struct PredicateByName : IPredicate<FrameworkElement>
{
    /// <summary>
    /// The name of the element to look for.
    /// </summary>
    private readonly string name;

    /// <summary>
    /// The comparison type to use to match <see name="name"/>.
    /// </summary>
    private readonly StringComparison comparisonType;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateByName"/> struct.
    /// </summary>
    /// <param name="name">The name of the element to look for.</param>
    /// <param name="comparisonType">The comparison type to use to match <paramref name="name"/>.</param>
    public PredicateByName(string name, StringComparison comparisonType)
    {
        this.name = name;
        this.comparisonType = comparisonType;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Match(FrameworkElement element)
    {
        return element.Name.Equals(this.name, this.comparisonType);
    }
}
#endregion
