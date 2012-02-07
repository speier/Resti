//
// Simple REST handler for quick prototyping
// 2012 (c) Kalman Speier
//
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Resti {
    public class RouteBuilder {
        private readonly IRouteHandler _routeHandler;
        private readonly Type _routeHandlerType;
        private readonly string _httpMethod;

        public RouteBuilder(IRouteHandler routeHandler, string httpMethod) {
            _routeHandler = routeHandler;
            _routeHandlerType = routeHandler.GetType();
            _httpMethod = httpMethod;
        }

        public Func<dynamic, object> this[string path] {
            set { AddRoute(path, value); }
        }

        private void AddRoute(string path, Func<dynamic, object> callback) {
            var url = FixPath(path);
            var routes = RouteTable.Routes;

            using (routes.GetWriteLock()) {

                var route = new Route(url, _routeHandler) {
                    Defaults = new RouteValueDictionary(new {
                        callback,
                        controller = _routeHandlerType.Name,
                        action = "ProcessRequest"
                    }),
                    DataTokens = new RouteValueDictionary(new {
                        namespaces = _routeHandlerType.Namespace
                    }),
                    Constraints = new RouteValueDictionary(new {
                        httpMethod = new HttpMethodConstraint(_httpMethod)
                    })
                };

                foreach (Match m in Regex.Matches(url, "{(.*?)}")) {
                    var optionalParam = m.Groups[1].Value;
                    route.Defaults.Add(optionalParam, string.Empty);
                }

                routes.Add(route);
            }
        }

        private string FixPath(string path) {
            return path.StartsWith("/") ? path.Remove(0, 1) : path;
        }
    }

    public class RestHandler : IRouteHandler, IHttpHandler {
        public IHttpHandler GetHttpHandler(RequestContext requestContext) {
            return this;
        }

        public bool IsReusable {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context) {
            var callback = context.Request.RequestContext.RouteData.Values["callback"] as Func<dynamic, object>;
            if (callback == null)
                return;

            var values = new Dictionary<string, object>(context.Request.RequestContext.RouteData.Values);
            foreach (var key in context.Request.Form.AllKeys) {
                values.Add(key, context.Request.Form[key]);
            }

            var arg = new DynamicJsonObject(values);
            var obj = callback(arg);

            var res = JsonConvert.SerializeObject(
                obj,
                Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
            );

            context.Response.ContentType = "application/json";
            context.Response.Write(res);
        }

        public RouteBuilder Get {
            get { return new RouteBuilder(this, HttpMethods.Get); }
        }

        public RouteBuilder Post {
            get { return new RouteBuilder(this, HttpMethods.Post); }
        }

        public RouteBuilder Put {
            get { return new RouteBuilder(this, HttpMethods.Put); }
        }

        public RouteBuilder Delete {
            get { return new RouteBuilder(this, HttpMethods.Delete); }
        }
    }

    public class HttpMethods {
        public const string Get = "GET";
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Delete = "DELETE";
    }
}