using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;

using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Web;
using Microsoft.IdentityModel.Web.Configuration;
//using System.IdentityModel.Tokens;
//using System.IdentityModel;

using MSGorilla.Filters;

namespace MSGorilla
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            //FederatedAuthentication.ServiceConfigurationCreated += this.OnServiceConfigurationCreated; 

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

        //private void OnServiceConfigurationCreated(object sender, ServiceConfigurationCreatedEventArgs e)
        //{
        //    // Use the <serviceCertificate> to protect the cookies that 
        //    // are sent to the client.
        //    List<CookieTransform> sessionTransforms =
        //        new List<CookieTransform>(
        //            new CookieTransform[] 
        //    {
        //        new DeflateCookieTransform(), 
        //        new RsaEncryptionCookieTransform(
        //            e.ServiceConfiguration.ServiceCertificate),
        //        new RsaSignatureCookieTransform(
        //            e.ServiceConfiguration.ServiceCertificate)  
        //    });
        //    SessionSecurityTokenHandler sessionHandler =
        //        new SessionSecurityTokenHandler(sessionTransforms.AsReadOnly());

        //    e.ServiceConfiguration.SecurityTokenHandlers.AddOrReplace(sessionHandler);
        //}

        protected void Application_PostAuthorizeRequest()
        {
            HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
        }

        void Application_Error(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;

            Exception ex = context.Server.GetLastError();
            if (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            if (ex is System.Security.Cryptography.CryptographicException ||
                ex is FormatException)
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

                Response.Redirect("/account/login");
            }
            //Server.Transfer("GeneralError.aspx");
        }
    }
}
