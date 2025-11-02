using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeworkAssignment3.Models
{
    public class HomeViewModel
    {
        public List<staff> StaffList { get; set; }
        public List<customer> CustomerList { get; set; }
        public List<product> ProductList { get; set; }
        public List<string> Brands { get; set; }
        public List<string> Categories { get; set; }
        public int StaffPage { get; set; }
        public int CustomerPage { get; set; }
        public int ProductPage { get; set; }
        public int TotalStaff { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public string SelectedBrand { get; set; }
        public string SelectedCategory { get; set; }

        // Add this property for product images
        public Dictionary<int, string> ProductImages { get; set; }
    }
}