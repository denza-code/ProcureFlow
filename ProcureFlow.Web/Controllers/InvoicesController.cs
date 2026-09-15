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
public class InvoicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public InvoicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.PurchaseOrder)
                .ThenInclude(order => order!.Supplier)
            .OrderByDescending(invoice => invoice.ReceivedAtUtc)
            .ToListAsync();

        return View(invoices);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.PurchaseOrder)
                .ThenInclude(order => order!.Supplier)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);

        return invoice is null ? NotFound() : View(invoice);
    }

    public async Task<IActionResult> Create(int? orderId)
    {
        var model = new CreateInvoiceViewModel { PurchaseOrderId = orderId };

        if (orderId is not null)
        {
            var order = await _context.PurchaseOrders
                .AsNoTracking()
                .Include(purchaseOrder => purchaseOrder.Items)
                .FirstOrDefaultAsync(purchaseOrder => purchaseOrder.Id == orderId);

            if (order is not null)
            {
                model.Amount = order.Items.Sum(item => item.Total);
            }
        }

        await PopulatePurchaseOrdersAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInvoiceViewModel model)
    {
        if (model.DueDate.Date < model.InvoiceDate.Date)
        {
            ModelState.AddModelError(nameof(model.DueDate), "Due date cannot be earlier than the invoice date.");
        }

        if (!ModelState.IsValid)
        {
            await PopulatePurchaseOrdersAsync(model);
            return View(model);
        }

        var order = await _context.PurchaseOrders.FindAsync(model.PurchaseOrderId!.Value);
        if (order is null || order.Status != PurchaseOrderStatus.Received)
        {
            ModelState.AddModelError(nameof(model.PurchaseOrderId), "Select a valid received purchase order.");
            await PopulatePurchaseOrdersAsync(model);
            return View(model);
        }

        if (await _context.Invoices.AnyAsync(invoice => invoice.InvoiceNumber == model.InvoiceNumber))
        {
            ModelState.AddModelError(nameof(model.InvoiceNumber), "An invoice with this number already exists.");
            await PopulatePurchaseOrdersAsync(model);
            return View(model);
        }

        var invoice = new Invoice
        {
            PurchaseOrderId = order.Id,
            InvoiceNumber = model.InvoiceNumber,
            InvoiceDate = model.InvoiceDate.Date,
            DueDate = model.DueDate.Date,
            Amount = model.Amount,
            Status = InvoiceStatus.Received
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Invoice was recorded as received.";
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status != InvoiceStatus.Received)
        {
            TempData["ErrorMessage"] = "Only received invoices can be approved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        invoice.Status = InvoiceStatus.Approved;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Invoice was approved for payment.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status != InvoiceStatus.Approved)
        {
            TempData["ErrorMessage"] = "Only approved invoices can be marked as paid.";
            return RedirectToAction(nameof(Details), new { id });
        }

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Invoice was marked as paid.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulatePurchaseOrdersAsync(CreateInvoiceViewModel model)
    {
        var orders = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Supplier)
            .Where(order => order.Status == PurchaseOrderStatus.Received)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync();

        model.PurchaseOrders = orders.Select(order => new SelectListItem
        {
            Value = order.Id.ToString(),
            Text = order.OrderNumber + " — " + (order.Supplier?.Name ?? "Supplier"),
            Selected = order.Id == model.PurchaseOrderId
        });
    }
}
