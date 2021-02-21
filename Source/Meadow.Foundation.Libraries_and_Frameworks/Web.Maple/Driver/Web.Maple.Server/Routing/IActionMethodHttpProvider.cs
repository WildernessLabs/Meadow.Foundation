using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Web.Maple.Server.Routing
{
    /// <summary>
    /// Interface that exposes a list of http methods that are supported by an provider.
    /// </summary>
    public interface IActionHttpMethodProvider
    {
        /// <summary>
        /// The list of http methods this action provider supports.
        /// </summary>
        IEnumerable<string> HttpMethods { get; }
    }
}
