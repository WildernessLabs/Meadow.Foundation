using Meadow.Foundation.Web.Maple.Server.Routing;
using Meadow.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Meadow.Foundation.Web.Maple.Server
{
    internal class HandlerInfo
    {
        public Type? HandlerType { get; set; }
        public MethodInfo? Method { get; set; }
        public ParameterInfo? Parameter { get; set; }
    }

    internal class RequestMethodCache
    {
        // this is a VERB:NAME:METHOD lookup
        private Dictionary<string, Dictionary<string, MethodInfo>> _methodCache = new Dictionary<string, Dictionary<string, MethodInfo>>(StringComparer.InvariantCultureIgnoreCase);

        public ILogger? Logger { get; }
        private Regex ParamRegex { get; } = new Regex("{(.*?)}");

        public RequestMethodCache(ILogger? logger)
        {
            Logger = logger;
        }

        public bool Contains(string httpVerb, string methodName)
        {
            if (!_methodCache.ContainsKey(httpVerb)) return false;
            return _methodCache[httpVerb].ContainsKey(methodName);
        }

        private object? GetTypedParameter(string param, Type type)
        {
            if (string.IsNullOrEmpty(param)) return null;

            if (type == typeof(string))
            {
                return param;
            }
            else if (type == typeof(int))
            {
                return Convert.ToInt32(param);
            }
            else if (type == typeof(double))
            {
                return Convert.ToDouble(param);
            }
            else if (type == typeof(float))
            {
                return Convert.ToSingle(param);
            }
            else if (type == typeof(Guid))
            {
                return Guid.Parse(param);
            }
            else if (type == typeof(DateTime))
            {
                return DateTime.Parse(param);
            }
            else if (type == typeof(short))
            {
                return Convert.ToInt16(param);
            }
            else if (type == typeof(byte))
            {
                return Convert.ToByte(param);
            }
            else if (type == typeof(bool))
            {
                return Convert.ToBoolean(param);
            }
            else
            {
                throw new Exception($"Unsupported Parameter type '{type.Name}'");
            }
        }

        public HandlerInfo? Match(string verb, string url, out object? param)
        {
            param = null;

            // clean off any querystring or trailing slash
            var qsi = url.IndexOf('?');
            if (qsi >= 0)
            {
                url = url.Substring(0, qsi);
            }
            url = url.TrimEnd('/');
            if (!url.StartsWith('/'))
            {
                url = $"/{url}";
            }

            if (_templateToTypeMap.ContainsKey(verb))
            {
                foreach (var template in _templateToTypeMap[verb].Keys)
                {
                    var regexIndex = template.IndexOf("(.*?)");

                    if (regexIndex >= 0)
                    {
                        var s = Regex.Split(url, template, RegexOptions.IgnoreCase);
                        if (s.Length > 1)
                        {
                            string paramString;

                            paramString = s.FirstOrDefault(m => m != string.Empty && m != "/");

                            // if the match contains a slash, we've matched but with a non-matching tail
                            // TODO: improve the regex to do proper matching
                            if (paramString.Contains('/'))
                            {
                                return null;
                            }

                            var info = _templateToTypeMap[verb][template];

                            // try to turn the string parameter into something typed
                            param = GetTypedParameter(paramString, info.Parameter.ParameterType);

                            return info;
                        }
                    }
                    else if (string.Compare(template, url, true) == 0)
                    {
                        return _templateToTypeMap[verb][template];
                    }
                }
            }

            // no handler
            return null;
        }

        // lookup is VERB + PATH to get handler type
        // then VERB + PATH to get method
        private Dictionary<string, Dictionary<string, HandlerInfo>> _templateToTypeMap = new Dictionary<string, Dictionary<string, HandlerInfo>>(StringComparer.OrdinalIgnoreCase);

        private void AddMethod(string verb, string template, MethodInfo method)
        {
            // is the template relative (no leading slash) or absolute?
            if (!template.StartsWith('/'))
            {
                // absolute.  Prefix with handler type name
                var prefix = method.DeclaringType.Name;
                if (prefix.EndsWith("Handler"))
                {
                    // crop "Handler" suffix
                    prefix = prefix.Substring(0, prefix.Length - 7);
                }

                if (template.Length > 0)
                {
                    template = $"{prefix}/{template}";
                }
                else
                {
                    template = prefix;
                }
            }

            // does the url contain a parameter?
            var match = ParamRegex.Match(template);
            if (match.Success)
            {
                // get the parameter name
                var name = template.Substring(match.Index + 1, match.Length - 2);

                // verify the method has a parameter with the same name
                var parm = method.GetParameters().Where(p => string.Compare(p.Name, name, true) == 0).FirstOrDefault();
                if (parm == null)
                {
                    throw new Exception($"Parameter name '{name}' not found in method '{method.Name}'");
                }

                // generate the regex to test against urls
                var rgx = template
                    .Replace(match.Value, "(.*?)", StringComparison.OrdinalIgnoreCase)
                    .Replace("/", "\\/");                
                rgx += "$";

                if (!_templateToTypeMap.ContainsKey(verb))
                {
                    _templateToTypeMap.Add(verb, new Dictionary<string, HandlerInfo>(StringComparer.OrdinalIgnoreCase));
                }
                if (!_templateToTypeMap[verb].ContainsKey(rgx))
                {
                    _templateToTypeMap[verb].Add(rgx, new HandlerInfo
                    {
                        Method = method,
                        HandlerType = method.DeclaringType,
                        Parameter = parm
                    });
                }
            }
            else
            {
                if (!template.StartsWith('/'))
                {
                    template = $"/{template}";
                }

                if (!_templateToTypeMap.ContainsKey(verb))
                {
                    _templateToTypeMap.Add(verb, new Dictionary<string, HandlerInfo>(StringComparer.OrdinalIgnoreCase));
                }
                if (!_templateToTypeMap[verb].ContainsKey(template))
                {
                    _templateToTypeMap[verb].Add(template, new HandlerInfo
                    {
                        Method = method,
                        HandlerType = method.DeclaringType
                    });
                }
            }

            Logger?.Info($"{verb} : {template} --> {method.Name}");
        }

        public void AddType(Type t)
        {
            if ((t.GetInterfaces() ?? null).Contains(typeof(IRequestHandler)))
            {
                foreach(var m in t.GetMethods())
                {
                    //first, let's see if the method has the correct http verb
                    foreach (var attr in m.GetCustomAttributes())
                    {
                        switch (attr)
                        {
                            case HttpGetAttribute a:
                                AddMethod("GET", a.Template ?? string.Empty, m);
                                break;
                            case HttpPutAttribute a:
                                AddMethod("PUT", a.Template ?? string.Empty, m);
                                break;
                            case HttpPatchAttribute a:
                                AddMethod("PATCH", a.Template ?? string.Empty, m);
                                break;
                            case HttpPostAttribute a:
                                AddMethod("POST", a.Template ?? string.Empty, m);
                                break;
                            case HttpDeleteAttribute a:
                                AddMethod("DELETE", a.Template ?? string.Empty, m);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                Logger?.Warn($"Attempt to add type {t.Name} to Request handlers, but does not implement IRequestHandler");
            }
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

        public MethodInfo? GetMethod(string httpVerb, string methodName)
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