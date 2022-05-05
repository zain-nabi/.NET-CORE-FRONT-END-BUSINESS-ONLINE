using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Triton.BusinessOnline.Models;

namespace Triton.BusinessOnline.Controllers
{
    public class MessageController : Controller
    {
        public IActionResult Message(string type, string url, string Response, string ImgUrl)
        {
            if (url == null)
            {
                url = "Home/Index";
            }

            var model = new MessageModel
            {
                Url = url                
            };

            switch (type)
            {
                case Types.NoRecords:
                    model.Title = "Oops";
                    model.Message = "Something has gone wrong... Contact IT";
                    model.Icon = Html.FailedIcon;
                    model.Type = "NoRecords";
                    break;
                case Types.UpdateSuccess:
                    model.Message = Html.UpdateRecordSuccessMessage;
                    model.Title = Html.UpdateRecordSuccessTitle;
                    model.Type = null;
                    break;
                case Types.UpdateFailed:
                    model.Title = Html.UpdateRecordFailedTitle;
                    model.Message = Html.UpdateRecordFailedMessage;
                    model.Icon = Html.FailedIcon;
                    model.Type = null;
                    break;
                case Types.SaveSuccess:
                    model.Title = Html.SaveRecordSuccessMessage;
                    model.Message = Response;
                    model.Icon = Html.SuccessIcon;
                    model.Type = null;
                    model.ImgUrl = ImgUrl;
                    break;
                case Types.DeleteSuccess:
                    model.Title = Html.DeleteRecordSuccessMessage;
                    model.Message = Html.DeleteRecordSuccessTitle;
                    model.Icon = Html.SuccessIcon;
                    model.Type = null;
                    break;
                case Types.SaveFailed:
                    model.Title = Html.SaveRecordFailedTitle;
                    model.Message = Response;
                    model.Icon = Html.FailedIcon;
                    model.Type = null;
                    model.ImgUrl = ImgUrl;
                    break;
            }

            return View(model);
        }
    }
}
