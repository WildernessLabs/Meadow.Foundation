using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Web.Maple.Server.Routing
{
    /// <summary>
    /// Identifies an action that supports the HTTP PATCH method.
    /// </summary>
    public class HttpPatchAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "PATCH" };

        /// <summary>
        /// Creates a new <see cref="HttpPatchAttribute"/>.
        /// </summary>
        public HttpPatchAttribute()
            : base(_supportedMethods)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpPatchAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public HttpPatchAttribute(string template)
            : base(_supportedMethods, template)
        {
            if (template == null) {
                throw new ArgumentNullException(nameof(template));
            }
        }
    }
}
