using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Triton.BusinessOnline.Models;
using Triton.Core;
using Triton.Interface.CRM;

namespace Triton.BusinessOnline.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ICustomers _customerService;

        public ReportController(ICustomers customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> CustomerAssessment(int CustomerID, string Name)
        {
            if(Name != null)
            {
                var model = new CustomerAssessmentModel
                {
                    CustomerList = await _customerService.FindCrmCustomerByIds(User.GetCustomerIds()), // Get a list of customers as a filter
                    CustomerName = Name
                };

                if (model.CustomerList != null)
                {
                    // Set the default to the first value in the list

                    // ReSharper disable once PossibleNullReferenceException - already checked for null
                    //var defaultCustomerId = model.CustomerList.FirstOrDefault().CustomerID;

                    // Weekly Assessment
                    //var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek).ToShortDateString();
                    //var saturday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).ToShortDateString();
                    //model.WeekAssessment = await _customerService.GetCustomerAssessment(defaultCustomerId, sunday, saturday);

                    // TODO:  Remove hardcoding
                    model.WeeklyCustomerAssessment = await _customerService.GetCustomerAssessment(CustomerID, DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek).ToShortDateString(), DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).ToShortDateString());

                    model.MonthlyCustomerAssessment = await _customerService.GetCustomerAssessment(CustomerID, new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString(), new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).ToShortDateString());

                    model.CustomerAssessment = await _customerService.GetCustomerAssessment(CustomerID, new DateTime(DateTime.Now.Year, 1, 1).ToShortDateString(), new DateTime(DateTime.Now.Year, 12, 31).ToShortDateString());

                    model.SelectedDatePeriod = "Weekly";

                    return View(model);
                }
            }
            else
            {
                var model = new CustomerAssessmentModel
                {
                    CustomerList = await _customerService.FindCrmCustomerByIds(User.GetCustomerIds()), // Get a list of customers as a filter
                };

                if (model.CustomerList != null)
                {
                    // Set the default to the first value in the list

                    // ReSharper disable once PossibleNullReferenceException - already checked for null
                    var defaultCustomerId = model.CustomerList.FirstOrDefault().CustomerID;

                    // Weekly Assessment
                    //var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek).ToShortDateString();
                    //var saturday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).ToShortDateString();
                    //model.WeekAssessment = await _customerService.GetCustomerAssessment(defaultCustomerId, sunday, saturday);

                    // TODO:  Remove hardcoding
                    model.WeeklyCustomerAssessment = await _customerService.GetCustomerAssessment(defaultCustomerId, DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek).ToShortDateString(), DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).ToShortDateString());

                    model.MonthlyCustomerAssessment = await _customerService.GetCustomerAssessment(defaultCustomerId, new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString(), new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).ToShortDateString());

                    model.CustomerAssessment = await _customerService.GetCustomerAssessment(defaultCustomerId, new DateTime(DateTime.Now.Year, 1, 1).ToShortDateString(), new DateTime(DateTime.Now.Year, 12, 31).ToShortDateString());

                    model.SelectedDatePeriod = "Weekly";

                    model.CustomerName = model.CustomerList.FirstOrDefault().Name;

                    return View(model);
                }
            }
           
            // Show message not assign to any customers
            ViewData["ErrorHeader"] = "Customer not <span class='font-weight-semi-bold'>found</span>";
            ViewData["ErrorMessage"] = "You are not assigned to any customers.  Please contact Triton Express";
            return View("~/Views/Shared/CustomError.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> CustomerAssessment(CustomerAssessmentModel model)
        {
            // Get a list of customers as a filter
            model.CustomerList = await _customerService.FindCrmCustomerByIds(User.GetCustomerIds());

            var startDate = new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = new DateTime(DateTime.Now.Year, 12, 31);

            model.WeeklyCustomerAssessment = await _customerService.GetCustomerAssessment(int.Parse(model.SelectedCustomerId), DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek).ToShortDateString(), DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).ToShortDateString());

            model.MonthlyCustomerAssessment = await _customerService.GetCustomerAssessment(int.Parse(model.SelectedCustomerId), new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString(), new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).ToShortDateString());

            model.CustomerAssessment = await _customerService.GetCustomerAssessment(int.Parse(model.SelectedCustomerId), startDate.ToShortDateString(), endDate.ToShortDateString());

            return View(model);
        }
    }
}