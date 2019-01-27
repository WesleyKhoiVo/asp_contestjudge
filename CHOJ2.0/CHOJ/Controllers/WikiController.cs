using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CHOJ.Models;
using CHOJ.Service;

namespace CHOJ.Controllers
{
    public class WikiController : BaseController
    {
        //
        // GET: /Wiki/
        [LoginedFilter(Role = "Admin")]
        public ActionResult Management()
        {
            var model = WikiService.GetInstance().List();
            return View(model);
        }

        //
        // GET: /Wiki/Details/5

        public ActionResult Details(string title)
        {
            var model = WikiService.GetInstance().Get(title);
            if (model == null)
                return RedirectToAction("Create");
            Title = model.Title;
            var wikiEngine = new WikiEngine.WikiEngine();
            model.Body = wikiEngine.Explain(model.Body);
            return View(model);
        }

        //
        // GET: /Wiki/Create
        [LoginedFilter(Role="Admin")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Wiki/Create
        [LoginedFilter(Role="Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Create(Wiki wiki)
        {
            try
            {
                WikiService.GetInstance().Add(wiki);
                return RedirectToAction("Management");
            }
            catch
            {
                return View();
            }
        }

        [LoginedFilter(Role = "Admin")]
        public ActionResult Delete(string id)
        {
            WikiService.GetInstance().Delete(id);
            return RedirectToAction("Management");
        }

        //
        // GET: /Wiki/Edit/5
        [LoginedFilter(Role = "Admin")]
        public ActionResult Edit(string id)
        {
            var model = WikiService.GetInstance().GetById(id);
            return View(model);
        }

        //
        // POST: /Wiki/Edit/5
        [LoginedFilter(Role = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(string id, Wiki wiki)
        {
            try
            {
                WikiService.GetInstance().Update(wiki, id);
                return RedirectToAction("Management");
            }
            catch
            {
                return View();
            }
        }
    }
}
