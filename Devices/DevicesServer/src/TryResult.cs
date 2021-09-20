using System.Diagnostics.CodeAnalysis;

namespace Automata.Devices
{
    public struct TryResult<T>
        where T : notnull
    {
        [MemberNotNullWhen(true, nameof(Result))]
        public bool DidSucceed { get; }
        
        public T? Result { get; }

        private TryResult(bool didSucceed, T? result)
        {
            DidSucceed = didSucceed;
            Result = result;
        }

        public static readonly TryResult<T> Failure = new TryResult<T>(false, default);

        public static TryResult<T> Success(T result)
        {
            return new TryResult<T>(true, result);
        }
    }
}