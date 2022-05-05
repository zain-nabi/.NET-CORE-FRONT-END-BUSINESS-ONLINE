using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Triton.BusinessOnline.Models;
using Triton.Core;
using Triton.Interface.BusinessOnline;
using Triton.Interface.CRM;
using Triton.Interface.Waybill;
using Vendor.Services.CustomModels;

namespace Triton.BusinessOnline.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public class TestClass
        {
            public string  Value { get; set; }
            public string  Description { get; set; }
            public bool Selected { get; set; }
        }
        //private readonly ILogger<HomeController> _logger;
        private readonly IBusinessOnline _boService;
        private readonly IWaybillQueryMaster _waybillQueryService;
        private readonly IQuotesAPI _quotesApi;

        public HomeController(IBusinessOnline boService, IWaybillQueryMaster waybillQueryService, IQuotesAPI quotesApi)
        {
            _boService = boService;
            _waybillQueryService = waybillQueryService;
            _quotesApi = quotesApi;
        }

        public IActionResult LoginRedirect()
        {
            return Redirect("../Identity/Account/Login");
        }

        public async Task<IActionResult> Index()
        {
            //return RedirectToAction("Create", "Collection");
            if (User.IsInRole("Customer"))
            {
                var model = await _boService.GetDashboardForCustomerMultiQuery(User.GetCustomerIds(), User.GetUserId(), true, null, StringHelpers.Controllers.UserMap);
                model.WaybillQueryList = await _waybillQueryService.GetWaybillQueryMaster(User.GetUserId(), "251, 252", 0);

                var x = model.DeliveryStatusCount;
                model.CustomerNameChart = string.Join(",", model.CustomerDeliveryList.Select(x => "\"" + x.CustomerName + "\"").ToArray());
                model.CustomerDeliveredChart = string.Join(",", model.CustomerDeliveryList.Select(x => x.Delivered).ToArray());
                model.CustomerOutstandingChart = string.Join(",", model.CustomerDeliveryList.Select(x => x.Outstanding).ToArray());

                model.TotalSubCategories = x.Bookings + x.Depot + x.PreviouslyDelivered + x.FutureDel + x.PreviouslyUndelivered + x.Retained;
                return View(model);
            }
            else
            {
                //Show message not assign to any customers
                ViewData["Heading"] = "No Customers";
                ViewData["ErrorHeader"] = "Customer not <span class='font-weight-semi-bold'>found</span>";
                ViewData["ErrorMessage"] = "You are not assigned to any customers.  Please contact Triton Express";
                return View("~/Views/Shared/CustomError.cshtml");
            }

        }

        public IActionResult Privacy()
        {
            try
            {
               // var model = new VendorQuoteModel {AllowedSundries = await _quotesApi.GetQuoteSurcharges()};
               var model = new List<TestClass>();

               for (int i = 0; i < 5; i++)
               {
                   var item = new TestClass
                   {
                       Description = "test" + i,
                       Value = "Value" + i,
                       Selected = false
                   };

                   model.Add(item);
               }

               return View(model);
            }
            catch
            {
                ViewData["Header"] = "404";
                ViewData["Message"] = "We are experiencing a problem with the quotations.  Please contact Triton Express";
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        [HttpPost]
        public IActionResult Privacy(List<TestClass> model)
        {
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Message(string type, string url, string title, string message)
        {
            if (url == null)
            {
                url = "Home/Index";
            }

            var model = new MessageModel
            {
                Message = Utils.StringHelper.Html.UpdateRecordSuccessMessage,
                //Route = "Index",
                // Controller = "Home",
                Title = Utils.StringHelper.Html.UpdateRecordSuccessTitle,
                Icon = "fas fa-check-circle text-success",
                Url = url
            };

            switch (type)
            {
                case Utils.StringHelper.Types.NoRecords:
                    model.Title = title ?? "Oops";
                    model.Message = message ?? "Something has gone wrong... Contact IT";
                    model.Icon =Utils.StringHelper.Html.FailedIcon;
                    model.Type = "NoRecords";
                    break;
                case Utils.StringHelper.Types.UpdateSuccess:
                    model.Message = Utils.StringHelper.Html.UpdateRecordSuccessMessage;
                    model.Title = Utils.StringHelper.Html.UpdateRecordSuccessTitle;
                    model.Type = null;
                    break;
                case Utils.StringHelper.Types.UpdateFailed:
                    model.Title = Utils.StringHelper.Html.UpdateRecordFailedTitle;
                    model.Message = Utils.StringHelper.Html.UpdateRecordFailedMessage;
                    model.Icon = Utils.StringHelper.Html.FailedIcon;
                    model.Type = null;
                    break;
                case Utils.StringHelper.Types.SaveSuccess:
                    model.Title = Utils.StringHelper.Html.SaveRecordSuccessMessage;
                    model.Message = Utils.StringHelper.Html.SaveRecordSuccessTitle;
                    model.Icon = Utils.StringHelper.Html.SuccessIcon;
                    model.Type = null;
                    break;
                case Utils.StringHelper.Types.DeleteSuccess:
                    model.Title = Utils.StringHelper.Html.DeleteRecordSuccessMessage;
                    model.Message = Utils.StringHelper.Html.DeleteRecordSuccessTitle;
                    model.Icon = Utils.StringHelper.Html.SuccessIcon;
                    model.Type = null;
                    break;
            }

            return View(model);
        }
    }
}
