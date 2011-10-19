using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting;
using civilsalary.data;
using civilsalary.web.Models;

namespace civilsalary.web.Controllers
{
    public class StatisticsController : Controller
    {
        IRepository _repository;

        public StatisticsController()
        {
            _repository = new AzureRepository();
        }

        [Route("statistics/{*keyString}", "GET")]
        public ActionResult Detail(string keyString)
        {
            if (string.IsNullOrWhiteSpace(keyString)) return RedirectPermanent("/");

            var keys = keyString.Split(new string[] { "/department/" }, StringSplitOptions.None);

            if (keys.Length > 2) return HttpNotFound();

            var model = new StatisticsDetailModel();

            model.Government = _repository.LoadGovernment(keys[0]);

            if (keys.Length > 1)
            {
                model.Department = _repository.LoadDepartment(keys[0], keys[1]);
            }

            return View(model);
        }

    }
}
