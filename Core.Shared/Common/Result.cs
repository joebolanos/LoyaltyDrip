using System;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

namespace Core.Shared.Common
{
    public interface IResult
    {
        public bool IsSuccess { get; }
        public bool IsFailure { get; }
    }

    public readonly struct Result<T> : IResult
    {
        private enum ResultState
        {
            Failure,
            Success
        }

        private readonly ResultState _state;

        public T Value { get; }
        public Exception Exception { get; }

        [JsonIgnore]
        public bool IsSuccess => _state == ResultState.Success;

        [JsonIgnore]
        public bool IsFailure => _state == ResultState.Failure;

        [JsonConstructor]
        public Result(T value)
        {
            Value = value;
            Exception = null;
            _state = ResultState.Success;
        }

        public Result(Exception exception)
        {
            Value = default;
            Exception = exception;
            _state = ResultState.Failure;
        }

        [Pure]
        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Exception, TResult> onFailure)
            => IsFailure ? onFailure(Exception!) : onSuccess(Value!);

        public static implicit operator Result<T>(T value)
            => value != null ? new Result<T>(value) : new Result<T>();

        public static implicit operator Result<T>(Exception exception)
            => new Result<T>(exception);
    }
}
