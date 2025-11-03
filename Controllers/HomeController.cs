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
    public class HomeController : Controller
    {
        private BikeStoresEntities1 db = new BikeStoresEntities1();

        public async Task<ActionResult> Index(
            int staffPage = 1, int customerPage = 1, int productPage = 1,
            string brandFilter = null, string categoryFilter = null)
        {
            const int pageSize = 1;

            // Data retreival
            var totalStaff = await db.staffs.CountAsync();
            var staffList = await db.staffs
                .Include(s => s.store)
                .OrderBy(s => s.staff_id)
                .Skip((staffPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCustomers = await db.customers.CountAsync();
            var customerList = await db.customers
                .OrderBy(c => c.customer_id)
                .Skip((customerPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Product filtering
            var productsQuery = db.products.Include(p => p.brand).Include(p => p.category);

            if (!string.IsNullOrEmpty(brandFilter))
                productsQuery = productsQuery.Where(p => p.brand.brand_name == brandFilter);
            if (!string.IsNullOrEmpty(categoryFilter))
                productsQuery = productsQuery.Where(p => p.category.category_name == categoryFilter);

            var totalProducts = await productsQuery.CountAsync();
            var productList = await productsQuery
                .OrderBy(p => p.product_id)
                .Skip((productPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allProducts = await productsQuery.ToListAsync();
            var productImages = AssignRandomImagesToProducts(allProducts);

            // Populate ViewBag for dropdowns
            ViewBag.Stores = await GetStoresListAsync();
            ViewBag.Brands = await GetBrandsListAsync();
            ViewBag.Categories = await GetCategoriesListAsync();

            var vm = new HomeViewModel
            {
                StaffList = staffList,
                CustomerList = customerList,
                ProductList = productList,
                Brands = await db.brands.Select(b => b.brand_name).Distinct().ToListAsync(),
                Categories = await db.categories.Select(c => c.category_name).Distinct().ToListAsync(),
                StaffPage = staffPage,
                CustomerPage = customerPage,
                ProductPage = productPage,
                TotalStaff = totalStaff,
                TotalCustomers = totalCustomers,
                TotalProducts = totalProducts,
                SelectedBrand = brandFilter,
                SelectedCategory = categoryFilter,
                ProductImages = productImages
            };

            return View(vm);
        }

        // Assign images to the bikes
        private Dictionary<int, string> AssignRandomImagesToProducts(List<product> products)
        {
            var productImages = new Dictionary<int, string>();

            foreach (var product in products)
            {
                // Product gets the same image everytime
                int imageNumber = (Math.Abs(product.product_id.GetHashCode()) % 18) + 1;

                string imagePath = $"~/Content/Bike-Images/bike{imageNumber}.jpeg";
                productImages[product.product_id] = imagePath;
            }

            return productImages;
        }

        // Gets the lists to display in the dropdowns
        private async Task<SelectList> GetStoresListAsync()
        {
            try
            {
                var stores = await db.stores
                    .Select(s => new
                    {
                        Value = s.store_id.ToString(),
                        Text = s.store_name
                    })
                    .ToListAsync();

                return new SelectList(stores, "Value", "Text");
            }
            catch
            {
                return new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Store 1" },
                    new SelectListItem { Value = "2", Text = "Store 2" },
                    new SelectListItem { Value = "3", Text = "Store 3" }
                }, "Value", "Text");
            }
        }

        private async Task<SelectList> GetBrandsListAsync()
        {
            try
            {
                var brands = await db.brands
                    .Select(b => new
                    {
                        Value = b.brand_id.ToString(),
                        Text = b.brand_name
                    })
                    .ToListAsync();

                return new SelectList(brands, "Value", "Text");
            }
            catch
            {
                return new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Trek" },
                    new SelectListItem { Value = "2", Text = "Haro" },
                    new SelectListItem { Value = "3", Text = "Electra" },
                    new SelectListItem { Value = "4", Text = "Pure Cycles" }
                }, "Value", "Text");
            }
        }

        private async Task<SelectList> GetCategoriesListAsync()
        {
            try
            {
                var categories = await db.categories
                    .Select(c => new
                    {
                        Value = c.category_id.ToString(),
                        Text = c.category_name
                    })
                    .ToListAsync();

                return new SelectList(categories, "Value", "Text");
            }
            catch
            {
                return new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Mountain Bikes" },
                    new SelectListItem { Value = "2", Text = "Road Bikes" },
                    new SelectListItem { Value = "3", Text = "Cruisers" }
                }, "Value", "Text");
            }
        }
    }
}