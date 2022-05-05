using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Triton.Core;
using Triton.Interface.CRM;
using Triton.Interface.Documents;
using Triton.Interface.TritonGroup;
using Triton.Model.CRM.Custom;

namespace Triton.BusinessOnline.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {

        private readonly IInvoices _invoices;
        private readonly IUserMap _userMapService;
        private readonly IDocuments _documents;

        public InvoiceController(IInvoices invoices,  IUserMap userMapService,  IDocuments documents)
        {
            _invoices = invoices;
            _userMapService = userMapService;
            _documents = documents;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new InvoiceSearchModel
            {
                AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string InvoiceNo, DateTime Startdate, DateTime EndDate, int customerId, InvoiceSearchModel invoiceSearchModel)
        {
            try
            {
                var dateSplit = invoiceSearchModel.FilterDate.Split("-");

                Startdate = Convert.ToDateTime(dateSplit[0].Trim());
                EndDate = Convert.ToDateTime(dateSplit[1].Trim());

                var result = (await _invoices.GetInvoiceNo(InvoiceNo, customerId, Startdate, EndDate));
                result.AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
                result.ShowReport = true;
                return View(result);
            }
            catch (HttpRequestException e)
            {
                ModelState.AddModelError("error", e.Message);
                ViewData["Header"] = "Invoice not <span class='font-weight-semi-bold'>found</span>";
                ViewData["Message"] = "Sorry we could not find Invoice";
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        public async Task<ActionResult> EDocs(string InvoiceNumber)
        {
            var x = await _documents.GetCustomerInvoicebyInvoiceNumber(InvoiceNumber);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = $"{InvoiceNumber}.pdf",
                Inline = false,
            };
            return File(x, "application/pdf");
        }


       public async Task<ActionResult> ExeclDoc(string invoiceNumber, int customerId, DateTime? startDate, DateTime? endDate)
        {
            var x = await _invoices.GetExcelInvoice(invoiceNumber, customerId, startDate, endDate);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = $"{invoiceNumber}.xls",
                Inline = false,
                
            };
            return File(x.ImgData, "application/vnd.ms-excel",$"{invoiceNumber}.xls");
        }

    }
}

