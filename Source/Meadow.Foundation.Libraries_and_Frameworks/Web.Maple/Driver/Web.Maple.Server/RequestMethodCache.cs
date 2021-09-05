using System;
using System.Collections.Generic;
using System.Reflection;

namespace Meadow.Foundation.Web.Maple.Server
{
    internal class RequestMethodCache
    {
        // this is a VERB:NAME:METHOD lookup
        private Dictionary<string, Dictionary<string, MethodInfo>> _methodCache = new Dictionary<string, Dictionary<string, MethodInfo>>(StringComparer.InvariantCultureIgnoreCase);

        public bool Contains(string httpVerb, string methodName)
        {
            if (!_methodCache.ContainsKey(httpVerb)) return false;
            return _methodCache[httpVerb].ContainsKey(methodName);
        }

        public void Add(string httpVerb, string methodName, MethodInfo method)
        {
            if (!_methodCache.ContainsKey(httpVerb))
            {
                _methodCache.Add(httpVerb, new Dictionary<string, MethodInfo>(StringComparer.InvariantCultureIgnoreCase));
            }

            if (_methodCache[httpVerb].ContainsKey(methodName))
            {
                throw new Exception($"Handler for {httpVerb}:{methodName} already registered");
            }

            _methodCache[httpVerb].Add(methodName, method);
        }

        public MethodInfo GetMethod(string httpVerb, string methodName)
        {
            if (!_methodCache.ContainsKey(httpVerb))
            {
                return null;
            }

            if (!_methodCache[httpVerb].ContainsKey(methodName))
            {
                return null;
            }

            return _methodCache[httpVerb][methodName];
        }
    }
}