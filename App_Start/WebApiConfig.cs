using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Filters;

namespace WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Filters.Add(new NotImplExceptionFilterAttribute());
            //GlobalConfiguration.Configuration.MessageHandlers.Add(new Common.MessageLoggingHandler());
            //config.SuppressDefaultHostAuthentication();
            //config.SuppressHostPrincipal();
            config.MapHttpAttributeRoutes();

            config.MessageHandlers.Add(new Common.CancelledTaskBugWorkaroundMessageHandler());
            config.MessageHandlers.Add(new Common.AllowOptionsHandler());
            config.MessageHandlers.Add(new WebAPI.Providers.TimeZoneHandler());

            //if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["LogEnabled"]) &&
            //    System.Configuration.ConfigurationManager.AppSettings["LogEnabled"].ToLower() == "true")
            //    config.MessageHandlers.Add(new Common.MessageLoggingHandler());

            //config.Formatters.Add(new Common.BinaryMediaTypeFormatter());
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config.Filters.Add(new AuthorizeAttribute());


            var defaultSettings = new JsonSerializerSettings
            {
                  NullValueHandling = NullValueHandling.Ignore,
            };

            JsonConvert.DefaultSettings = () => { return defaultSettings; };

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings = defaultSettings;
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            //var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            //jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public class ShouldSerializeContractResolver : DefaultContractResolver
        {
            public new static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                property.ShouldSerialize = Instance => true;
                //if (property.DeclaringType == typeof(Employee) && property.PropertyName == "Manager")
                //{
                //    property.ShouldSerialize =
                //        instance =>
                //        {
                //            Employee e = (Employee)instance;
                //            return e.Manager != e;
                //        };
                //}

                return property;
            }
        }

        public class NotImplExceptionFilterAttribute : ExceptionFilterAttribute
        {
            public override void OnException(HttpActionExecutedContext context)
            {
                try
                {
                    WebAPI.Common.Logger.Instance(0).Log(context.Exception, Common.Logger.LogType.Error, "Unhandled Error");
                }
                catch(Exception exp)
                {

                }
                context.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.ExpectationFailed);
                context.Response.Content = new System.Net.Http.StringContent(context.Exception.Message);
                if(context.Exception is Npgsql.NpgsqlException)
                {
                    if((context.Exception as Npgsql.NpgsqlException).Message != null)
                        context.Response.Content = new System.Net.Http.StringContent((context.Exception as Npgsql.NpgsqlException).Message);
                    else
                        context.Response.Content = new System.Net.Http.StringContent((context.Exception as Npgsql.NpgsqlException).Message);
                }
            }
        }


    }
}
