using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureFlow.Web.Data;
using ProcureFlow.Web.Models;
using ProcureFlow.Web.Models.Enums;
using ProcureFlow.Web.Security;
using ProcureFlow.Web.ViewModels;
using System.Diagnostics;

namespace ProcureFlow.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var requests = _context.PurchaseRequests.AsNoTracking().AsQueryable();
            if (!User.IsInRole(AppRoles.Manager) && !User.IsInRole(AppRoles.Admin))
            {
                var userId = _userManager.GetUserId(User);
                requests = requests.Where(request => request.RequesterUserId == userId);
            }

            var dashboard = new DashboardViewModel
            {
                DraftRequests = await requests.CountAsync(request => request.Status == PurchaseRequestStatus.Draft),
                SubmittedRequests = await requests.CountAsync(request => request.Status == PurchaseRequestStatus.Submitted),
                ApprovedRequests = await requests.CountAsync(request => request.Status == PurchaseRequestStatus.Approved),
                RejectedRequests = await requests.CountAsync(request => request.Status == PurchaseRequestStatus.Rejected),
                TotalEstimatedValue = await requests
                    .SelectMany(request => request.Items)
                    .SumAsync(item => (decimal?)(item.Quantity * item.EstimatedUnitPrice)) ?? 0m,
                RecentRequests = await requests
                    .Include(request => request.RequesterUser)
                    .Include(request => request.Items)
                    .OrderByDescending(request => request.CreatedAtUtc)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
