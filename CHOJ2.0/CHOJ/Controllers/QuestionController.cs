using System.Linq;
using System.Web.Mvc;
using CHOJ.Models;
using CHOJ.Service;

namespace CHOJ.Controllers
{
    public class QuestionController : BaseController
    {
        public ActionResult Index(string id)
        {
            ViewData["comefrom"] = Request.UrlReferrer;
            var d = QuestionService.GetInstance().All().FirstOrDefault(c => c.Id == id);
            if (d == null) throw new OJException("is not null");
            Title = d.Title;
            return View(d);
        }
        public ActionResult List(string id, int? p)
        {
            InitIntPage(ref p);
            var model = QuestionService.GetInstance().All().Where(c => c.GroupId == id);
            var group = GroupService.GetInstance().GetGroup(id);
            ViewData["group"] = group;
            Title = group.Title;
            return View(model);
        }
        [LoginedFilter(Role = "Admin")]
        public ActionResult Management(string groupId)
        {
            Title = "Question category";
            ViewData["GroupList"] = GroupService.GetInstance().GroupList();
            var model = QuestionService.GetInstance().All();
            if (!groupId.IsNullOrEmpty())
                model = model.Where(c => c.GroupId == groupId);
            return View(model);
        }
        void GetGroupList(string groupId)
        {
            var gl = GroupService.GetInstance().GroupList();
            ViewData["GroupId"] = new SelectList(gl, "Id", "Title", groupId);
        }

        [LoginedFilter(Role = "Admin")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Create(string groupId, string id)
        {
            GetGroupList(groupId);
            return View();
        }
        [LoginedFilter(Role = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Create(Question question)
        {
            QuestionService.GetInstance().Create(question);
            // GetGroupList(question.GroupId);
            Title = "Create a question.";
            return RedirectToAction("Management");
        }

        [LoginedFilter(Role = "Admin")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Delete(string id)
        {
            QuestionService.GetInstance().Delete(id);
            return RedirectToAction("Management");
        }

        [LoginedFilter(Role = "Admin")]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(string id)
        {
            var q = QuestionService.GetInstance().All().FirstOrDefault(c => c.Id == id);
            GetGroupList("");
            Title = "Edit" + q.Title;
            return View(q);
        }
        [LoginedFilter(Role = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Edit(string id, Question question)
        {
            QuestionService.GetInstance().Update(question, id);
            return RedirectToAction("Management");
        }
    }
}
