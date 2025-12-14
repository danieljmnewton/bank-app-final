## Bank App (Blazor WebAssembly)

Simple personal banking demo built with Blazor WebAssembly. Create accounts, deposit/withdraw, transfer between your own accounts, and browse a searchable/sortable history. Data is stored locally in the browser and can be exported/imported as JSON.

- Default PIN: 9867
- Language: UI in English, code comments in English

---

## Quick Start

- Prerequisites: .NET 8 SDK
- Clone and run:
  - `git clone https://github.com/danieljmnewton/Bankkonto-FInal.git`
  - `cd Bankkonto-Final`
  - `dotnet run`
- Open the URL that `dotnet run` prints.
- Log in with the 4‑digit PIN.

---

## How To Use

- Enter the PIN on the start page to unlock.
- Create accounts on `Accounts` (Savings or Basic account, currency SEK).
- Make deposits, withdrawals, and transfers between your accounts on `Transactions`.
- Each operation writes to `History`. Use filters (date range, type, category) and search to find entries.
- Go to `Backup` to copy all account data as JSON, or paste JSON to import (merge mode keeps existing data).

Note: Data persists in your browser only. Clearing site data or using a different browser/device will reset it.

---

## Features

- Accounts: create Savings or Basic (Deposit) accounts
- Transactions: deposit, withdraw, transfer between own accounts
- Expense categories: Food, Rent, Transport (+ None) for withdrawals and transfers
- History: filter by date/type/category, full‑text search, sorting and paging
- Backup: export/import accounts as JSON, with input validation
- Persistence: stored in `localStorage` (no server required)
- UI lock: simple PIN lock to hide the app while demoing

---

## Features (VG): Why and How

### 1) Budget/expense categories

- Why
  - Provides a quick overview of where the money goes (e.g., Food, Rent, Transport).
  - Enables filtering and simpler reporting in history without extra data sources.
  - An enum keeps categories consistent, easy to extend, and stable in JSON.

- How
  - Data model: `ExpenseCategory` is defined in `Domain/Types.cs` (None, Food, Rent, Transport).
  - Transactions carry the category via `Transaction.Category` (see `Domain/Transaction.cs`), set for withdrawals and transfers.
  - UI: The category is selected in `Pages/Payments.razor` via `<InputSelect>`; a simple list of available categories is shown.
  - History: Filtering by category in `Components/History.cs` to break out costs by type.

### 2) Export/Import (JSON) with validation

- Why
  - Manual backup/restore without a server; easy to share, inspect, and debug.
  - Human-readable JSON makes testing and versioning easier.

- How
  - Export: `AccountService.ExportJsonAsync()` serializes accounts with `System.Text.Json` and `JsonStringEnumConverter` (enums as strings, indented JSON).
  - Import: `AccountService.ImportJsonAsync(json, replaceExisting)` validates empty string, invalid JSON, and empty list; returns error messages. On merge it de-duplicates by `Id`; on replace it overwrites everything.
  - UI: The `Pages/Backup.razor` page copies the export to the clipboard and imports from a textarea.
  - Storage: Underlying persistence uses the browser’s `localStorage` via `Services/StorageService.cs` (JSON in/out).

- Usage
  - Go to "Backup". Choose "Copy JSON" to export. Paste JSON and click "Import (merge)" to import data (without overwriting everything).

### 3) Access protection: simple PIN (UI lock)

- Why
  - Quick UI lock for demo/school projects without a backend — deters casual viewing but not an attacker.
  - Minimal friction, easy to understand and test.

- How
  - Service: `PinLockService` exposes `IsUnlocked` and saves the lock state in `localStorage`.
  - Unlocking: Hard-coded PIN "9867" in `Services/PinLockService.cs` (change if needed).
  - Flow: The start page `Pages/Pin.razor` asks for the PIN and navigates on success.
  - Guards: Pages like `CreateAccount`, `Payments`, and `History` protect themselves and redirect to the PIN page if locked. The menu (`Layout/NavMenu.razor`) shows protected pages only when unlocked.
  - Registration: Services are registered for DI in `Program.cs`.

- Limitations
  - No real authentication or encryption; the PIN is hard-coded and the status is stored in clear text in `localStorage`. This is intentional, given the requirement for a "simple UI lock".

## Architecture at a Glance

- Layers and interfaces
  - Domain models: `BankAccount`, `Transaction`, enum types in `Domain/*`.
  - Services: `IAccountService`, `ITransactionService`, `IStorageService`, `IPinLockService` with implementations in `Services/*`.
  - UI: Razor pages/components in `Pages/*`, `Components/*`, `Layout/*` that communicate only through service interfaces.
  - DI: Registered in `Program.cs` for easy testability and replaceability.

## Technical Choices and Trade‑offs

- Blazor WASM without backend
  - + Simple deployment, low complexity, runs entirely in the browser.
  - – All logic and data reside on the client (limited security). Suits demos/school projects.

- `localStorage` for persistence
  - + Sufficient for small datasets, no external dependencies, simple API via `IJSRuntime`.
  - – Clear text, quota (~5–10 MB), no transactions/key indexing. For growing data: consider IndexedDB or a server API.

- JSON serialization with `System.Text.Json`
  - + Built into .NET, fast and lightweight. `JsonStringEnumConverter` + camelCase provides stable, readable JSON across languages.
  - `WriteIndented = true` prioritizes readability over minimal size (reasonable for small data and simple debugging).

- Import merge based on `Id` (GUID)
  - + Prevents duplicates unambiguously. Name/type matching is avoided to prevent collisions.
  - The "replace" option simplifies full restore without special cases.

- Enum for expense categories
  - + Compile-time safety, consistent UI/JSON, easy to extend in code.
  - – Less flexible than free text. If user-defined tags are needed, introduce a separate tag model.

- UI lock (PIN) instead of real auth
  - + Minimal dependencies, quick to understand/use, sufficient to hide content.
  - – Not real security. For production: use real authentication (e.g., OIDC) and server-side storage.

- Error handling and logging
  - Services use guard checks and return errors (e.g., a list on import). Console output is sufficient for debugging in a demo.

## Future Improvements

- Real authentication/authorization and server-side data storage.
- Replace `localStorage` with IndexedDB for larger datasets and better robustness.
- User-defined categories/tags with simple searchability.
- Export/import both accounts and transaction history, plus checksums for integrity.
