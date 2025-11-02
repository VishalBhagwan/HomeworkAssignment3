using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeworkAssignment3.Models
{
    public class MaintainViewModel
    {
        public List<staff> StaffList { get; set; }
        public List<customer> CustomerList { get; set; }
        public List<product> ProductList { get; set; }
    }
}