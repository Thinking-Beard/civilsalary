using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using civilsalary.data;

namespace civilsalary.datasync
{
    public sealed class EmployeeData
    {
        public EmployeeRow Row { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentKey { get; set; }
    }
}
