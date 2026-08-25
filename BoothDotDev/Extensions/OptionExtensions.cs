using Optional;

namespace BoothDotDev.Extensions;

/// <summary>
///     Extension methods for the <see cref="Option{T}" /> type.
/// </summary>
public static class OptionExtensions
{
    /// <param name="optional">The <see cref="Option{T}" /> value.</param>
    extension<T>(Option<T> optional)
    {
        /// <summary>
        ///     Gets the value of the <see cref="Option{T}" /> if it has a value; otherwise, returns the default value for the
        ///     type <typeparamref name="T" />.
        /// </summary>
        /// <value>
        ///     The value of the <see cref="Option{T}" /> if it has a value; otherwise, the default value for the type
        ///     <typeparamref name="T" />.
        /// </value>
        public T Value
        {
            get => optional.ValueOr((T)default!);
        }
    }
}
