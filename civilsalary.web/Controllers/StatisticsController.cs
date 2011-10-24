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

        [Route("statistics/data/departments/{keyString}", "GET")]
        public GoogleChartDataSourceResult DepartmentData(string keyString, string tq, string tqx)
        {
            var query = _repository.LoadDepartments(keyString);

            return new GoogleChartDataSourceResult(query, tq, tqx, null);
        }

        [Route("statistics/data/tree", "GET")]
        public GoogleChartDataSourceResult TreeMapData(string tqx)
        {
            var governmentDictionary = _repository.LoadGovernments().ToDictionary(g => g.Key);
            var childAssociations = _repository.LoadGovernmentAssociations().Where(a => a.Association == GovernmentAssociationRow.ChildOfType).ToList();
            var roots = governmentDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var data = new List<TreeMapModel>();

            var query = from a in childAssociations
                        let g = governmentDictionary[a.Key1]
                        select new { association = a, government = g };

            foreach(var a in query)
            {
                if (roots.ContainsKey(a.government.Key))
                {
                    roots.Remove(a.government.Key);
                }

                data.Add(new TreeMapModel()
                {
                    Node = a.government.Name,
                    ParentNode = governmentDictionary[a.association.Key2].Name,
                    Size = Math.Max((a.government.SalarySum ?? a.government.GrossPaySum ?? 100000) / 100000, 1),
                    Color = Math.Max((a.government.SalaryAvg ?? a.government.GrossPayAvg ?? 1000) / 1000, 1)
                });
            }

            foreach (var r in roots.Values)
            {
                data.Add(new TreeMapModel()
                {
                    Node = r.Name,
                    ParentNode = null,
                    Size = Math.Max((r.SalarySum ?? r.GrossPaySum ?? 100000) / 100000, 1),
                    Color = Math.Max((r.SalaryAvg ?? r.GrossPayAvg ?? 1000) / 1000, 1)
                });
            }

            return GoogleChartDataSourceResult.Create(data, tqx, null);
        }

        [Route("statistics/data/{*keyString}", "GET")]
        public ActionResult DetailData(string keyString)
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
