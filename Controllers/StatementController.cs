using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Triton.Core;
using Triton.Interface.CRM;
using Triton.Interface.Documents;
using Triton.Interface.TritonGroup;
using Triton.Model.CRM.Custom;
using Triton.Model.CRM.Tables;
using Triton.Model.TritonOps.StoredProcs;
using Vendor.Services.CustomModels;

namespace Triton.BusinessOnline.Controllers
{
    [Authorize]
    public class StatementController : Controller
    {
        private readonly IStatements _statements;
        private readonly IUserMap _userMapService;
        private readonly IDocuments _documents;
        private readonly ICustomers _customers;


        public StatementController(IStatements statements, IUserMap userMap, IDocuments documents, ICustomers customers)
        {
            _statements = statements;
            _userMapService = userMap;
            _documents = documents;
            _customers = customers;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new VendorStatementSearch()
            {
                AllowedCustomers = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int customerId, DateTime Period, VendorStatementSearch vendorStatementSearchModel)
        {

            var dateSplit = vendorStatementSearchModel.FilterDate.Split("-");

            Period = Convert.ToDateTime(dateSplit[0].Trim());

            var result = new VendorStatementSearch()
            {
                GetStatementResponseStatementOutput = (await _statements.GetCustomerStatement(customerId, Period)),
                Customers = await _customers.GetCrmCustomerById(customerId),
                AllowedCustomers = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers,
                ShowReport = true

            };
            return View(result);
        }

        public async Task<ActionResult> PrintStatement(Triton.Model.CRM.Tables.Customers customers, DateTime Period)
        {

            var x = await _documents.GetCustomerStatementByAccountCodeandPeriod(customers.AccountCode, Period);
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Statement.pdf",
                Inline = false,
            };

            return File(x, "application/pdf");
        }
    }

}