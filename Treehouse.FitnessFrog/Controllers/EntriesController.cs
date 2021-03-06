﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Treehouse.FitnessFrog.Data;
using Treehouse.FitnessFrog.Models;

namespace Treehouse.FitnessFrog.Controllers
{
    public class EntriesController : Controller
    {
        private EntriesRepository _entriesRepository = null;

        public EntriesController()
        {
            _entriesRepository = new EntriesRepository();
        }

        public ActionResult Index()
        {
            List<Entry> entries = _entriesRepository.GetEntries();

            // Calculate the total activity.
            double totalActivity = entries
                .Where(e => e.Exclude == false)
                .Sum(e => e.Duration);

            // Determine the number of days that have entries.
            int numberOfActiveDays = entries
                .Select(e => e.Date)
                .Distinct()
                .Count();

            ViewBag.TotalActivity = totalActivity;
            ViewBag.AverageDailyActivity = (totalActivity / (double)numberOfActiveDays);
            if (TempData["SuccessMessage"] != null) { ViewBag.SuccessMessage = TempData["SuccessMessage"]; }
            return View(entries);
        }

        public ActionResult Add()
        {


            var entry = new Entry();
            
            entry.Date = DateTime.Today.Date;
            SetupActivitiesSelectList();
            return View(entry);
        }

        [HttpPost]
        public ActionResult Add(Entry entry)
        {
            ValidateForm(entry);

            if (ModelState.IsValid)
            {
                List<Entry> entries = _entriesRepository.GetEntries();
                List<int> ids = new List<int>();
                foreach (Entry e in entries)
                {
                    ids.Add(e.Id);
                }
                int newId = ids.Max() + 1;
                entry.Id = newId;

                ConnectDb c = new ConnectDb();
                c.AddEntry(entry);
                TempData["SuccessMessage"] = "Your new entry was successfully added!";
                return RedirectToAction("Index");
            }

            SetupActivitiesSelectList();
            return View(entry);
        }

        private void SetupActivitiesSelectList()
        {
            ViewBag.ActivitySelectList = new SelectList(Treehouse.FitnessFrog.Data.Data.Activities, "Id", "Name");
        }

        private void ValidateForm(Entry entry)
        {
            if (ModelState.IsValidField("Duration") && entry.Duration <= 0)
            {
                ModelState.AddModelError("Duration", "The Duration field value must be greater than 0.");
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SetupActivitiesSelectList();

            try
            {
                int i = Convert.ToInt32(id);
                ConnectDb c = new ConnectDb();
                return View(c.GetEntry(i));
            }
            catch (Exception e)
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult Edit(Entry entry)
        {
            ValidateForm(entry);

            if (ModelState.IsValid)
            {
                _entriesRepository.UpdateEntry(entry);
                TempData["SuccessMessage"] = "Your changes were successfully saved!";
                return RedirectToAction("Index");
            }
            SetupActivitiesSelectList();

            return View(entry);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SetupActivitiesSelectList();

            try { 
                int i = Convert.ToInt32(id);
                return View(_entriesRepository.GetEntry(i));
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Delete(Entry entry)
        {
            _entriesRepository.DeleteEntry(entry.Id);
            TempData["SuccessMessage"] = "Undesirable entry successfully deleted!";
            return RedirectToAction("Index");
        }
                
        public ActionResult CreateDb()
        {
            ConnectDb c = new ConnectDb();
            c.CreateDb();
            return RedirectToAction("Index");
        }
                
        public ActionResult DropDb()
        {
            ConnectDb c = new ConnectDb();
            c.DropDb();
            return RedirectToAction("Index");
        }
    }
}