using System;
using System.Threading;

namespace Rem.Threading;

/// <summary>
/// A lock that can be locked asynchronously.
/// </summary>
/// <remarks>
/// Unlike standard .NET locking, this class can be locked synchronously.
/// The class includes <see cref="Lock(CancellationToken)"/> and <see cref="LockAsync(CancellationToken)"/> methods,
/// both of which return handles that can be disposed of to unlock the lock, as well as an explicit
/// <see cref="Unlock"/> method. This allows an asynchronous lock that can be guaranteed to be released, even if an
/// exception is thrown.
/// </remarks>
public sealed class AsyncLock : IDisposable
{
    /// <summary>
    /// Gets a semaphore that is used to provide the lock.
    /// </summary>
    private SemaphoreSlim Semaphore { get; } = new(1);

    /// <summary>
    /// Locks the <see cref="AsyncLock"/> synchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException">This instance has already been disposed of.</exception>
    public Handle Lock(CancellationToken cancellationToken = default)
    {
        Semaphore.Wait(cancellationToken);
        return new Handle(this);
    }

    /// <summary>
    /// Locks the <see cref="AsyncLock"/> asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task representing the work.</returns>
    /// <exception cref="ObjectDisposedException">This instance has already been disposed of.</exception>
    public async Task<Handle> LockAsync(CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new Handle(this);
    }

    /// <summary>
    /// Unlocks the <see cref="AsyncLock"/>.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This instance has already been disposed of.</exception>
    public void Unlock() => Semaphore.Release();

    /// <summary>
    /// Disposes of all resources used by the <see cref="AsyncLock"/>.
    /// </summary>
    public void Dispose() => Semaphore.Dispose();

    /// <summary>
    /// A disposable handle representing locking a <see cref="AsyncLock"/>.
    /// </summary>
    public readonly struct Handle : IDisposable
    {
        private AsyncLock Lock { get; }

        internal Handle(AsyncLock Lock) { this.Lock = Lock; }

        /// <summary>
        /// Releases this handle, unlocking the <see cref="AsyncLock"/> that was locked when creating this instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// The <see cref="AsyncLock"/> has already been disposed of.
        /// </exception>
        public void Dispose() => Lock?.Unlock();
    }
}
