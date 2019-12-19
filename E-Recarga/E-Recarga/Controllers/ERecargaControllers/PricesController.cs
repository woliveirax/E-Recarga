﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using E_Recarga.Models;
using E_Recarga.Models.ERecargaModels;
using Microsoft.AspNet.Identity;

namespace E_Recarga.Controllers.ERecargaControllers
{
    [Authorize(Roles = nameof(RoleEnum.CompanyManager) + "," + nameof(RoleEnum.Employee))]
    public class PricesController : Controller
    {
        private ERecargaDbContext db = new ERecargaDbContext();

        // GET: Prices
        public ActionResult Index()
        {
            var user = db.Employees.Find(User.Identity.GetUserId());
            IQueryable<Price> prices;

            if (User.IsInRole(nameof(RoleEnum.CompanyManager)))
            {
                prices = db.Prices.Include(p => p.Station).Where(x=>x.Station.CompanyId == user.CompanyId);
            }
            else
            {
                prices = db.Prices.Include(p => p.Station).Where(x => x.StationId == user.StationId);
            }

            return View(prices.ToList());
        }

        // GET: Prices/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Price price = db.Prices.Find(id);
            if (price == null)
            {
                return HttpNotFound();
            }

            var user = db.Employees.Find(User.Identity.GetUserId());
            if (User.IsInRole(nameof(RoleEnum.CompanyManager)))
            {
                if (price.Station.CompanyId != user.CompanyId)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                if (price.Station.Id != user.StationId)
                {
                    return HttpNotFound();
                }
            }
            ViewBag.PriceStationId = price.StationId;

            return View(price);
        }

        // POST: Prices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CostNormal,CostUltra")]Price price)
        {
            var myPrice = db.Prices.Find(price.Id);
            myPrice.CostNormal = price.CostNormal;
            myPrice.CostUltra = price.CostUltra;

            if (ModelState.IsValid)
            {
                db.Entry(myPrice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(price);
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
