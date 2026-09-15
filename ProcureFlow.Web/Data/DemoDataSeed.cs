using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcureFlow.Web.Models;
using ProcureFlow.Web.Models.Enums;

namespace ProcureFlow.Web.Data;

public static class DemoDataSeed
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        if (await context.Suppliers.AnyAsync() || await context.CatalogItems.AnyAsync())
        {
            return;
        }

        var employee = await userManager.FindByEmailAsync("employee@procureflow.local")
            ?? throw new InvalidOperationException("Employee demo account was not found.");
        var manager = await userManager.FindByEmailAsync("manager@procureflow.local")
            ?? throw new InvalidOperationException("Manager demo account was not found.");

        var techNova = new Supplier
        {
            Name = "TechNova Solutions d.o.o.",
            VatNumber = "12345678901",
            ContactPerson = "Marko Kovač",
            Email = "marko@technova.example",
            Phone = "+385 1 555 1001"
        };
        var officePoint = new Supplier
        {
            Name = "OfficePoint d.o.o.",
            VatNumber = "23456789012",
            ContactPerson = "Petra Marić",
            Email = "petra@officepoint.example",
            Phone = "+385 1 555 1002"
        };
        var cleanPro = new Supplier
        {
            Name = "CleanPro d.o.o.",
            VatNumber = "34567890123",
            ContactPerson = "Ivan Horvat",
            Email = "ivan@cleanpro.example",
            Phone = "+385 1 555 1003"
        };

        var laptop = new CatalogItem { Name = "Dell Latitude 5550", ItemCode = "IT-LAP-001", Description = "Business laptop for a new employee.", EstimatedUnitPrice = 1250m, UnitOfMeasure = "pcs" };
        var monitor = new CatalogItem { Name = "Dell 27-inch Monitor", ItemCode = "IT-MON-001", Description = "External monitor for office work.", EstimatedUnitPrice = 280m, UnitOfMeasure = "pcs" };
        var chair = new CatalogItem { Name = "Ergonomic Office Chair", ItemCode = "OFF-CHAIR-001", Description = "Adjustable ergonomic office chair.", EstimatedUnitPrice = 230m, UnitOfMeasure = "pcs" };
        var paper = new CatalogItem { Name = "A4 Copy Paper Box", ItemCode = "OFF-PAPER-001", Description = "Box of A4 copy paper.", EstimatedUnitPrice = 32m, UnitOfMeasure = "box" };
        var toner = new CatalogItem { Name = "Printer Toner", ItemCode = "OFF-TONER-001", Description = "Black toner cartridge for office printer.", EstimatedUnitPrice = 78m, UnitOfMeasure = "pcs" };

        context.AddRange(techNova, officePoint, cleanPro, laptop, monitor, chair, paper, toner);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var approvedRequest = new PurchaseRequest
        {
            RequestNumber = "PR-2026-00001",
            Title = "IT equipment for a new employee",
            BusinessJustification = "Equipment is required to prepare a workstation for a new employee joining the team.",
            RequesterUserId = employee.Id,
            Status = PurchaseRequestStatus.Approved,
            CreatedAtUtc = now.AddDays(-14),
            SubmittedAtUtc = now.AddDays(-13),
            DecidedAtUtc = now.AddDays(-12),
            DecisionByUserId = manager.Id,
            DecisionNote = "Approved for onboarding requirements.",
            Items = new List<PurchaseRequestItem>
            {
                new() { CatalogItemId = laptop.Id, Quantity = 1, EstimatedUnitPrice = laptop.EstimatedUnitPrice },
                new() { CatalogItemId = monitor.Id, Quantity = 2, EstimatedUnitPrice = monitor.EstimatedUnitPrice }
            }
        };
        var submittedRequest = new PurchaseRequest
        {
            RequestNumber = "PR-2026-00002",
            Title = "Office supplies replenishment",
            BusinessJustification = "Office supplies need to be replenished for regular day-to-day operations.",
            RequesterUserId = employee.Id,
            Status = PurchaseRequestStatus.Submitted,
            CreatedAtUtc = now.AddDays(-6),
            SubmittedAtUtc = now.AddDays(-5),
            Items = new List<PurchaseRequestItem>
            {
                new() { CatalogItemId = paper.Id, Quantity = 10, EstimatedUnitPrice = paper.EstimatedUnitPrice },
                new() { CatalogItemId = toner.Id, Quantity = 3, EstimatedUnitPrice = toner.EstimatedUnitPrice }
            }
        };
        var rejectedRequest = new PurchaseRequest
        {
            RequestNumber = "PR-2026-00003",
            Title = "Replacement ergonomic chairs",
            BusinessJustification = "Several chairs require replacement to improve staff ergonomics.",
            RequesterUserId = employee.Id,
            Status = PurchaseRequestStatus.Rejected,
            CreatedAtUtc = now.AddDays(-9),
            SubmittedAtUtc = now.AddDays(-8),
            DecidedAtUtc = now.AddDays(-7),
            DecisionByUserId = manager.Id,
            DecisionNote = "The requested replacement is not included in the current office budget.",
            Items = new List<PurchaseRequestItem>
            {
                new() { CatalogItemId = chair.Id, Quantity = 4, EstimatedUnitPrice = chair.EstimatedUnitPrice }
            }
        };
        var draftRequest = new PurchaseRequest
        {
            RequestNumber = "PR-2026-00004",
            Title = "Additional monitor for meeting room",
            BusinessJustification = "An additional monitor is needed to improve hybrid meeting presentations.",
            RequesterUserId = employee.Id,
            Status = PurchaseRequestStatus.Draft,
            CreatedAtUtc = now.AddDays(-2),
            Items = new List<PurchaseRequestItem>
            {
                new() { CatalogItemId = monitor.Id, Quantity = 1, EstimatedUnitPrice = monitor.EstimatedUnitPrice }
            }
        };

        context.PurchaseRequests.AddRange(approvedRequest, submittedRequest, rejectedRequest, draftRequest);
        await context.SaveChangesAsync();

        var order = new PurchaseOrder
        {
            OrderNumber = "PO-2026-00001",
            PurchaseRequestId = approvedRequest.Id,
            SupplierId = techNova.Id,
            Status = PurchaseOrderStatus.Received,
            CreatedAtUtc = now.AddDays(-11),
            SentAtUtc = now.AddDays(-10),
            Items = new List<PurchaseOrderItem>
            {
                new() { ItemName = laptop.Name, UnitOfMeasure = laptop.UnitOfMeasure, Quantity = 1, UnitPrice = laptop.EstimatedUnitPrice },
                new() { ItemName = monitor.Name, UnitOfMeasure = monitor.UnitOfMeasure, Quantity = 2, UnitPrice = monitor.EstimatedUnitPrice }
            }
        };

        context.PurchaseOrders.Add(order);
        await context.SaveChangesAsync();

        context.Invoices.Add(new Invoice
        {
            InvoiceNumber = "TN-2026-001",
            PurchaseOrderId = order.Id,
            InvoiceDate = now.AddDays(-9).Date,
            DueDate = now.AddDays(21).Date,
            Amount = 1810m,
            Status = InvoiceStatus.Paid,
            ReceivedAtUtc = now.AddDays(-8),
            PaidAtUtc = now.AddDays(-1)
        });

        await context.SaveChangesAsync();
    }
}
