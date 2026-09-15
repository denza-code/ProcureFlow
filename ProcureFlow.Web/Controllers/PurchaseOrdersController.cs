using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProcureFlow.Web.Data;
using ProcureFlow.Web.Models;
using ProcureFlow.Web.Models.Enums;
using ProcureFlow.Web.Security;
using ProcureFlow.Web.ViewModels;

namespace ProcureFlow.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class PurchaseOrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public PurchaseOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Supplier)
            .Include(order => order.PurchaseRequest)
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var order = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Supplier)
            .Include(order => order.PurchaseRequest)
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id);

        return order is null ? NotFound() : View(order);
    }

    public async Task<IActionResult> Create(int? requestId)
    {
        return View(await CreateViewModelAsync(requestId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePurchaseOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(model);
            return View(model);
        }

        var request = await _context.PurchaseRequests
            .Include(request => request.Items)
                .ThenInclude(item => item.CatalogItem)
            .FirstOrDefaultAsync(request => request.Id == model.PurchaseRequestId);

        if (request is null || request.Status != PurchaseRequestStatus.Approved)
        {
            ModelState.AddModelError(nameof(model.PurchaseRequestId), "Select a valid approved purchase request.");
            await PopulateSelectListsAsync(model);
            return View(model);
        }

        if (await _context.PurchaseOrders.AnyAsync(order => order.PurchaseRequestId == request.Id))
        {
            ModelState.AddModelError(nameof(model.PurchaseRequestId), "A purchase order already exists for this purchase request.");
            await PopulateSelectListsAsync(model);
            return View(model);
        }

        var supplier = await _context.Suppliers.FindAsync(model.SupplierId!.Value);
        if (supplier is null)
        {
            ModelState.AddModelError(nameof(model.SupplierId), "Select a valid supplier.");
            await PopulateSelectListsAsync(model);
            return View(model);
        }

        if (!request.Items.Any())
        {
            ModelState.AddModelError(nameof(model.PurchaseRequestId), "The selected purchase request has no items.");
            await PopulateSelectListsAsync(model);
            return View(model);
        }

        var order = new PurchaseOrder
        {
            PurchaseRequestId = request.Id,
            SupplierId = supplier.Id,
            Status = PurchaseOrderStatus.Draft,
            Items = request.Items.Select(item => new PurchaseOrderItem
            {
                ItemName = item.CatalogItem?.Name ?? "Catalog item",
                UnitOfMeasure = item.CatalogItem?.UnitOfMeasure ?? "pcs",
                Quantity = item.Quantity,
                UnitPrice = item.EstimatedUnitPrice
            }).ToList()
        };

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();

        order.OrderNumber = $"PO-{order.CreatedAtUtc:yyyy}-{order.Id:D5}";
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Purchase order was created from the approved request.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id)
    {
        var order = await _context.PurchaseOrders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status != PurchaseOrderStatus.Draft)
        {
            TempData["ErrorMessage"] = "Only draft purchase orders can be marked as sent.";
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = PurchaseOrderStatus.Sent;
        order.SentAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Purchase order was marked as sent.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(int id)
    {
        var order = await _context.PurchaseOrders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status != PurchaseOrderStatus.Sent)
        {
            TempData["ErrorMessage"] = "Only sent purchase orders can be marked as received.";
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = PurchaseOrderStatus.Received;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Purchase order was marked as received.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _context.PurchaseOrders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled)
        {
            TempData["ErrorMessage"] = "Received or cancelled purchase orders cannot be cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = PurchaseOrderStatus.Cancelled;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Purchase order was cancelled.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<CreatePurchaseOrderViewModel> CreateViewModelAsync(int? requestId)
    {
        var model = new CreatePurchaseOrderViewModel { PurchaseRequestId = requestId };
        await PopulateSelectListsAsync(model);
        return model;
    }

    private async Task PopulateSelectListsAsync(CreatePurchaseOrderViewModel model)
    {
        var eligibleRequests = await _context.PurchaseRequests
            .AsNoTracking()
            .Where(request => request.Status == PurchaseRequestStatus.Approved)
            .Where(request => !_context.PurchaseOrders.Any(order => order.PurchaseRequestId == request.Id))
            .OrderByDescending(request => request.CreatedAtUtc)
            .ToListAsync();

        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.Name)
            .ToListAsync();

        model.PurchaseRequests = eligibleRequests.Select(request => new SelectListItem
        {
            Value = request.Id.ToString(),
            Text = request.RequestNumber + " — " + request.Title,
            Selected = request.Id == model.PurchaseRequestId
        });

        model.Suppliers = suppliers.Select(supplier => new SelectListItem
        {
            Value = supplier.Id.ToString(),
            Text = supplier.Name,
            Selected = supplier.Id == model.SupplierId
        });
    }
}
