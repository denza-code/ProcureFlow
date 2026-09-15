using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureFlow.Web.Data;
using ProcureFlow.Web.Models;
using ProcureFlow.Web.Security;

namespace ProcureFlow.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class CatalogItemsController : Controller
{
    private readonly ApplicationDbContext _context;

    public CatalogItemsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _context.CatalogItems
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var catalogItem = await _context.CatalogItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return catalogItem is null ? NotFound() : View(catalogItem);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CatalogItem catalogItem)
    {
        if (!ModelState.IsValid)
        {
            return View(catalogItem);
        }

        _context.CatalogItems.Add(catalogItem);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Catalog item was created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var catalogItem = await _context.CatalogItems.FindAsync(id);
        return catalogItem is null ? NotFound() : View(catalogItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CatalogItem catalogItem)
    {
        if (id != catalogItem.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(catalogItem);
        }

        try
        {
            _context.Update(catalogItem);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CatalogItemExistsAsync(catalogItem.Id))
            {
                return NotFound();
            }

            throw;
        }

        TempData["SuccessMessage"] = "Catalog item was updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var catalogItem = await _context.CatalogItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return catalogItem is null ? NotFound() : View(catalogItem);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var catalogItem = await _context.CatalogItems.FindAsync(id);
        if (catalogItem is not null)
        {
            var isUsedInPurchaseRequest = await _context.PurchaseRequestItems
                .AnyAsync(item => item.CatalogItemId == id);

            if (isUsedInPurchaseRequest)
            {
                TempData["ErrorMessage"] = "This catalog item cannot be deleted because it is used in a purchase request.";
                return RedirectToAction(nameof(Index));
            }

            _context.CatalogItems.Remove(catalogItem);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Catalog item was deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    private Task<bool> CatalogItemExistsAsync(int id)
    {
        return _context.CatalogItems.AnyAsync(item => item.Id == id);
    }
}
