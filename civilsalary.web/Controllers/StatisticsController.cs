using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting;
using civilsalary.data;
using civilsalary.web.Models;
using GoogleVisualization;

namespace civilsalary.web.Controllers
{
    public class StatisticsController : Controller
    {
        IRepository _repository;

        public StatisticsController()
        {
            _repository = new AzureRepository();
        }

        [Route("data/statistics/departments/{*keyString}", "GET")]
        public GoogleChartDataSourceResult DepartmentData(string keyString, string tq, string tqx)
        {
            var query = _repository.LoadDepartments(keyString);

            return new GoogleChartDataSourceResult(query, tq, tqx, null);
        }

        [Route("statistics/{*keyString}", "GET")]
        public ActionResult Detail(string keyString)
        {
            if (string.IsNullOrWhiteSpace(keyString)) return RedirectPermanent("/");

            var keys = keyString.Split(new string[] { "/department/" }, StringSplitOptions.None);

            if (keys.Length > 2) return HttpNotFound();

            var model = new StatisticsDetailModel();

            model.Government = _repository.LoadGovernment(keys[0]);

            if (model.Government == null) return HttpNotFound();

            if (keys.Length > 1)
            {
                model.Department = _repository.LoadDepartment(keys[0], keys[1]);

                if (model.Department == null) return HttpNotFound();

                return View("DepartmentDetail", model);
            }

            return View("GovernmentDetail", model);
        }

    }
}
