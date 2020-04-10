﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using JasperHttp.Model;

namespace JasperHttp.Routing
{
    public static class ParameterInfoExtensions
    {
        public static bool IsSpread(this ParameterInfo parameter)
        {
            if (parameter.Name == Route.RelativePath && parameter.ParameterType == typeof(string)) return true;
            if (parameter.Name == Route.PathSegments && parameter.ParameterType == typeof(string[])) return true;
            return false;
        }
    }

    public class Route
    {
        public const string RelativePath = "relativePath";
        public const string PathSegments = "pathSegments";
        public static int Count;
        private readonly List<ISegment> _segments = new List<ISegment>();

        private Lazy<RouteArgument[]> _arguments;
        private Spread _spread;


        public Route(string pattern, string httpMethod)
        {
            pattern = pattern?.TrimStart('/').TrimEnd('/') ?? throw new ArgumentNullException(nameof(pattern));


            HttpMethod = httpMethod;

            if (pattern.IsEmpty())
            {
                Pattern = "";
            }
            else
            {
                var segments = pattern.Split('/');
                for (var i = 0; i < segments.Length; i++)
                {
                    var segment = ToParameter(segments[i], i);
                    _segments.Add(segment);
                }

                validateSegments();


                Pattern = string.Join("/", _segments.Select(x => x.SegmentPath));
            }

            Name = $"{HttpMethod}:/{Pattern}";

            setupArgumentsAndSpread();
        }

        public Route(ISegment[] segments, string httpVerb)
        {
            _segments.AddRange(segments);

            validateSegments();

            HttpMethod = httpVerb;

            Pattern = _segments.Select(x => x.SegmentPath).Join("/");
            Name = $"{HttpMethod}:{Pattern}";

            setupArgumentsAndSpread();
        }

        public string Description => $"{HttpMethod}: {Pattern}";

        public string VariableName { get; } = "Route" + ++Count;


        public IEnumerable<ISegment> Segments => _segments;

        public Type InputType { get; set; }
        public Type HandlerType { get; set; }
        public MethodInfo Method { get; set; }

        public bool HasParameters => HasSpread || _arguments.Value.Any();

        public IEnumerable<RouteArgument> Arguments => _arguments.Value.ToArray();

        public string Pattern { get; }

        public bool HasSpread => _spread != null;

        public string Name { get; set; }
        public string HttpMethod { get; internal set; }

        public string LastSegment => _segments.Count == 0 ? string.Empty : _segments.Last().CanonicalPath();

        public IEnumerable<ISegment> Parameters => _segments.Where(x => !(x is Segment)).ToArray();

        public RouteHandler Handler { get; set; }
        public RouteChain Chain { get; internal set; }

        /// <summary>
        ///     This is only for testing purposes
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Route For(string url, string httpMethod)
        {
            return new Route(url.TrimStart('/'), httpMethod ?? HttpVerbs.GET);
        }

        public static ISegment ToParameter(string path, int position)
        {
            if (path == "...") return new Spread(position);

            if (path.StartsWith(":"))
            {
                var key = path.Trim(':');
                return new RouteArgument(key, position);
            }

            if (path.StartsWith("{") && path.EndsWith("}"))
            {
                var key = path.TrimStart('{').TrimEnd('}');
                return new RouteArgument(key, position);
            }

            return new Segment(path, position);
        }

        private void validateSegments()
        {
            if (_segments.FirstOrDefault() is Spread)
                throw new InvalidOperationException(
                    $"'{Pattern}' is an invalid route. Cannot use a spread argument as the first segment");

            if (_segments.FirstOrDefault() is RouteArgument)
                throw new InvalidOperationException(
                    $"'{Pattern}' is an invalid route. Cannot use a route argument as the first segment");
        }


        private void setupArgumentsAndSpread()
        {
            _arguments = new Lazy<RouteArgument[]>(() => _segments.OfType<RouteArgument>().ToArray());
            _spread = _segments.OfType<Spread>().SingleOrDefault();

            if (!HasSpread) return;

            if (!Equals(_spread, _segments.Last()))
                throw new ArgumentOutOfRangeException(nameof(Pattern),
                    "The spread parameter can only be the last segment in a route");
        }


        public RouteArgument GetArgument(string key)
        {
            return _segments.OfType<RouteArgument>().FirstOrDefault(x => x.Key == key);
        }


        public IDictionary<string, string> ToParameters(object input)
        {
            var dict = new Dictionary<string, string>();
            _arguments.Value.Each(x => dict.Add(x.Key, x.ReadRouteDataFromInput(input)));

            return dict;
        }


        public string ToUrlFromInputModel(object model)
        {
            return "/" + _segments.Select(x => x.SegmentFromModel(model)).Join("/");
        }

        public override string ToString()
        {
            return $"{HttpMethod}: {Pattern}";
        }

        public string ReadRouteDataFromMethodArguments(Expression expression)
        {
            var arguments = MethodCallParser.ToArguments(expression);
            return "/" + _segments.Select(x => x.ReadRouteDataFromMethodArguments(arguments)).Join("/");
        }

        public string ToUrlFromParameters(IDictionary<string, object> parameters)
        {
            return "/" + _segments.Select(x => x.SegmentFromParameters(parameters)).Join("/");
        }

    }
}
