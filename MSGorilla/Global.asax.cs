using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using MSGorilla.Filters;

namespace MSGorilla
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //json.UseDataContractJsonSerializer = true;
            json.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;

            var config = GlobalConfiguration.Configuration;
            config.Services.Replace(typeof(IDocumentationProvider), 
                new XmlCommentDocumentationProvider(HttpContext.Current.Server.MapPath("~/bin/MSGorilla.XML")));
        }

        void Application_Error(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;

            Exception ex = context.Server.GetLastError();
            if (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            if (ex is System.Security.Cryptography.CryptographicException)
            {
                HttpCookie cookie = new HttpCookie("ASP.NET_SessionId");
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);

                cookie = new HttpCookie("FedAuth");
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);

                cookie = new HttpCookie("FedAuth1");
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);

                cookie = new HttpCookie("Authorization");
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);

                Response.Redirect("/error?message=System.Security.Cryptography.CryptographicException&returnUrl=/account/login");
            }
            //Server.Transfer("GeneralError.aspx");
        }
    }
}
