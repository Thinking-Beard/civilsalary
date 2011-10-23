using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting;
using civilsalary.data;

namespace civilsalary.web.Controllers
{
    public class HomeController : Controller
    {
        IRepository _repository;

        public HomeController()
        {
            _repository = new AzureRepository();
        }

        [Route("", "GET")]
        public ActionResult Index()
        {
            var model = _repository.LoadGovernments().ToList();

            return View(model);
        }

    }
}
