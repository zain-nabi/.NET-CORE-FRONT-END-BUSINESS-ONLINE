using System.Collections.Generic;
using Triton.Model.CRM.StoredProcs;
using Triton.Model.CRM.Tables;

namespace Triton.BusinessOnline.Models
{
    public class CustomerAssessmentModel
    {
        public List<Customers> CustomerList { get; set; }
        public CustomerAssessment CustomerAssessment { get; set; }

        public CustomerAssessment WeeklyCustomerAssessment { get; set; }

        public CustomerAssessment MonthlyCustomerAssessment { get; set; }

        public string SelectedCustomerId { get; set; }
        public string SelectedDatePeriod { get; set; }

        public string[] DatePeriodRadio = {"Weekly", "Monthly", "Yearly"};
        public bool ShowReport { get; set; }
        public string CustomerName { get; set; }
    }
}
