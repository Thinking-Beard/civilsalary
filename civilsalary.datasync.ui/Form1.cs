using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using civilsalary.datasync.usa.md.baltimorecity;
using civilsalary.datasync.usa.md.baltimorecounty;

namespace civilsalary.datasync.ui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var data = new EmployeeDataProvider(typeof(BaltimoreCountyEmployeeEnumerator));
            var data = new EmployeeDataProvider(typeof(BaltimoreCityEmployeeEnumerator));

            foreach (var employee in data)
            {
                Console.WriteLine("{0} ({1}:{2}) - {3}", employee.Name, employee.Department, employee.Position, employee.Salary);
            }
        }
    }
}
