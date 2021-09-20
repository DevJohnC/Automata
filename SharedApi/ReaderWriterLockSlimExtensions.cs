using System;
using System.Threading;

namespace Automata
{
    public static class ReaderWriterLockSlimExtensions
    {
        public static IDisposable UseReadLock(this ReaderWriterLockSlim @lock)
        {
            @lock.EnterReadLock();
            return new ReadLock(@lock);
        }

        public static IDisposable UseUpgradableReadLock(this ReaderWriterLockSlim @lock)
        {
            @lock.EnterUpgradeableReadLock();
            return new UpgradableReadLock(@lock);
        }
        
        public static IDisposable UseWriteLock(this ReaderWriterLockSlim @lock)
        {
            @lock.EnterWriteLock();
            return new WriteLock(@lock);
        }

        private class ReadLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;

            public ReadLock(ReaderWriterLockSlim @lock)
            {
                _lock = @lock;
            }

            public void Dispose()
            {
                _lock.ExitReadLock();
            }
        }
        
        private class UpgradableReadLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;

            public UpgradableReadLock(ReaderWriterLockSlim @lock)
            {
                _lock = @lock;
            }

            public void Dispose()
            {
                _lock.ExitUpgradeableReadLock();
            }
        }
        
        private class WriteLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;

            public WriteLock(ReaderWriterLockSlim @lock)
            {
                _lock = @lock;
            }

            public void Dispose()
            {
                _lock.ExitWriteLock();
            }
        }
    }
}