using System;
using System.Diagnostics;
using System.Threading;

namespace TabApp;

/// <summary>
/// A fancy locking class which uses <see cref="ReaderWriterLockSlim"/>. 
/// Offers baked-in prevention of recursive locks.
/// </summary>
/// <remarks>
/// A reader lock is not the same as a writer lock.
/// </remarks>
public class SlimLock : IDisposable
{
    bool _disposing = false;
    readonly ReaderWriterLockSlim lockObj;

    /// <summary>
    /// Creates a new instance of the <see cref="SlimLock"/> class
    /// </summary>
    /// <returns>new <see cref="SlimLock"/> object</returns>
    public static SlimLock Create() => new SlimLock();

    /// <summary>
    /// Constructor
    /// </summary>
    SlimLock()
    {
        lockObj = new ReaderWriterLockSlim();
    }

    /// <summary>
    /// Enter read mode
    /// </summary>
    public void EnterRead()
    {
        try { lockObj?.EnterReadLock(); }
        catch (LockRecursionException) { throw new Exception("Recursive locks aren't supported."); }
    }

    /// <summary>
    /// Exit read mode
    /// </summary>
    public void ExitRead()
    {
        try { lockObj?.ExitReadLock(); }
        catch (SynchronizationLockException) { throw new Exception("Synchronization error during lock exit."); }
    }

    /// <summary>
    /// Enter write mode
    /// </summary>
    public void EnterWrite()
    {
        try { lockObj?.EnterWriteLock(); }
        catch (LockRecursionException) { throw new Exception("Recursive locks aren't supported."); }

    }

    /// <summary>
    /// Exit write mode
    /// </summary>
    public void ExitWrite()
    {
        try { lockObj?.ExitWriteLock(); }
        catch (SynchronizationLockException) { throw new Exception("Synchronization error during lock exit."); }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            if (_disposing)
                return;

            _disposing = true;

            if (lockObj != null)
                lockObj.Dispose();
        }
        catch (Exception ex) { Debug.WriteLine($"Dispose: {ex.Message}", nameof(SlimLock)); }
    }
}
