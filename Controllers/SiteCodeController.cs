using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Triton.BusinessOnline.Models;
using Triton.Interface.Freightware;
using Triton.Interface.TritonGroup;
using Triton.Model.CRM.Custom;
using Triton.Interface.CRM;
using Triton.Model.CRM.Tables;
using Triton.Core;
using Vendor.Services.CustomModels;

namespace Triton.BusinessOnline.Controllers
{
    public class SiteCodeController : Controller
    {
        private readonly IFreightware _freightware;
        private readonly IUserMap _userMapService;
        private readonly IPostalCodes _posatlCodeService;

        public SiteCodeController(IFreightware freightware, IUserMap userMapService, IPostalCodes postalCodesService)
        {
            _freightware = freightware;
            _userMapService = userMapService;
            _posatlCodeService = postalCodesService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var siteCode = new SiteCodeModel();
            siteCode.Customers = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
            return View(siteCode);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SiteCodeModel model)
        {
            model.Customers = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
            var CustCode = model.Customers.Find(x => x.CustomerID == model.CustomerID).AccountCode;
            model.AccountCode = CustCode;
            var response = await _freightware.SetSiteProduction(model);
            string Url = "/Quotation/Create";
            string Type = Types.SaveSuccess;
            string Response = response.ReturnMessage;
            string ImgUrl = "/front-dashboard-v1.1/src/assets/svg/illustrations/hi-five.svg";
            if (response.ReturnCode != "0000")
            {
                Url = "/SiteCode/Create";
                Type = Types.SaveFailed;
                Response = response.ReturnMessage;
                ImgUrl = "/front-dashboard-v1.1/dist/assets/svg/illustrations/sorry.svg";
                return RedirectToAction("Message", "Message",new { Url = Url, Type= Type, Response = Response, ImgUrl = ImgUrl});
            }
            return RedirectToAction("Message", "Message", new { Url = Url, Type = Type, Response = Response, ImgUrl = ImgUrl });
        }

        [HttpGet]
        public async Task<IActionResult> GetPostalCodeSuburb(string suburb)
        {
            var siteCode = new SiteCodeModel();
            siteCode.PostalCodes = await _posatlCodeService.GetPostalCodes(suburb);
            return Json(new { PostalCodesList = siteCode.PostalCodes });
        }
    }
}
