using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Triton.BusinessOnline.Models;
using Triton.Interface.CRM;
using Triton.Interface.Freightware;
using Triton.Interface.TritonGroup;
using Triton.Model.CRM.Tables;
using Triton.Model.TritonExpress.Tables;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Triton.BusinessOnline.Controllers
{
    [Authorize]
    public class AjaxController : Controller
    {
        private readonly IPostalCodes _postalCodes;
        private readonly ICustomers _customers;
        private readonly IFreightware _freightware;

        public AjaxController(IPostalCodes postalCodes, ICustomers customers, IFreightware freightware)
        {
            _postalCodes = postalCodes;
            _customers = customers;
            _freightware = freightware;
        }
        public async Task<ActionResult<List<PostalCodes>>> GetPostalCodes(string name)
        {
            return await _postalCodes.GetPostalCodes(name);
        }

        public async Task<ActionResult<List<Customers>>> GetCustomerSearch(string search)
        {
            return await _customers.FindCrmCustomer(search);
        }

        public async Task<ActionResult<List<SiteModel>>> GetSiteList(string customerCode, string siteCode)
        {
            var model = new List<SiteModel>();
            var result = await _freightware.GetSiteList(customerCode, siteCode);
            var returnList = JsonConvert.DeserializeObject<List<Vendor.Services.Freightware.PROD.GetSiteList.GetSiteListResponseSiteOutput>>(result.DataObject.ToString());

            foreach (var item in returnList)
            {
                var site = new SiteModel
                {
                    SiteCode = item.SiteCode,
                    SiteName = item.SiteName,
                    PostalCode = item.AddressPhysical.StreetAdd5
                };
                
                model.Add(site);
            }

            return model;
        }

        public async Task<ActionResult<List<Vendor.Services.Freightware.UAT.GetSiteList.GetSiteListResponseSiteOutput>>> GetSiteListUAT(string customerCode, string siteCode)
        {
            var x = await _freightware.GetSiteListUAT(customerCode, siteCode);
            return JsonConvert.DeserializeObject<List<Vendor.Services.Freightware.UAT.GetSiteList.GetSiteListResponseSiteOutput>>(x.DataObject.ToString());
        }

        public async Task<ActionResult<List<PostalCodes>>> GetPostalCodeName(string name)
        {
            var model = new List<PostalCodes>();
            var result = await _postalCodes.GetPostalCodes(name);

            foreach (var item in result)
            {
                var ps = new PostalCodes
                {
                    Name = item.Name,
                    Suburb = item.Suburb,
                    PostalCode = item.PostalCode
                };
                model.Add(ps);
            }
            return model;
        }
    }
}