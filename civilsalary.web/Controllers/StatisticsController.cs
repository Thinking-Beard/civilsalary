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

        [Route("statistics/data/department/{keyString}", "GET")]
        public GoogleChartDataSourceResult DepartmentData(string keyString, string tq, string tqx)
        {
            var query = _repository.LoadDepartments(keyString);

            return new GoogleChartDataSourceResult(query, tq, tqx, null);
        }

        [Route("statistics/data/tree", "GET")]
        public GoogleChartDataSourceResult TreeMapData(string tqx)
        {
            var governmentDictionary = _repository.LoadGovernments().ToDictionary(g => g.Key);

            var query = from a in _repository.LoadGovernmentAssociations()
                        where a.Association == GovernmentAssociationRow.ChildOfType
                        let g = governmentDictionary[a.Key1]
                        select new TreeMapModel()
                        {
                            Node = g.Name,
                            ParentNode = governmentDictionary[a.Key2].Name,
                            Size = g.SalarySum ?? g.GrossPaySum ?? 1,
                            Color = g.SalaryAvg ?? g.GrossPayAvg ?? 1
                        };

            return new GoogleChartDataSourceResult(query, string.Empty, tqx, null);
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
