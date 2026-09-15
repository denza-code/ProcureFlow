using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureFlow.Web.Data;
using ProcureFlow.Web.Models;
using ProcureFlow.Web.Security;

namespace ProcureFlow.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.Name)
            .ToListAsync();

        return View(suppliers);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(supplier => supplier.Id == id);

        return supplier is null ? NotFound() : View(supplier);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (!ModelState.IsValid)
        {
            return View(supplier);
        }

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Supplier was created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var supplier = await _context.Suppliers.FindAsync(id);
        return supplier is null ? NotFound() : View(supplier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier supplier)
    {
        if (id != supplier.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(supplier);
        }

        try
        {
            _context.Update(supplier);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await SupplierExistsAsync(supplier.Id))
            {
                return NotFound();
            }

            throw;
        }

        TempData["SuccessMessage"] = "Supplier was updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(supplier => supplier.Id == id);

        return supplier is null ? NotFound() : View(supplier);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is not null)
        {
            var isUsedInPurchaseOrder = await _context.PurchaseOrders
                .AnyAsync(order => order.SupplierId == id);

            if (isUsedInPurchaseOrder)
            {
                TempData["ErrorMessage"] = "This supplier cannot be deleted because it is used in a purchase order.";
                return RedirectToAction(nameof(Index));
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Supplier was deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    private Task<bool> SupplierExistsAsync(int id)
    {
        return _context.Suppliers.AnyAsync(supplier => supplier.Id == id);
    }
}
