
using System;

namespace Meadow.Foundation.Web.Maple.Server
{
    public enum EmptyBodyBehavior
    {
        /// <summary>
        /// Set to `Allow`.  Kept for code compatibility with ASP.NET
        /// </summary>
        Default,
        /// <summary>
        /// Empty bodies are treated as valid inputs.
        /// </summary>
        Allow,
        /// <summary>
        /// Empty bodies are treated as invalid inputs.
        /// </summary>
        Disallow
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromBodyAttribute : Attribute
    {
        public EmptyBodyBehavior EmptyBodyBehavior { get; set; }
    }
}