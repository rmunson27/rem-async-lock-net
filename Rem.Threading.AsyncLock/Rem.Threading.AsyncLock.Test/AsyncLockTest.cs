namespace Rem.Threading.Test;

/// <summary>
/// Tests the <see cref="AsyncLock"/> class.
/// </summary>
[TestClass]
public class AsyncLockTest
{
    /// <summary>
    /// Tests locking and unlocking of the async lock using the return values of the
    /// <see cref="AsyncLock.Lock(CancellationToken)"/> method.
    /// </summary>
    [TestMethod]
    public void TestSynchronousLocking()
    {
        using AsyncLock asyncLock = new();

        using (asyncLock.Lock())
        {
            // Can't get the lock within the other lock - the attempt will be canceled after 1 second
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(1));
            Assert.ThrowsException<OperationCanceledException>(() => asyncLock.Lock(cts.Token));
        }

        using (asyncLock.Lock()) { } // This should allow the lock to be acquired, as it was disposed of
    }

    /// <summary>
    /// Asynchronously tests locking and unlocking of the async lock using the return values of the
    /// <see cref="AsyncLock.LockAsync(CancellationToken)"/> method.
    /// </summary>
    /// <returns>A task representing the work.</returns>
    [TestMethod]
    public async Task TestAsynchronousLocking()
    {
        using AsyncLock asyncLock = new();

        using (await asyncLock.LockAsync().ConfigureAwait(false))
        {
            // Can't get the lock within the other lock - the attempt will be canceled after 1 second
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(1));
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                            async () => await asyncLock.LockAsync(cts.Token).ConfigureAwait(false))
                    .ConfigureAwait(false);
        }

        // This should allow the lock to be acquired, as it was disposed of
        using (await asyncLock.LockAsync().ConfigureAwait(false)) { }
    }
}
