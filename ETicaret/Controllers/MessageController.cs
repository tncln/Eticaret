using ETicaret.Filter;
using ETicaret.Models.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaret.Controllers
{
    [MyAuthorization]
    public class MessageController : BaseController
    {
        // GET: Message
        [HttpGet]
        public ActionResult i()
        {
            var currentId = CurrentUserId();
            Models.Message.iModels model = new Models.Message.iModels();
            #region Select List Item
            model.Users = new List<SelectListItem>();
            var users = context.Members.Where(x => ((int)x.MemberType) > 0 && x.Id != currentId).ToList();
            model.Users = users.Select(X => new SelectListItem()
            {
                Value = X.Id.ToString(),
                Text = string.Format("{0} {1} ({2})", X.Name, X.Surname, X.MemberType.ToString())
            }).ToList();

            #endregion
            #region Mesaj listesi
            var mList = context.Messages.Where(x => x.ToMemberId == currentId || x.MessageReplies.Any(y => y.Member_Id == currentId)).ToList();
            model.Messages = mList;
            #endregion
            return View(model);
        }
        [HttpPost]
        public ActionResult SendMessage(Models.Message.SendMessageModel message)
        {
            DB.Messages mesaj = new DB.Messages()
            {
                AddedDate = DateTime.Now,
                IsRead = false,
                Subject = message.Subject,
                ToMemberId = message.ToUserId
            };
            var mRep = new DB.MessageReplies()
            {
                AddedDate = DateTime.Now,
                Member_Id = CurrentUserId(),
                Text = message.MessageBody
            };
            mesaj.MessageReplies.Add(mRep);
            context.Messages.Add(mesaj);
            context.SaveChanges();
            return RedirectToAction("i", "Message");
        }
        [HttpGet]
        public ActionResult MessageReplies(string id)
        {
            var currentId = CurrentUserId();
            var str = int.Parse(id);
            DB.Messages message = context.Messages.FirstOrDefault(x => x.Id == str);
            if (message.ToMemberId == currentId)
            {
                message.IsRead = true;
                context.SaveChanges();
            }

            MessageRepliesModel model = new MessageRepliesModel();

            model.MReplies = context.MessageReplies.Where(x => x.MessageId == str).OrderBy(x => x.AddedDate).ToList();
            return View(model);
        }
        [HttpPost]
        public ActionResult MessageReplies(DB.MessageReplies message)
        {
            message.AddedDate = DateTime.Now;
            message.Member_Id = CurrentUserId();
            context.MessageReplies.Add(message);
            context.SaveChanges();
            return RedirectToAction("MessageReplies", "Message", new { id = message.MessageId });
        }
        [HttpGet]
        public ActionResult RenderMessage()
        {
            RenderMessageModel model = new RenderMessageModel();
            var currentId = CurrentUserId();
            var mList = context.Messages.Where(x => x.ToMemberId == currentId || x.MessageReplies.Any(y => y.Member_Id == currentId)).OrderByDescending(x => x.AddedDate);
            model.Messages = mList.Take(4).ToList();
            model.Count = mList.Count();
            return PartialView("_Message", model);
        }

        public ActionResult RemoveMessageReplies(string id)
        {
            int uid = int.Parse(id);
            //Mesajın cevapları Silme..
            var mReplies = context.MessageReplies.Where(x => x.MessageId == uid);
            context.MessageReplies.RemoveRange(mReplies);
            //Mesajın kendisini silme..
            var message = context.Messages.FirstOrDefault(x => x.Id == uid);
            context.Messages.Remove(message);
            context.SaveChanges();

            return RedirectToAction("i", "Message");
        }
    }
}