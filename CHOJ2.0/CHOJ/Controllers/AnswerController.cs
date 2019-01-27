using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Data;
using CHOJ.Service;

namespace CHOJ.Controllers {
	public class AnswerController : BaseController {
		[LoginedFilter]
		public ActionResult Submit(string id) {
		    Title = "Submit your code";
			var li = ConfigSerializer.Load<List<Compiler>>("Compiler");
			ViewData["Compiler"] = new SelectList(li, "Guid", "Name");
			if (!id.IsNullOrEmpty())
				ViewData["QuestionId"] = id;
			return View();
		}
        [LoginedFilter]
        [ValidateInput(false)]
		public ActionResult SubmitProcess(string code, Guid compiler
            , string questionId) {
            if(HalfoxUser.Id.IsNullOrEmpty())
            {
                    return RedirectToAction("Index", "Profile");
            }
            var x = new OJer(HalfoxUser.Id, HalfoxUser.Name, 
                code, compiler, questionId, 
                Server.MapPath("/"));
			ThreadPool.QueueUserWorkItem(x.Start);
			return RedirectToAction("Status");
		}
        public ActionResult Status(int? p)
        {
            Title = "All Status";
            InitIntPage(ref p);
            var x = AnswerList.Answers.Select(c=>c.Value);

            var model = x.Union(AnswerService.GetInstance().Status(p.Value, 50));
            return View(model);
        }

        public ActionResult UserStatus(string uId,string userName, int? p)
        {
            InitIntPage(ref p);
            Title = userName + "'s status";
            var x = AnswerList.Answers.Select(c => c.Value).Where(c => c.UserId == uId);

            var model =  x.Union(AnswerService.GetInstance().UserStatus(uId, p.Value, 50));
            return View("Status", model);
		}
        public ActionResult QuestionStatus(string qId,string title, int? p)
        {
            InitIntPage(ref p);
            Title = title + "'s status";
            var x = AnswerList.Answers.Select(c => c.Value).Where(c => c.QuestionId == qId);

            var model =  x.Union(AnswerService.GetInstance().QuestionStatus(qId, p.Value, 50));
            return View("Status", model);
        }
	}
}
