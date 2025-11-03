using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeworkAssignment3.Models
{
    public class HomeViewModel
    {
        // Data collections
        public List<staff> StaffList { get; set; }
        public List<customer> CustomerList { get; set; }
        public List<product> ProductList { get; set; }

        // Filter options
        public List<string> Brands { get; set; }
        public List<string> Categories { get; set; }

        // Page tracking
        public int StaffPage { get; set; }
        public int CustomerPage { get; set; }
        public int ProductPage { get; set; }

        // Total count for pages
        public int TotalStaff { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }

        // Current filters
        public string SelectedBrand { get; set; }
        public string SelectedCategory { get; set; }

        // Image mapping
        public Dictionary<int, string> ProductImages { get; set; }
    }
}