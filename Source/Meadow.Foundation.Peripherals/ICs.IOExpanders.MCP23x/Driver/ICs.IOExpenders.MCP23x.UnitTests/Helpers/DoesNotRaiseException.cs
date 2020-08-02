using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers
{
    public
        class DoesNotRaiseException : XunitException
    {
        private readonly string _stackTrace = null;

        public DoesNotRaiseException(Type actual)
            : base("(Unexpected event was raised, no event expected)")
        {
            Actual = ConvertToSimpleTypeName(actual.GetTypeInfo());
        }

        /// <summary>
        /// Gets the actual value.
        /// </summary>
        public string Actual { get; }


        /// <summary>
        /// Gets a message that describes the current exception. Includes the expected and actual values.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
        /// <filterpriority>1</filterpriority>
        public override string Message =>
            string.Format(
                CultureInfo.CurrentCulture,
                "{0}{2}{1}",
                base.Message,
                Actual ?? "(null)",
                Environment.NewLine);

        /// <summary>
        /// Gets a string representation of the frames on the call stack at the time the current exception was thrown.
        /// </summary>
        /// <returns>A string that describes the contents of the call stack, with the most recent method call appearing first.</returns>
        public override string StackTrace => _stackTrace ?? base.StackTrace;

        private static string ConvertToSimpleTypeName(TypeInfo typeInfo)
        {
            if (!typeInfo.IsGenericType)
            {
                return typeInfo.Name;
            }

            var simpleNames = typeInfo.GenericTypeArguments.Select(type => ConvertToSimpleTypeName(type.GetTypeInfo()));
            var backTickIdx = typeInfo.Name.IndexOf('`');
            if (backTickIdx < 0)
            {
                backTickIdx = typeInfo.Name.Length; // F# doesn't use backticks for generic type names
            }

            return $"{typeInfo.Name.Substring(0, backTickIdx)}<{string.Join(", ", simpleNames)}>";
        }
    }
}
