using ProcureFlow.Web.Models;

namespace ProcureFlow.Web.ViewModels;

public class DashboardViewModel
{
    public int DraftRequests { get; set; }
    public int SubmittedRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public IReadOnlyList<PurchaseRequest> RecentRequests { get; set; } = Array.Empty<PurchaseRequest>();
}
