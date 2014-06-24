using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Web.Http.Description;
using System.Collections.ObjectModel;

namespace MSGorilla.Controllers
{
    public class DocumentController : Controller
    {
        //
        // GET: /Document/
        public ActionResult Index()
        {
            IApiExplorer apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            //Collection<ApiDescription> apiDescriptions = apiExplorer.ApiDescriptions;
            //foreach (ApiDescription api in apiDescriptions)
            //{
            //    //api.
            //}



            //return View(apiExplorer);
            return View(FindControllerApi("api/base"));
        }

        public ActionResult Base()
        {
            return View(FindControllerApi("api/base"));
        }

        private List<ApiDescription> FindControllerApi(string routeStart)
        {
            IApiExplorer apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            List<ApiDescription> list = new List<ApiDescription>();
            foreach(var api in apiExplorer.ApiDescriptions)
            {
                if (api.RelativePath.StartsWith(routeStart, StringComparison.InvariantCultureIgnoreCase))
                {
                    list.Add(api);
                }
            }
            return list;
        }
	}
}