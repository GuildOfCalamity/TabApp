using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments.AppointmentsProvider;

namespace TabApp.Redemption;

internal class GrowableArray<T>
{
    private T[] _array;

    public GrowableArray(int initialCount = 10)
    {
        _array = new T[initialCount];
    }

    public int Count => this._array.Length;

    public T this[int index]
    {
        get
        {
            if (index > this._array.Length - 1)
                return (T)default;
            return this._array[index];
        }
        set
        {
            this.EnsureOrGrow(index);
            this._array[index] = value;
        }
    }

    public void Add(T item)
    {
        var insert = this._array.Length;
        var newArray = new T[this._array.Length + 1];
        Array.Copy(this._array, 0, newArray, 0, this._array.Length);
        this._array = newArray;
        this._array[insert] = item;
    }

    public void ShrinkBy(int amount)
    {
        if (amount <= 0 || this._array.Length == 0)
            return;

        var newLength = this._array.Length - amount;
        if (newLength > 0)
        {
            var newArray = new T[newLength];
            Array.Copy(this._array, 0, newArray, 0, newLength);
            this._array = newArray;
        }
        else
            Debug.WriteLine("[WARNING] Amount exceeds current capacity.");
    }

    void EnsureOrGrow(int index)
    {
        if (index > this._array.Length - 1)
        {
            var newArray = new T[Math.Max(this._array.Length * 2, index + 1)];
            Array.Copy(this._array, 0, newArray, 0, this._array.Length);
            this._array = newArray;
        }
    }

    public override string ToString()
    {
        string format = "[{0,-4}] ⇨ {1,-30}{2}"; //negative left-justifies, positive right-justifies
        StringBuilder sb = new StringBuilder();
        sb.Append($"{nameof(GrowableArray<T>)}(Length=").Append(Count).Append(")").AppendLine();
        for (int i = 0; i < Count; i++)
        {
            sb.Append(String.Format(format, $"{i:D4}", $"{this._array[i]}", Environment.NewLine));
        }
        return sb.AppendLine().ToString();
    }
}

public class GrowableArrayTest
{
    public static void PerformTest()
    {
        GrowableArray<int> ga = new(10);
        Debug.WriteLine($"{ga.Count}");
        ga.Add(123);
        var val1 = ga[10];      // returns 123
        var val2 = ga[11];      // returns (int)default
        ga[11] = 321;              // grows array
        Debug.WriteLine(ga.ToString()); // shows override output
        ga.ShrinkBy(ga.Count / 2);
        Debug.WriteLine(ga.ToString()); // show truncation
    }
}