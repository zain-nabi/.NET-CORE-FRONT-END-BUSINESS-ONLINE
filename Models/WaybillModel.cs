using System.Collections.Generic;
using Triton.Model.CRM.Views;
using Triton.Model.TritonGroup.Tables;
using Triton.Model.TritonOps.StoredProcs;

namespace Triton.BusinessOnline.Models
{
    public class WaybillCategoryModel
    {
        public List<proc_Customer_By_CustomerID_Tabs_Select> WaybillInfo { get; set; }
        public string Category { get; set; }
        public int TotalWaybills { get; set; }
        public int DeliveredPerc { get; set; }
        public int OutstandingPerc { get; set; }
    }

    public class WaybillSearchModel
    {
        public List<vwOpsWaybills> VwOpsWaybill { get; set; }
        public string WaybillNo { get; set; }
        public string CustomerXRef { get; set; }
        public string SearchText { get; set; }
        public string LookUpCodeID { get; set; }
        public int WaybillId { get; set; }
        public IEnumerable<LookUpCodes> WaybillSearchItemList { get; set; }
        public bool ShowReport { get; set; }
        public IEnumerable<vwOpsWaybills> WaybillStatusTypes { get; set; }
    }
}
