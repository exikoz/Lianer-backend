using System.Runtime.CompilerServices;

namespace Lianer.Core.API.Common;
public static class Guard
{
    public static class Against
    {
        public static void Null<T>(T? value,[CallerArgumentExpression(nameof(value))] string valueName= "") where T : class
        {
            if(value is null) throw new ArgumentNullException(valueName);
        }

        public static void NullOrWhiteSpace(string? value, [CallerArgumentExpression(nameof(value))] string valueName= "")
        {
            if(string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value cannot be null or whitespace.", valueName);
        }

        public static void NullOrEmptyGuid(Guid? value, [CallerArgumentExpression(nameof(value))] string valueName= "")
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                throw new ArgumentException("Guid cannot be null or empty.", valueName);
        }

        public static void NullOrEmpty<T>(ICollection<T>? value, [CallerArgumentExpression(nameof(value))] string valueName= "")
        {
            if (value == null || value.Count == 0)
                throw new ArgumentException("Collection cannot be null or empty.", valueName);
        }

        public static void Zero(int value, [CallerArgumentExpression(nameof(value))] string valueName= "")
        {
            if (value == 0)
                throw new ArgumentException("Value cannot be zero.", valueName);
        }

        public static void NegativeOrZero(int value, [CallerArgumentExpression(nameof(value))] string valueName= "")
        {
            if (value <= 0)
                throw new ArgumentException("Value must be greater than zero.", valueName);
        }
    }
}