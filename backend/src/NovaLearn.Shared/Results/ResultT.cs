namespace NovaLearn.Shared.Results;

/// <summary>
/// Represents the outcome of an operation that yields a <typeparamref name="TValue"/> on success.
/// Accessing <see cref="Value"/> on a failed result throws, forcing callers to check first.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>The success value. Throws if the result is a failure.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    /// <summary>Implicitly lifts a raw value into a successful result.</summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>Implicitly lifts an error into a failed result.</summary>
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
