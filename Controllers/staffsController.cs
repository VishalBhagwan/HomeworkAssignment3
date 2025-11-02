using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HomeworkAssignment3.Models;

namespace HomeworkAssignment3.Controllers
{
    public class staffsController : Controller
    {
        private BikeStoresEntities1 db = new BikeStoresEntities1();

        // GET: staffs
        public async Task<ActionResult> Index(bool isPartial = false)
        {
            if (isPartial)
            {
                // Return partial view without layout
                var staffs = db.staffs.Include(s => s.store).Include(s => s.staff1);
                return PartialView("_StaffPartial", await staffs.ToListAsync());
            }
            else
            {
                // Return full view with layout
                var staffs = db.staffs.Include(s => s.store).Include(s => s.staff1);
                return View(await staffs.ToListAsync());
            }
        }

        // GET: staffs/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            staff staff = await db.staffs.FindAsync(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        // GET: staffs/Create
        public ActionResult Create()
        {
            ViewBag.manager_id = new SelectList(db.staffs, "staff_id", "first_name");
            ViewBag.store_id = new SelectList(db.stores, "store_id", "store_name");
            return View();
        }

        // POST: staffs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "staff_id,first_name,last_name,email,phone,active,store_id,manager_id")] staff staff)
        {
            if (ModelState.IsValid)
            {
                db.staffs.Add(staff);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            ViewBag.manager_id = new SelectList(db.staffs, "staff_id", "first_name", staff.manager_id);
            ViewBag.store_id = new SelectList(db.stores, "store_id", "store_name", staff.store_id);
            return View(staff);
        }

        // GET: staffs/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            staff staff = await db.staffs.FindAsync(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            ViewBag.manager_id = new SelectList(db.staffs, "staff_id", "first_name", staff.manager_id);
            ViewBag.store_id = new SelectList(db.stores, "store_id", "store_name", staff.store_id);
            return View(staff);
        }

        // POST: staffs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "staff_id,first_name,last_name,email,phone,active,store_id,manager_id")] staff staff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(staff).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Maintain");
            }
            ViewBag.manager_id = new SelectList(db.staffs, "staff_id", "first_name", staff.manager_id);
            ViewBag.store_id = new SelectList(db.stores, "store_id", "store_name", staff.store_id);
            return View(staff);
        }

        // GET: staffs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            staff staff = await db.staffs.FindAsync(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        // POST: staffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            staff staff = await db.staffs.FindAsync(id);
            db.staffs.Remove(staff);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Maintain");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
