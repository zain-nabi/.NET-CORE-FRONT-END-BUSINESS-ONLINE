using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Triton.Core;
using Triton.Interface.Collection;
using Triton.Interface.CRM;
using Triton.Interface.TritonGroup;
using Triton.Model.CRM.Custom;
using Triton.Model.CRM.Tables;

namespace Triton.BusinessOnline.Controllers
{
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly ICustomers _customers;
        private readonly ITransportTypes _transportTypes;
        private readonly IUserMap _userMapService;
        private readonly ICollectionRequest _collectionRequest;
        private readonly ICollectionRequestAPI _collectionRequestApi;
        private readonly ICollectionRequestTrackAndTraces _collectionRequestTrackAndTraces;


        public CollectionController(ICustomers customers, ITransportTypes transportTypes, IUserMap userMapService, ICollectionRequest collectionRequest, ICollectionRequestTrackAndTraces collectionRequestTrackAndTraces, ICollectionRequestAPI collectionRequestApi)
        {
            _customers = customers;
            _transportTypes = transportTypes;
            _userMapService = userMapService;
            _collectionRequest = collectionRequest;
            _collectionRequestTrackAndTraces = collectionRequestTrackAndTraces;
            _collectionRequestApi = collectionRequestApi;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new CollectionRequestsModel
            {
                AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Index(string customerXRef, DateTime? StartDate, DateTime? EndDate, string CollectionRequestNumber, int customerId, CollectionRequestsModel collectionRequestsModel)
        {
            try
            {
                var dateSplit = collectionRequestsModel.FilterDate.Split("-");

                StartDate = Convert.ToDateTime(dateSplit[0].Trim());
                EndDate = Convert.ToDateTime(dateSplit[1].Trim());

                var model = await _collectionRequest.FindCollectionRequest(customerXRef, StartDate, EndDate, CollectionRequestNumber, customerId);
                model.AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
                model.ShowReport = true;
                return View(model);
            }
            catch (HttpRequestException e)
            {
                ModelState.AddModelError("error", e.Message);
                ViewData["Header"] = "Collection not found";
                ViewData["Message"] = "Sorry we could not find the Collection details";
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        [HttpGet]
        public async Task<IActionResult> View(int collectionRequestId)
        {
            try
            {
                var model = await _collectionRequest.GetComplex(collectionRequestId);
                var collectionTnt = await _collectionRequestTrackAndTraces.FindCollectionRequest(collectionRequestId);

                if (collectionTnt.Count != 0)
                {
                    model.CollectionRequestTrackandTracesModel = collectionTnt;
                }
                return View(model);
            }
            catch (HttpRequestException e)
            {
                ModelState.AddModelError("error", e.Message);
                ViewData["Header"] = "Collection not <span class='font-weight-semi-bold'>found</span>";
                ViewData["Message"] = "Sorry we could not find the Collection details";
                return View("~/Views/Shared/Error.cshtml");
            }

        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CollectionRequestsModel model = new CollectionRequestsModel
            {
                CollectionRequests = new CollectionRequests
                {
                    ServiceTypeID = 1,
                    DateCollReq = DateTime.Now,
                    TimeCollBefore = "1700",
                    TimeCollAfter = "1500"
                },
                TransportTypes = await _transportTypes.GetAllTransportTypes(),
                AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers,

            };
            if (model.AllowedCustomerList.Count == 0)
            {
                Customers cash = await _customers.GetCrmCustomerById(500);
                model.AllowedCustomerList.Add(cash);
            }
            model.TransportTypes = model.TransportTypes.Where(f => f.TransportTypeID != 6).ToList();
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> Create(CollectionRequestsModel model)
        {
            if (ModelState.IsValid)
            {
                model.TransportTypes = await _transportTypes.GetAllTransportTypes();
                model.AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
                if (model.AllowedCustomerList.Find(x => x.CustomerID == model.CollectionRequests.CustomerID) == null)
                    ModelState.AddModelError("Error", "Access denied on submitted Customer");
                else
                {
                    try
                    {
                        // Format the time for FW
                        model.CollectionRequests.TimeCollBefore = model.CollectionRequests.TimeCollBefore.Replace(":", "");
                        model.CollectionRequests.TimeCollAfter = model.CollectionRequests.TimeCollAfter.Replace(":", "");

                        model.CollectionRequests.DateCollMan = null;

                        var response = await _collectionRequestApi.PostCollectionRequestProduction(model.CollectionRequests);

                        if (response.ReturnCode == "0000")
                        {
                            ViewData["Header"] = "Successfully saved";
                            ViewData["Message"] = $"Thank you for creating a collection request.  We will provide a confirmation of this collection. <h6>Collection Request No:  {response.Reference}</h6>";
                            ViewData["Url"] = $"{Request.Path}";

                            return View("~/Views/Shared/_Success.cshtml");
                        }

                        ViewData["Message"] = "Error code - " + response.ReturnCode + " - " + response.ReturnMessage;
                        return View(model);
                    }
                    catch
                    {
                        ViewData["Message"] = "Failed to save the changes.  Please contact Triton Express";
                        return View(model);
                    }
                }
            }
            else
            {
                model.TransportTypes = await _transportTypes.GetAllTransportTypes();
                model.AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
                ModelState.AddModelError("Error",
                    ModelState.Keys.SelectMany(key => ModelState[key].Errors).FirstOrDefault()?.ErrorMessage);
                return View(model);
            }

            return View(model);
        }
    }
}