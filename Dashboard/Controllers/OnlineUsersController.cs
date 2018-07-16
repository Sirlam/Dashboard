using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Dashboard.Models;
using NLog;


namespace Dashboard.Controllers
{
    public class OnlineUsersController : Controller
    {
        private OnlineUsersContext db = new OnlineUsersContext();
        public Logger logger = LogManager.GetCurrentClassLogger();

        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }        

        //
        // GET: /OnlineUsers/

        public ActionResult Index()
        {
            logger.Info("Admin login attempt with id: " + User.Identity.Name + " IP: " + GetLocalIpAddress());
            return View(db.onlineusers.ToList());
        }

        //
        // GET: /OnlineUsers/Details/5

        public ActionResult Details(int id = 0)
        {
            onlineuser onlineuser = db.onlineusers.Find(id);
            if (onlineuser == null)
            {
                return HttpNotFound();
            }
            logger.Info("Admin viewed user details page of " + onlineuser.username + " with id: " + User.Identity.Name + " IP: " + GetLocalIpAddress());
            return View(onlineuser);
        }

        //
        // GET: /OnlineUsers/Create

        public ActionResult Create()
        {
            logger.Info("Admin viewed create page with id: " + User.Identity.Name + " IP: " + GetLocalIpAddress());
            return View();
        }

        //
        // POST: /OnlineUsers/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(onlineuser onlineuser)
        {
            if (ModelState.IsValid)
            {
                db.onlineusers.Add(onlineuser);
                db.SaveChanges();
                logger.Info("Admin created" + onlineuser.username + " with id: " + User.Identity.Name + " IP: " + GetLocalIpAddress());
                return RedirectToAction("Index");
            }

            return View(onlineuser);
        }

        //
        // GET: /OnlineUsers/Edit/5

        public ActionResult Edit(int id = 0)
        {
            onlineuser onlineuser = db.onlineusers.Find(id);
            if (onlineuser == null)
            {
                return HttpNotFound();
            }
            logger.Info("Admin viewed user verification page of "+ onlineuser.username +" with id: " + User.Identity.Name + " IP: " + GetLocalIpAddress());
            return View(onlineuser);
        }

        //
        // POST: /OnlineUsers/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(onlineuser onlineuser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(onlineuser).State = EntityState.Modified;
                db.SaveChanges();
                logger.Info("Admin verified / modified " + onlineuser.username + " with id: " + User.Identity.Name + " IP: " + GetLocalIpAddress());
                return RedirectToAction("Index");
            }
            return View(onlineuser);
        }

        //
        // GET: /OnlineUsers/Delete/5

        public ActionResult Delete(int id = 0)
        {
            onlineuser onlineuser = db.onlineusers.Find(id);
            if (onlineuser == null)
            {
                return HttpNotFound();
            }
            logger.Info("Admin viewed delete page with id: " + User.Identity.Name + " IP: " + GetLocalIpAddress());
            return View(onlineuser);
        }

        //
        // POST: /OnlineUsers/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            onlineuser onlineuser = db.onlineusers.Find(id);
            db.onlineusers.Remove(onlineuser);
            db.SaveChanges();
            logger.Info("Admin deleted " + onlineuser.username + " with id: " + User.Identity.Name + " IP: " + GetLocalIpAddress());
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}