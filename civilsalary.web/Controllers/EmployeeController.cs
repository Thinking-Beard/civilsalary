using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting;

namespace civilsalary.web.Controllers
{
    public class EmployeeController : Controller
    {
        [Route("employee/search", "GET")]
        public ActionResult Search()
        {
            return View();
        }

    }
}
