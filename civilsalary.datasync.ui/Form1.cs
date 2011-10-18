﻿using System;
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
            Importer.Import(new EmployeeDataProvider(typeof(BaltimoreCountyEmployeeEnumerator))
                , new EmployeeDataProvider(typeof(BaltimoreCityEmployeeEnumerator)));
        }
    }
}
