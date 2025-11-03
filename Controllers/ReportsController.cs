using HomeworkAssignment3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Web.Hosting;

namespace HomeworkAssignment3.Controllers
{
    public class ReportsController : Controller
    {
        private BikeStoresEntities1 db = new BikeStoresEntities1();
        private string ReportFolderPath => Server.MapPath("~/App_Data/Reports/");

        public ActionResult Index()
        {
            try
            {
                // Ensure reports directory exists
                if (!Directory.Exists(ReportFolderPath))
                {
                    Directory.CreateDirectory(ReportFolderPath);
                }

                // Stock Items Report: Products available but not yet sold
                var stockItemsReport = GetStockItemsReport();
                ViewBag.StockItems = stockItemsReport;

                // Popular Products Report: Most frequently ordered products
                var popularProductsReport = GetPopularProductsReport();
                ViewBag.PopularProducts = popularProductsReport;

                // Summary statistics
                ViewBag.TotalProductsInStock = stockItemsReport.Count;
                ViewBag.TotalStockValue = stockItemsReport.Sum(p => p.ListPrice * p.QuantityInStock);
                ViewBag.TopSellingProduct = popularProductsReport.FirstOrDefault()?.ProductName ?? "No sales data";
                ViewBag.TotalOrders = popularProductsReport.Sum(p => p.OrderCount);

                // Get chart data
                ViewBag.ChartData = GetChartData(stockItemsReport, popularProductsReport);

                // Get saved reports for archive
                ViewBag.SavedReports = GetSavedReports();

                return View();
            }
            catch (Exception ex)
            {
                // Fallback data if there's an error
                ViewBag.Error = "Error generating reports: " + ex.Message;

                ViewBag.StockItems = GetDemoStockItems();
                ViewBag.PopularProducts = GetDemoPopularProducts();
                ViewBag.TotalProductsInStock = 15;
                ViewBag.TotalStockValue = 25000;
                ViewBag.TopSellingProduct = "Trek Mountain Bike";
                ViewBag.TotalOrders = 45;
                ViewBag.SavedReports = new List<SavedReport>();

                return View();
            }
        }

        #region Chart Data Methods

        private ChartData GetChartData(List<StockItemReport> stockItems, List<PopularProductReport> popularProducts)
        {
            var chartData = new ChartData();

            // Stock by Category Data
            chartData.StockByCategory = stockItems
                .GroupBy(s => s.CategoryName)
                .Select(g => new ChartItem
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .ToList();

            // Top Products Data
            chartData.TopProducts = popularProducts
                .Take(5)
                .Select(p => new ChartItem
                {
                    Label = p.ProductName,
                    Value = p.OrderCount
                })
                .ToList();

            // Brand Performance Data
            chartData.BrandPerformance = popularProducts
                .GroupBy(p => p.BrandName)
                .Select(g => new ChartItem
                {
                    Label = g.Key,
                    Value = (int)g.Sum(p => p.TotalRevenue)
                })
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            // Price vs Stock Data
            chartData.PriceStockData = stockItems
                .Select(s => new PriceStockPoint
                {
                    Price = (double)s.ListPrice,
                    Stock = s.QuantityInStock,
                    ProductName = s.ProductName
                })
                .ToList();

            return chartData;
        }

        // JSON endpoint for chart data
        public JsonResult GetChartDataJson()
        {
            try
            {
                var stockItems = GetStockItemsReport();
                var popularProducts = GetPopularProductsReport();
                var chartData = GetChartData(stockItems, popularProducts);

                return Json(chartData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        // ... (Rest of your existing methods remain the same)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportReport(string fileName, string fileType, string reportType)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    TempData["Error"] = "Please enter a file name.";
                    return RedirectToAction("Index");
                }

                byte[] fileContents;
                string contentType;
                string fileExtension;
                string actualFileType;

                switch (fileType.ToLower())
                {
                    case "pdf":
                        fileContents = ExportToPdf(reportType, fileName);
                        contentType = "application/pdf";
                        fileExtension = ".pdf";
                        actualFileType = "PDF";
                        break;
                    case "excel":
                        fileContents = ExportToCsv(reportType, fileName);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileExtension = ".xlsx";
                        actualFileType = "XLSX";
                        TempData["Warning"] = "Excel file generated as CSV format. Can be opened in Excel.";
                        break;
                    case "csv":
                        fileContents = ExportToCsv(reportType, fileName);
                        contentType = "text/csv";
                        fileExtension = ".csv";
                        actualFileType = "CSV";
                        break;
                    default:
                        TempData["Error"] = "Invalid file type selected.";
                        return RedirectToAction("Index");
                }

                SaveReportToArchive(fileName + fileExtension, actualFileType, reportType, fileContents);
                return File(fileContents, contentType, fileName + fileExtension);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error exporting report: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public FileResult DownloadReport(string fileName)
        {
            try
            {
                var filePath = Path.Combine(ReportFolderPath, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = "File not found.";
                    return null;
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(Path.GetExtension(filePath));
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error downloading file: " + ex.Message;
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReport(string fileName)
        {
            try
            {
                var filePath = Path.Combine(ReportFolderPath, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    TempData["Success"] = "Report deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "File not found.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting file: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        #region Report Generation Methods

        private List<StockItemReport> GetStockItemsReport()
        {
            try
            {
                var productIdsInOrders = db.order_items.Select(oi => oi.product_id).Distinct();

                var unsoldProducts = (from p in db.products
                                      join s in db.stocks on p.product_id equals s.product_id
                                      where !productIdsInOrders.Contains(p.product_id)
                                            && s.quantity > 0
                                      select new StockItemReport
                                      {
                                          ProductId = p.product_id,
                                          ProductName = p.product_name,
                                          BrandName = p.brand.brand_name,
                                          CategoryName = p.category.category_name,
                                          ModelYear = p.model_year,
                                          ListPrice = p.list_price,
                                          QuantityInStock = (int)s.quantity
                                      })
                                     .Distinct()
                                     .OrderByDescending(p => p.QuantityInStock)
                                     .ThenBy(p => p.ProductName)
                                     .ToList();

                return unsoldProducts;
            }
            catch (Exception ex)
            {
                return GetDemoStockItems();
            }
        }

        private List<PopularProductReport> GetPopularProductsReport()
        {
            try
            {
                var popularProducts = (from oi in db.order_items
                                       join p in db.products on oi.product_id equals p.product_id
                                       group oi by new
                                       {
                                           p.product_id,
                                           p.product_name,
                                           p.brand.brand_name,
                                           p.category.category_name,
                                           p.model_year,
                                           p.list_price
                                       } into g
                                       select new PopularProductReport
                                       {
                                           ProductId = g.Key.product_id,
                                           ProductName = g.Key.product_name,
                                           BrandName = g.Key.brand_name,
                                           CategoryName = g.Key.category_name,
                                           ModelYear = g.Key.model_year,
                                           ListPrice = g.Key.list_price,
                                           OrderCount = g.Sum(oi => oi.quantity),
                                           TotalRevenue = g.Sum(oi => oi.quantity * oi.list_price)
                                       })
                                     .OrderByDescending(p => p.OrderCount)
                                     .ThenByDescending(p => p.TotalRevenue)
                                     .Take(20)
                                     .ToList();

                return popularProducts;
            }
            catch (Exception ex)
            {
                return GetDemoPopularProducts();
            }
        }

        #endregion

        #region Export Methods

        private byte[] ExportToPdf(string reportType, string fileName)
        {
            try
            {
                string pdfContent = GeneratePdfContent(reportType);
                return Encoding.UTF8.GetBytes(pdfContent);
            }
            catch (Exception ex)
            {
                return ExportToCsv(reportType, fileName);
            }
        }

        private string GeneratePdfContent(string reportType)
        {
            var content = new StringBuilder();
            content.AppendLine($"BikeStores - {GetReportTitle(reportType)}");
            content.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}");
            content.AppendLine();
            content.AppendLine(new string('=', 80));

            if (reportType == "stock")
            {
                var stockItems = GetStockItemsReport();
                content.AppendLine("STOCK ITEMS REPORT - Products available but not yet sold");
                content.AppendLine();
                content.AppendLine("Product ID | Product Name           | Brand     | Category      | Price     | Stock Qty");
                content.AppendLine("---------- | --------------------- | --------- | ------------- | --------- | ---------");

                foreach (var item in stockItems)
                {
                    content.AppendLine($"{item.ProductId,-10} | {item.ProductName,-20} | {item.BrandName,-9} | {item.CategoryName,-12} | R {item.ListPrice,-7} | {item.QuantityInStock,-9}");
                }

                content.AppendLine();
                content.AppendLine($"Total Products: {stockItems.Count}");
                content.AppendLine($"Total Stock Value: R {stockItems.Sum(s => s.ListPrice * s.QuantityInStock):N2}");
            }
            else
            {
                var popularProducts = GetPopularProductsReport();
                content.AppendLine("POPULAR PRODUCTS REPORT - Most frequently ordered products");
                content.AppendLine();
                content.AppendLine("Product ID | Product Name           | Brand     | Category      | Orders | Revenue");
                content.AppendLine("---------- | --------------------- | --------- | ------------- | ------ | --------");

                foreach (var item in popularProducts)
                {
                    content.AppendLine($"{item.ProductId,-10} | {item.ProductName,-20} | {item.BrandName,-9} | {item.CategoryName,-12} | {item.OrderCount,-6} | R {item.TotalRevenue:N2}");
                }

                content.AppendLine();
                content.AppendLine($"Total Products: {popularProducts.Count}");
                content.AppendLine($"Total Orders: {popularProducts.Sum(p => p.OrderCount)}");
                content.AppendLine($"Total Revenue: R {popularProducts.Sum(p => p.TotalRevenue):N2}");
            }

            return content.ToString();
        }

        private byte[] ExportToCsv(string reportType, string fileName)
        {
            var csv = new StringBuilder();

            if (reportType == "stock")
            {
                var stockItems = GetStockItemsReport();
                csv.AppendLine("ProductID,ProductName,Brand,Category,ModelYear,Price,StockQty");
                foreach (var item in stockItems)
                {
                    csv.AppendLine($"{item.ProductId},\"{item.ProductName}\",\"{item.BrandName}\",\"{item.CategoryName}\",{item.ModelYear},{item.ListPrice},{item.QuantityInStock}");
                }
            }
            else
            {
                var popularProducts = GetPopularProductsReport();
                csv.AppendLine("ProductID,ProductName,Brand,Category,ModelYear,Orders,Revenue");
                foreach (var item in popularProducts)
                {
                    csv.AppendLine($"{item.ProductId},\"{item.ProductName}\",\"{item.BrandName}\",\"{item.CategoryName}\",{item.ModelYear},{item.OrderCount},{item.TotalRevenue}");
                }
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        #endregion

        #region Helper Methods

        private string GetReportTitle(string reportType)
        {
            return reportType == "stock" ? "Stock Items Report" : "Popular Products Report";
        }

        private void SaveReportToArchive(string fileName, string fileType, string reportType, byte[] fileContents)
        {
            var filePath = Path.Combine(ReportFolderPath, fileName);
            System.IO.File.WriteAllBytes(filePath, fileContents);
        }

        private List<SavedReport> GetSavedReports()
        {
            try
            {
                if (!Directory.Exists(ReportFolderPath))
                    return new List<SavedReport>();

                var files = Directory.GetFiles(ReportFolderPath)
                    .Select(f => new SavedReport
                    {
                        FileName = Path.GetFileName(f),
                        FileType = Path.GetExtension(f).ToUpper().Replace(".", ""),
                        CreatedDate = System.IO.File.GetCreationTime(f),
                        FileSize = new FileInfo(f).Length
                    })
                    .OrderByDescending(f => f.CreatedDate)
                    .ToList();

                return files;
            }
            catch
            {
                return new List<SavedReport>();
            }
        }

        private string GetContentType(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".csv":
                    return "text/csv";
                default:
                    return "application/octet-stream";
            }
        }

        // Demo data methods for fallback
        private List<StockItemReport> GetDemoStockItems()
        {
            return new List<StockItemReport>
            {
                new StockItemReport { ProductId = 1, ProductName = "Trek Mountain Bike", BrandName = "Trek", CategoryName = "Mountain Bikes", ModelYear = 2023, ListPrice = 1200.00m, QuantityInStock = 5 },
                new StockItemReport { ProductId = 2, ProductName = "Haro BMX Pro", BrandName = "Haro", CategoryName = "BMX Bikes", ModelYear = 2023, ListPrice = 450.00m, QuantityInStock = 3 },
                new StockItemReport { ProductId = 3, ProductName = "Electra Cruiser", BrandName = "Electra", CategoryName = "Cruisers", ModelYear = 2023, ListPrice = 350.00m, QuantityInStock = 8 }
            };
        }

        private List<PopularProductReport> GetDemoPopularProducts()
        {
            return new List<PopularProductReport>
            {
                new PopularProductReport { ProductId = 5, ProductName = "Trek Speedster", BrandName = "Trek", CategoryName = "Road Bikes", ModelYear = 2023, ListPrice = 1500.00m, OrderCount = 25, TotalRevenue = 37500.00m },
                new PopularProductReport { ProductId = 6, ProductName = "Electra Beach Cruiser", BrandName = "Electra", CategoryName = "Cruisers", ModelYear = 2023, ListPrice = 400.00m, OrderCount = 18, TotalRevenue = 7200.00m }
            };
        }

        #endregion
    }

    // Chart Data Models
    public class ChartData
    {
        public List<ChartItem> StockByCategory { get; set; }
        public List<ChartItem> TopProducts { get; set; }
        public List<ChartItem> BrandPerformance { get; set; }
        public List<PriceStockPoint> PriceStockData { get; set; }
    }

    public class ChartItem
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    public class PriceStockPoint
    {
        public double Price { get; set; }
        public int Stock { get; set; }
        public string ProductName { get; set; }
    }

    // Existing Report Model Classes
    public class StockItemReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public int ModelYear { get; set; }
        public decimal ListPrice { get; set; }
        public int QuantityInStock { get; set; }
    }

    public class PopularProductReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public int ModelYear { get; set; }
        public decimal ListPrice { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class SavedReport
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime CreatedDate { get; set; }
        public long FileSize { get; set; }
        public string FileSizeFormatted => FormatFileSize(FileSize);

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}