using HomeworkAssignment3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace HomeworkAssignment3.Controllers
{
    public class MaintainController : Controller
    {
        private BikeStoresEntities1 db = new BikeStoresEntities1();

        public ActionResult Index()
        {
            try
            {
                // Simple data loading without async/await
                var staffList = db.staffs
                    .Include(s => s.store)
                    .Include(s => s.staff1)
                    .ToList();

                var customerList = db.customers.ToList();

                var productList = db.products
                    .Include(p => p.brand)
                    .Include(p => p.category)
                    .ToList();

                // Create simple dropdown lists
                ViewBag.Stores = GetStoresList();
                ViewBag.Brands = GetBrandsList();
                ViewBag.Categories = GetCategoriesList();
                ViewBag.Managers = GetManagersList();

                var viewModel = new MaintainViewModel
                {
                    StaffList = staffList,
                    CustomerList = customerList,
                    ProductList = productList
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Return empty data on error
                ViewBag.Stores = new List<SelectListItem>();
                ViewBag.Brands = new List<SelectListItem>();
                ViewBag.Categories = new List<SelectListItem>();
                ViewBag.Managers = new List<SelectListItem>();

                return View(new MaintainViewModel
                {
                    StaffList = new List<staff>(),
                    CustomerList = new List<customer>(),
                    ProductList = new List<product>()
                });
            }
        }

        private List<SelectListItem> GetStoresList()
        {
            try
            {
                return db.stores
                    .Select(s => new SelectListItem
                    {
                        Value = s.store_id.ToString(),
                        Text = s.store_name
                    })
                    .ToList();
            }
            catch
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Store 1" },
                    new SelectListItem { Value = "2", Text = "Store 2" },
                    new SelectListItem { Value = "3", Text = "Store 3" }
                };
            }
        }

        private List<SelectListItem> GetBrandsList()
        {
            try
            {
                return db.brands
                    .Select(b => new SelectListItem
                    {
                        Value = b.brand_id.ToString(),
                        Text = b.brand_name
                    })
                    .ToList();
            }
            catch
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Trek" },
                    new SelectListItem { Value = "2", Text = "Haro" },
                    new SelectListItem { Value = "3", Text = "Electra" },
                    new SelectListItem { Value = "4", Text = "Pure Cycles" }
                };
            }
        }

        private List<SelectListItem> GetCategoriesList()
        {
            try
            {
                return db.categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.category_id.ToString(),
                        Text = c.category_name
                    })
                    .ToList();
            }
            catch
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Mountain Bikes" },
                    new SelectListItem { Value = "2", Text = "Road Bikes" },
                    new SelectListItem { Value = "3", Text = "Cruisers" }
                };
            }
        }

        private List<SelectListItem> GetManagersList()
        {
            try
            {
                return db.staffs
                    .Select(s => new SelectListItem
                    {
                        Value = s.staff_id.ToString(),
                        Text = s.first_name + " " + s.last_name
                    })
                    .ToList();
            }
            catch
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "John Manager" },
                    new SelectListItem { Value = "2", Text = "Jane Manager" }
                };
            }
        }
    }
}