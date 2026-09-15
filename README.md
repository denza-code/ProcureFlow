# ProcureFlow ERP

ProcureFlow ERP is a procurement management web application built with ASP.NET Core MVC. It models a complete purchasing workflow, from an employee's request to supplier invoice payment.

## Live Demo

[Open ProcureFlow ERP](https://procureflow-denis-demo-hgh4g5bgb0gpgng5.swedencentral-01.azurewebsites.net/)

## Features

- Supplier and catalog item management
- Purchase requests with multiple catalog items
- Role-based workflow with ASP.NET Core Identity:
  - Employee creates and submits purchase requests
  - Manager approves or rejects submitted requests
  - Admin manages suppliers, catalog items, purchase orders and invoices
- Purchase orders created from approved purchase requests
- Purchase order lifecycle: Draft, Sent, Received and Cancelled
- Invoice tracking for received purchase orders
- Invoice lifecycle: Received, Approved and Paid
- Dashboard with request counts, recent activity and estimated values
- Business rules that protect historical data from invalid deletion
- SQL Server LocalDB and Entity Framework Core migrations

## Workflow

```text
Purchase Request → Manager Approval → Purchase Order → Received → Invoice → Paid
```

## Technologies

- C# and .NET 8
- ASP.NET Core MVC
- Entity Framework Core
- Microsoft SQL Server LocalDB
- ASP.NET Core Identity and role-based authorization
- Bootstrap 5
- Razor Views
- LINQ

## Demo Accounts

| Role | Email | Password | Access |
| --- | --- | --- | --- |
| Admin | `admin@procureflow.local` | `AdminDemo!2026` | Full access to all modules |
| Manager | `manager@procureflow.local` | `ManagerDemo!2026` | Reviews and decides purchase requests |
| Employee | `employee@procureflow.local` | `EmployeeDemo!2026` | Creates and tracks personal purchase requests |

## Getting Started

1. Clone the repository.
2. Open `ProcureFlow.slnx` in Visual Studio.
3. Open Package Manager Console and select `ProcureFlow.Web` as the Default project.
4. Run:

   ```powershell
   Update-Database
   ```

5. Run the application. Demo roles and accounts are created automatically on startup.

## Project Structure

```text
ProcureFlow
├── ProcureFlow.Web
│   ├── Controllers
│   ├── Data
│   ├── Models
│   ├── Security
│   ├── ViewModels
│   ├── Views
│   └── Migrations
└── ProcureFlow.slnx
```

## Author

Denis Milinkovic
