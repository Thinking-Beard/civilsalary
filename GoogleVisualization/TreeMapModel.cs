using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace GoogleVisualization
{
    public class TreeMapModel
    {
        [Display(Order = 1)]
        public string Node { get; set; }
        [Display(Order = 2)]
        public string ParentNode { get; set; }
        [Display(Order = 3)]
        public double Size { get; set; }
        [Display(Order = 4)]
        public double Color { get; set; }
    }
}
