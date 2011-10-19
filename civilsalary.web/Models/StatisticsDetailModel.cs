using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using civilsalary.data;

namespace civilsalary.web.Models
{
    public class StatisticsDetailModel
    {
        public GovernmentRow Government { get; set; }
        public DepartmentRow Department { get; set; }
    }
}