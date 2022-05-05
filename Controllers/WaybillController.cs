using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Triton.BusinessOnline.Models;
using Triton.Interface.BusinessOnline;
using Triton.Interface.CRM;
using Triton.Interface.Waybill;
using Triton.Model.CRM.Custom;
using Triton.Core;
using Triton.Interface.Documents;
using Triton.Interface.TritonGroup;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Triton.Model.CRM.Tables;
using WaybillSearchModel = Triton.BusinessOnline.Models.WaybillSearchModel;

namespace Triton.BusinessOnline.Controllers
{
    [Authorize]
    public class WaybillController : Controller
    {
        private readonly IBusinessOnline _boService;
        private readonly IWaybill _waybillService;
        private readonly ITransportTypes _transportTypeService;
        private readonly IUserMap _userMapService;
        private readonly IDocuments _documents;
        private readonly IWaybillAPI _waybillApi;
        private readonly IWaybillQueryMaster _waybillQueryMaster;
        private readonly ILookUpCodes _lookUpCodes;


        public WaybillController(IBusinessOnline boService, IWaybill waybillService, ITransportTypes transportTypeService, IUserMap userMapService, IWaybillAPI waybillApi, IDocuments documents, IWaybillQueryMaster waybillQueryMaster, ILookUpCodes lookUpCodes)
        {
            _waybillService = waybillService;
            _boService = boService;
            _transportTypeService = transportTypeService;
            _userMapService = userMapService;
            _waybillApi = waybillApi;
            _documents = documents;
            _waybillQueryMaster = waybillQueryMaster;
            _lookUpCodes = lookUpCodes;
        }

        public async Task<IActionResult> Search()
        {
            var model = new WaybillSearchModel
            {
                WaybillSearchItemList = await _lookUpCodes.GetTritonGroupLookUpCodesByLookupcodeIDs("431,432")
            };
            return View(model);
        }

        public async Task<IActionResult> Info(long waybillId)
        {
            try
            {
                var model = await _waybillService.GetWaybillInfoById(waybillId);
                if (model != null) return View(model);

                ViewData["Header"] = "Waybill not <span class='font-weight-semi-bold'>found</span>";
                ViewData["Message"] = "Sorry we could not find the waybill details";
            }
            catch
            {
                ViewData["Header"] = "Waybill not <span class='font-weight-semi-bold'>found</span>";
                ViewData["Message"] = "Sorry we could not find the waybill details";
            }

            return View("~/Views/Shared/Error.cshtml");
        }

        public async Task<IActionResult> WaybillListCategory(string customerId, string type, int totalWaybills)
        {
            var model = new WaybillCategoryModel
            {
                WaybillInfo = await _boService.GetCustomerDeliveriesByStatus(customerId, type),
                Category = type,
                TotalWaybills = totalWaybills
            };

            model.DeliveredPerc = (int)((double)model.WaybillInfo.Count / totalWaybills * 100);
            model.OutstandingPerc = 100 - model.DeliveredPerc;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Search(WaybillSearchModel wayBillmodel)
        {
            try
            {
                var waybillNo = "";
                var customerXRef = "";
                int? waybillId = null;

                if (wayBillmodel.LookUpCodeID == "431")
                {
                    waybillNo = wayBillmodel.SearchText;
                    customerXRef = null;
                }
                else if (wayBillmodel.LookUpCodeID == "432")
                {
                    waybillNo = null;
                    customerXRef = wayBillmodel.SearchText;
                }

                var model = new WaybillSearchModel
                {
                    VwOpsWaybill = await _waybillService.GetWaybillViewList(User.GetCustomerIds(), waybillNo, customerXRef, waybillId),
                    WaybillSearchItemList = await _lookUpCodes.GetTritonGroupLookUpCodesByLookupcodeIDs("431,432"),
                    ShowReport = true
                };

                model.WaybillStatusTypes = from b in await _waybillService.GetWaybillViewList(User.GetCustomerIds(), waybillNo, customerXRef, waybillId)
                                           group b by b.WaybillStatus into g
                                           select g.First();

                switch (model.VwOpsWaybill.Count)
                {
                    case 0:
                        // No Records Found
                        ModelState.AddModelError("CustomError", "No Records Found");
                        return View(model);
                    case 1:
                        // ReSharper disable once PossibleNullReferenceException
                        return RedirectToAction("Info", new { waybillId = model.VwOpsWaybill.FirstOrDefault().WaybillID });
                    default:
                        return View(model);
                }
            }

            catch (HttpRequestException e)
            {
                ModelState.AddModelError("error", e.Message);
                ViewData["Header"] = "Waybill not <span class='font-weight-semi-bold'>found</span>";
                ViewData["Message"] = "Sorry we could not find the waybill details";
                return View("~/Views/Shared/Error.cshtml");
            }
        }


        public async Task<IActionResult> SaveQuery(long waybillId, string query, int queryStatusLcid)
        {
            try
            {
                var model = new WaybillQueryMasterInsertModel
                {
                    Query = query,
                    WaybillId = waybillId,
                    CreatedByUserId = User.GetUserId(),
                    QueryStatusLcid = queryStatusLcid,
                    Resolution = null,
                    SystemId = 0,
                    UserId = User.GetUserId()
                };


                var result = await _waybillQueryMaster.PostWaybillQueryMaster(model);
                if (result)
                {
                    ViewData["Message"] = HtmlHelpers.Success;
                    var waybillModel = await _waybillService.GetWaybillInfoById(waybillId);
                    if (waybillModel != null) return View("Info", waybillModel);
                }
            }
            catch
            {
                ViewData["Header"] = "Waybill not <span class='font-weight-semi-bold'>found</span>";
                ViewData["Message"] = "Sorry we could not find the waybill details";
            }

            return View("~/Views/Shared/Error.cshtml");
        }

        public async Task<IActionResult> DeleteWaybillQuery(long waybillId)
        {
            try
            {
                var result = await _waybillQueryMaster.Delete(waybillId, User.GetUserId());
                if (result)
                {
                    ViewData["Message"] = HtmlHelpers.Success;
                    var waybillModel = await _waybillService.GetWaybillInfoById(waybillId);
                    if (waybillModel != null) return View("Info", waybillModel);
                }
            }
            catch
            {
                ViewData["Header"] = "Waybill not <span class='font-weight-semi-bold'>found</span>";
                ViewData["Message"] = "Sorry we could not find the waybill details";
            }

            return View("~/Views/Shared/Error.cshtml");
        }

        public async Task<ActionResult> PrintWaybillSticker(string waybillNo)
        {
            var x = await _waybillApi.GetStickersForWaybillasPDF(waybillNo);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = $"StickersFor{waybillNo}.pdf",
                Inline = false,
            };

            return File(x.ImgData, "application/pdf");
        }

        public async Task<ActionResult> PrintWaybillPODSticker(string waybillNo)
        {
            var x = await _waybillApi.GetPODStickersForWaybillasPDF(waybillNo);

            //var cd = new System.Net.Mime.ContentDisposition
            //{
            //    FileName = $"PODStickersFor{waybillNo}.pdf",
            //    Inline = false,
            //};

            return File(x.ImgData, "application/pdf", $"PODStickersFor{waybillNo}.pdf");
        }

        public async Task<ActionResult> PrintEWaybill(string waybillNo)
        {
            var x = await _waybillApi.GetEWaybillasPDF(waybillNo);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = $"{waybillNo}.pdf",
                Inline = false,
            };
            return File(x.ImgData, "application/pdf");
        }


        public async Task<ActionResult> PrintWaybillPOD(string waybillNo, string NodeName = "Final")
        {
            var x = await _waybillApi.GetWaybillPODImage(waybillNo, NodeName);
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = $"{waybillNo}.pdf",
                Inline = false,
            };
            return File(x, "application/pdf");
        }

        public async Task<ActionResult> PrintSignature(string waybillNo)
        {
            try
            {
                var x = await _waybillApi.GetWaybillSignature(waybillNo);
                if (x != null)
                {
                    var cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = $"Signature.jpg",
                        Inline = false,
                    };
                    return File(x.Signature, "image/jpg");
                }

                ViewData["Header"] = "<h4>Not found</h4>";
                ViewData["Message"] = "Sorry we could not find the signature";
                return View("~/Views/Shared/Error.cshtml");

                //throw new Exception();
            }
            catch
            {
                ViewData["Header"] = "Signature image not <span class='font-weight-semi-bold'>found</span>";
                ViewData["Message"] = "The digital signature image is unable to be retreived.  Please contact Triton Express";
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new TritonWaybillSubmitModels
            {
                TransportTypes = (await _transportTypeService.GetAllTransportTypes()).Where(f => f.TransportTypeID != 6).ToList(),
                AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers,
                ServiceType = "T4",
                CreatorName = User.GetUserEmail(),
                SenderContactEmail = User.GetUserEmail(),
                SenderContactName = User.FindFirst("FullName").Value
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TritonWaybillSubmitModels model)
        {
            var lines = JsonConvert.DeserializeObject<List<TransportPriceLineItemModels>>(WebUtility.UrlDecode(model.LinesJson));
            model.Lines = lines;
            model.TransportTypes = await _transportTypeService.GetAllTransportTypes();
            model.AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
            model.CustCode = model.AllowedCustomerList.Find(x => x.CustomerID == model.CustomerID).AccountCode;
            model.TransportTypes = model.TransportTypes.Where(f => f.TransportTypeID != 6).ToList();


            model.ServiceType =  model.TransportTypes.Find(m => m.TransportTypeID == int.Parse(model.ServiceType))?.Description.Trim();
            var waybill = await _waybillApi.PostInternalWaybill(model);
            //var waybill = await _waybillApi.PostInternalWaybillUAT(model);

            if (waybill.ReturnCode == "0000")
            {
                var x = JsonConvert.DeserializeObject<InternalWaybills>(waybill.DataObject.ToString());
                ViewData["Header"] = "Successfully saved";
                ViewData["Message"] = $"Thank you for creating a new waybill.  We will provide a confirmation of this waybill.<h6>Waybill No:  {waybill.Reference}</h6>";
                ViewData["Url"] = $"{Request.Path}";

                return View("~/Views/Shared/_Success.cshtml");
                //ViewData["Message"] = $"<div class='alert alert-success' role='alert'> Waybill {x.ReferenceNo} successfully created, a copy will be emailed to you shortly.</div>";
                //return RedirectToAction("Info", new { x.WaybillID });
            }

            ViewData["Message"] = "<div class='alert alert-danger' role='alert'> Waybill not created! Error code - " + waybill.ReturnCode + " - " + waybill.ReturnMessage + ". </div>";
            return View(model);

            //var iWaybill = await PostAsJsonRequest<InternalWaybills>("Waybills", "PostInternalWaybillUAT", new InternalWaybills
            //{
            //    CreatedOn = DateTime.Now,
            //    CreatedByUserID = profileData.UserID,
            //    CreatedByGroupUserID = profileData.GroupUserID,
            //    ReceiverCell=model.ReceiverContactCell,
            //    ReceiverContact=model.ReceiverContactName,
            //    ReceiverEmail=model.ReceiverContactEmail,
            //    ReceiverTel=model.ReceiverContactTel,
            //    SenderCell=model.SenderContactCell,
            //    SenderContact=model.SenderContactName,
            //    SenderEmail=model.SenderContactEmail,
            //    SenderTel=model.SenderContactTel
            //});
            //if (!String.IsNullOrEmpty(iWaybill.ReferenceNo))
            //{
            //    model.WaybillNo = iWaybill.ReferenceNo;
            //    var results = PostAsJsonAsyncReturnDynamic("FreightwareWS", "PostWaybillUAT", model);
            //    FWResponsePacket packet = JsonConvert.DeserializeObject<FWResponsePacket>(results.ToString());
            //    if (packet.ReturnCode == "0000")
            //    {
            //        var pWaybill = GetAsync<Waybills>("", "GetWaybillByWaybillNo", $"WaybillNo={iWaybill.ReferenceNo}&DBName=CRMTest");
            //        if (pWaybill != null)
            //        {
            //            iWaybill.WaybillID = pWaybill.WaybillID;
            //            PutAsJsonAsync<InternalWaybills>("Waybills", "PutInternalWaybillUAT", iWaybill);
            //        }
            //        #region SendEmail
            //        var x = await GetAsyncRequest<TritonModel.TritonGroup.Tables.DocumentRepositories>("Waybills", "GetEWaybillasPDFUAT", $"WaybillNo={model.WaybillNo}");
            //        List<String> sb = new List<String> { profileData.GroupEmail };
            //        if (!String.IsNullOrEmpty(model.SenderContactEmail) && model.SenderContactEmail != profileData.GroupEmail)
            //            sb.Add(model.SenderContactEmail);
            //        if (!String.IsNullOrEmpty(model.ReceiverContactEmail) && model.ReceiverContactEmail != profileData.GroupEmail)
            //            sb.Add(model.ReceiverContactEmail);
            //        EmailSender.SendEmail(sb.ToArray(), "no-reply@tritonexpress.co.za", $"Please find attached a copy of the waybill document for {model.WaybillNo}. Also note the PIN for this transaction is {iWaybill.Pin}.",
            //            $"Attention for Waybill {model.WaybillNo}", ConfigurationManager.AppSettings["SMTP"], new List<System.Net.Mail.Attachment>{
            //                new System.Net.Mail.Attachment(new MemoryStream(x.ImgData),x.ImgName,x.ImgContentType)
            //                }); ;

            //        #endregion
            //        ViewBag.Message = $"<div class='alert alert-success' role='alert'> Waybill {packet.DataObject.WaybillNo} succesfully created, a copy will be emailed to you shortly.</div>";
            //        return RedirectToAction("View", new { pWaybill.WaybillID });
            //    }
            //    else
            //        ViewBag.Message = $"<div class='alert alert-danger' role='alert'> Waybill not created! Freightware error code - " + packet.ReturnCode + " - " + packet.ReturnMessage + ". </div>";
            //}
            //else
            //    ViewBag.Message = $"<div class='alert alert-danger' role='alert'> Internal Waybill could not be created. Please contact IT! </div>";
            //return View(model);
        }

        /// <summary>
        /// This method writes the percentage form of a double to the console.
        /// </summary>
        static string DisplayPercentage(double ratio)
        {
            return $"{ratio:0%}";
        }
    }
}

