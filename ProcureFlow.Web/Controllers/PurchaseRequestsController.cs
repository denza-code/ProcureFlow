using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProcureFlow.Web.Data;
using ProcureFlow.Web.Models;
using ProcureFlow.Web.Models.Enums;
using ProcureFlow.Web.Security;
using ProcureFlow.Web.ViewModels;

namespace ProcureFlow.Web.Controllers;

[Authorize]
public class PurchaseRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PurchaseRequestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var requests = _context.PurchaseRequests
            .AsNoTracking()
            .Include(request => request.RequesterUser)
            .Include(request => request.Items)
            .OrderByDescending(request => request.CreatedAtUtc)
            .AsQueryable();

        if (!IsManagerOrAdmin())
        {
            var userId = _userManager.GetUserId(User);
            requests = requests.Where(request => request.RequesterUserId == userId);
        }

        return View(await requests.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var request = await GetRequestWithDetailsAsync(id.Value);
        if (request is null)
        {
            return NotFound();
        }

        if (!CanViewRequest(request))
        {
            return Forbid();
        }

        return View(request);
    }

    [Authorize(Roles = AppRoles.Employee + "," + AppRoles.Admin)]
    public IActionResult Create()
    {
        return View(new CreatePurchaseRequestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Employee + "," + AppRoles.Admin)]
    public async Task<IActionResult> Create(CreatePurchaseRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new PurchaseRequest
        {
            Title = model.Title,
            BusinessJustification = model.BusinessJustification,
            RequesterUserId = _userManager.GetUserId(User)!,
            Status = PurchaseRequestStatus.Draft
        };

        _context.PurchaseRequests.Add(request);
        await _context.SaveChangesAsync();

        request.RequestNumber = $"PR-{request.CreatedAtUtc:yyyy}-{request.Id:D5}";
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Draft purchase request was created. Add at least one item before submitting it.";
        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    [Authorize(Roles = AppRoles.Employee + "," + AppRoles.Admin)]
    public async Task<IActionResult> AddItem(int? requestId)
    {
        if (requestId is null)
        {
            return NotFound();
        }

        var request = await _context.PurchaseRequests.AsNoTracking().FirstOrDefaultAsync(item => item.Id == requestId);
        if (request is null)
        {
            return NotFound();
        }

        if (!CanManageDraft(request))
        {
            return Forbid();
        }

        return View(await CreateAddItemViewModelAsync(request.Id));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Employee + "," + AppRoles.Admin)]
    public async Task<IActionResult> AddItem(AddPurchaseRequestItemViewModel model)
    {
        var request = await _context.PurchaseRequests.FindAsync(model.PurchaseRequestId);
        if (request is null)
        {
            return NotFound();
        }

        if (!CanManageDraft(request))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.CatalogItems = await GetCatalogItemsAsync();
            return View(model);
        }

        var catalogItem = await _context.CatalogItems.FindAsync(model.CatalogItemId!.Value);
        if (catalogItem is null)
        {
            ModelState.AddModelError(nameof(model.CatalogItemId), "Select a valid catalog item.");
            model.CatalogItems = await GetCatalogItemsAsync();
            return View(model);
        }

        var itemAlreadyAdded = await _context.PurchaseRequestItems.AnyAsync(item =>
            item.PurchaseRequestId == request.Id && item.CatalogItemId == catalogItem.Id);

        if (itemAlreadyAdded)
        {
            ModelState.AddModelError(nameof(model.CatalogItemId), "This catalog item is already part of the request.");
            model.CatalogItems = await GetCatalogItemsAsync();
            return View(model);
        }

        _context.PurchaseRequestItems.Add(new PurchaseRequestItem
        {
            PurchaseRequestId = request.Id,
            CatalogItemId = catalogItem.Id,
            Quantity = model.Quantity,
            EstimatedUnitPrice = catalogItem.EstimatedUnitPrice
        });
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Item was added to the purchase request.";
        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Employee + "," + AppRoles.Admin)]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var item = await _context.PurchaseRequestItems
            .Include(requestItem => requestItem.PurchaseRequest)
            .FirstOrDefaultAsync(requestItem => requestItem.Id == id);

        if (item?.PurchaseRequest is null)
        {
            return NotFound();
        }

        if (!CanManageDraft(item.PurchaseRequest))
        {
            return Forbid();
        }

        var requestId = item.PurchaseRequestId;
        _context.PurchaseRequestItems.Remove(item);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Item was removed from the purchase request.";
        return RedirectToAction(nameof(Details), new { id = requestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Employee + "," + AppRoles.Admin)]
    public async Task<IActionResult> Submit(int id)
    {
        var request = await _context.PurchaseRequests
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (request is null)
        {
            return NotFound();
        }

        if (!CanManageDraft(request))
        {
            return Forbid();
        }

        if (!request.Items.Any())
        {
            TempData["ErrorMessage"] = "Add at least one item before submitting a purchase request.";
            return RedirectToAction(nameof(Details), new { id });
        }

        request.Status = PurchaseRequestStatus.Submitted;
        request.SubmittedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Purchase request was submitted for approval.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Manager + "," + AppRoles.Admin)]
    public async Task<IActionResult> Approve(int id)
    {
        var request = await _context.PurchaseRequests.FindAsync(id);
        if (request is null)
        {
            return NotFound();
        }

        if (request.Status != PurchaseRequestStatus.Submitted)
        {
            TempData["ErrorMessage"] = "Only submitted purchase requests can be approved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        request.Status = PurchaseRequestStatus.Approved;
        request.DecidedAtUtc = DateTime.UtcNow;
        request.DecisionByUserId = _userManager.GetUserId(User);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Purchase request was approved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = AppRoles.Manager + "," + AppRoles.Admin)]
    public async Task<IActionResult> Reject(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var request = await _context.PurchaseRequests.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (request is null)
        {
            return NotFound();
        }

        if (request.Status != PurchaseRequestStatus.Submitted)
        {
            TempData["ErrorMessage"] = "Only submitted purchase requests can be rejected.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(new RejectPurchaseRequestViewModel
        {
            PurchaseRequestId = request.Id,
            RequestNumber = request.RequestNumber,
            Title = request.Title
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Manager + "," + AppRoles.Admin)]
    public async Task<IActionResult> Reject(RejectPurchaseRequestViewModel model)
    {
        var request = await _context.PurchaseRequests.FindAsync(model.PurchaseRequestId);
        if (request is null)
        {
            return NotFound();
        }

        if (request.Status != PurchaseRequestStatus.Submitted)
        {
            TempData["ErrorMessage"] = "Only submitted purchase requests can be rejected.";
            return RedirectToAction(nameof(Details), new { id = request.Id });
        }

        if (!ModelState.IsValid)
        {
            model.RequestNumber = request.RequestNumber;
            model.Title = request.Title;
            return View(model);
        }

        request.Status = PurchaseRequestStatus.Rejected;
        request.DecisionNote = model.DecisionNote;
        request.DecidedAtUtc = DateTime.UtcNow;
        request.DecisionByUserId = _userManager.GetUserId(User);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Purchase request was rejected.";
        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    private async Task<PurchaseRequest?> GetRequestWithDetailsAsync(int id)
    {
        return await _context.PurchaseRequests
            .AsNoTracking()
            .Include(request => request.RequesterUser)
            .Include(request => request.DecisionByUser)
            .Include(request => request.Items)
                .ThenInclude(item => item.CatalogItem)
            .FirstOrDefaultAsync(request => request.Id == id);
    }

    private bool CanViewRequest(PurchaseRequest request)
    {
        return IsManagerOrAdmin() || request.RequesterUserId == _userManager.GetUserId(User);
    }

    private bool CanManageDraft(PurchaseRequest request)
    {
        return request.Status == PurchaseRequestStatus.Draft &&
               (User.IsInRole(AppRoles.Admin) || request.RequesterUserId == _userManager.GetUserId(User));
    }

    private bool IsManagerOrAdmin()
    {
        return User.IsInRole(AppRoles.Manager) || User.IsInRole(AppRoles.Admin);
    }

    private async Task<AddPurchaseRequestItemViewModel> CreateAddItemViewModelAsync(int requestId)
    {
        return new AddPurchaseRequestItemViewModel
        {
            PurchaseRequestId = requestId,
            CatalogItems = await GetCatalogItemsAsync()
        };
    }

    private async Task<IEnumerable<SelectListItem>> GetCatalogItemsAsync()
    {
        var catalogItems = await _context.CatalogItems
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync();

        return catalogItems.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Name + " — " + item.EstimatedUnitPrice.ToString("N2") + " €"
            });
    }
}
