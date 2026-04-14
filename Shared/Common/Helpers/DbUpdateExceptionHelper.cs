using Microsoft.EntityFrameworkCore;

namespace aqua_api.Shared.Common.Helpers
{
    public static class DbUpdateExceptionHelper
    {
        public static bool TryGetUniqueViolation(DbUpdateException exception, out string? rawMessage)
        {
            rawMessage = null;

            foreach (var current in EnumerateExceptionChain(exception))
            {
                rawMessage = current.Message;
                var typeName = current.GetType().FullName ?? current.GetType().Name;

                // SQL Server: 2601, 2627
                if (typeName.Contains("SqlException", StringComparison.OrdinalIgnoreCase) &&
                    TryGetIntProperty(current, "Number", out var sqlNumber) &&
                    (sqlNumber == 2601 || sqlNumber == 2627))
                {
                    return true;
                }

                // PostgreSQL: 23505
                if (typeName.Contains("PostgresException", StringComparison.OrdinalIgnoreCase) &&
                    TryGetStringProperty(current, "SqlState", out var sqlState) &&
                    string.Equals(sqlState, "23505", StringComparison.Ordinal))
                {
                    return true;
                }

                // MySQL: 1062
                if (typeName.Contains("MySqlException", StringComparison.OrdinalIgnoreCase) &&
                    TryGetIntProperty(current, "Number", out var mySqlNumber) &&
                    mySqlNumber == 1062)
                {
                    return true;
                }

                // Fallback text checks
                var message = current.Message ?? string.Empty;
                if (message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<Exception> EnumerateExceptionChain(Exception exception)
        {
            Exception? current = exception;
            while (current != null)
            {
                yield return current;
                current = current.InnerException;
            }
        }

        private static bool TryGetIntProperty(Exception exception, string propertyName, out int value)
        {
            value = default;
            var prop = exception.GetType().GetProperty(propertyName);
            if (prop == null)
            {
                return false;
            }

            var propValue = prop.GetValue(exception);
            if (propValue is int intValue)
            {
                value = intValue;
                return true;
            }

            return false;
        }

        private static bool TryGetStringProperty(Exception exception, string propertyName, out string? value)
        {
            value = null;
            var prop = exception.GetType().GetProperty(propertyName);
            if (prop == null)
            {
                return false;
            }

            value = prop.GetValue(exception) as string;
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
