using Microsoft.AspNetCore.Components;

namespace BankAccount_Final.Pages;

public partial class History : ComponentBase
{
    [Inject] private ITransactionService TransactionHistory { get; set; } = default!;
    [Inject] private IPinLockService PinLock { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    protected List<Transaction> _transactions = new();
    protected string? _errorMessage;
    protected int _pageSize = 10;
    protected int _currentPage = 1;

    protected string? _searchQuery;
    protected DateTime? _fromDate;
    protected DateTime? _toDate;
    protected TransactionType? _typeFilter;
    protected ExpenseCategory? _categoryFilter;

    protected string _sortBy = nameof(Transaction.Timestamp);
    protected bool _sortDesc = true;

    /// <summary>
    /// Current page of transactions after filtering and sorting.
    /// </summary>
    protected IEnumerable<Transaction> PagedTransactions =>
        FilteredSortedTransactions.Skip((_currentPage - 1) * _pageSize).Take(_pageSize);

    /// <summary>
    /// Total number of pages based on current page size and filters.
    /// </summary>
    protected int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)_pageSize));
    protected int FilteredCount => FilteredSortedTransactions.Count();

    /// <summary>
    /// Transactions after applying filters and sorting settings.
    /// </summary>
    protected IEnumerable<Transaction> FilteredSortedTransactions
    {
        get
        {
            IEnumerable<Transaction> query = _transactions;

            if (_fromDate is not null)
            {
                var from = _fromDate.Value.Date;
                query = query.Where(t => t.Timestamp.Date >= from);
            }
            if (_toDate is not null)
            {
                var to = _toDate.Value.Date;
                query = query.Where(t => t.Timestamp.Date <= to);
            }

            if (_typeFilter is not null)
            {
                var type = _typeFilter.Value;
                query = query.Where(t => t.Type == type);
            }

            if (_categoryFilter is not null)
            {
                var cat = _categoryFilter.Value;
                query = query.Where(t => t.Category == cat);
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                var s = _searchQuery.Trim().ToLowerInvariant();
                query = query.Where(t =>
                    GetDisplayAccountId(t).ToString().ToLowerInvariant().Contains(s) ||
                    t.Currency.ToString().ToLowerInvariant().Contains(s) ||
                    GetTransactionTypeLabel(t.Type).ToLowerInvariant().Contains(s) ||
                    (t.Note ?? string.Empty).ToLowerInvariant().Contains(s));
            }

            query = (_sortBy, _sortDesc) switch
            {
                (nameof(Transaction.Timestamp), true) => query.OrderByDescending(t => t.Timestamp),
                (nameof(Transaction.Timestamp), false) => query.OrderBy(t => t.Timestamp),
                (nameof(Transaction.Type), true) => query.OrderByDescending(t => t.Type).ThenByDescending(t => t.Timestamp),
                (nameof(Transaction.Type), false) => query.OrderBy(t => t.Type).ThenByDescending(t => t.Timestamp),
                ("AccountId", true) => query.OrderByDescending(t => GetDisplayAccountId(t)),
                ("AccountId", false) => query.OrderBy(t => GetDisplayAccountId(t)),
                (nameof(Transaction.Amount), true) => query.OrderByDescending(t => t.Amount),
                (nameof(Transaction.Amount), false) => query.OrderBy(t => t.Amount),
                (nameof(Transaction.Currency), true) => query.OrderByDescending(t => t.Currency),
                (nameof(Transaction.Currency), false) => query.OrderBy(t => t.Currency),
                (nameof(Transaction.BalanceAfter), true) => query.OrderByDescending(t => t.BalanceAfter),
                (nameof(Transaction.BalanceAfter), false) => query.OrderBy(t => t.BalanceAfter),
                _ => query.OrderByDescending(t => t.Timestamp)
            };

            return query;
        }
    }

    /// <summary>
    /// Navigates to a given page within valid range.
    /// </summary>
    protected void GoToPage(int page)
    {
        var total = TotalPages;
        if (page < 1) page = 1;
        if (page > total) page = total;
        _currentPage = page;
    }

    /// <summary>
    /// Selected page size; changing it resets to page 1.
    /// </summary>
    protected int PageSize
    {
        get => _pageSize;
        set { _pageSize = value; _currentPage = 1; }
    }

    /// <summary>
    /// Start date filter; changing it resets to page 1.
    /// </summary>
    protected DateTime? FromDate
    {
        get => _fromDate;
        set { _fromDate = value; _currentPage = 1; }
    }

    /// <summary>
    /// End date filter; changing it resets to page 1.
    /// </summary>
    protected DateTime? ToDate
    {
        get => _toDate;
        set { _toDate = value; _currentPage = 1; }
    }

    /// <summary>
    /// Type filter as string for binding.
    /// </summary>
    protected string TypeFilterString
    {
        get => _typeFilter?.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) _typeFilter = null;
            else if (Enum.TryParse<TransactionType>(value, out var parsed)) _typeFilter = parsed;
            _currentPage = 1;
        }
    }

    /// <summary>
    /// Free text search query; changing it resets to page 1.
    /// </summary>
    protected string? SearchQuery
    {
        get => _searchQuery;
        set { _searchQuery = value; _currentPage = 1; }
    }

    /// <summary>
    /// Handles search input changes.
    /// </summary>
    protected void OnSearchChanged(ChangeEventArgs e) => SearchQuery = e?.Value?.ToString();
    /// <summary>
    /// Handles type filter changes.
    /// </summary>
    protected void OnTypeFilterChanged(ChangeEventArgs e) => TypeFilterString = e?.Value?.ToString() ?? string.Empty;
    /// <summary>
    /// Handles category filter changes.
    /// </summary>
    protected void OnCategoryFilterChanged(ChangeEventArgs e) => CategoryFilterString = e?.Value?.ToString() ?? string.Empty;
    /// <summary>
    /// Handles page size selection changes.
    /// </summary>
    protected void OnPageSizeChanged(ChangeEventArgs e)
    {
        if (e?.Value is not null && int.TryParse(e.Value.ToString(), out var size) && size > 0)
        {
            PageSize = size;
        }
    }

    /// <summary>
    /// Clears all filters and resets pagination.
    /// </summary>
    protected void ClearFilters()
    {
        _fromDate = null;
        _toDate = null;
        _typeFilter = null;
        _categoryFilter = null;
        _searchQuery = null;
        _currentPage = 1;
    }

    /// <summary>
    /// Toggles sorting on a column.
    /// </summary>
    protected void ToggleSort(string column)
    {
        if (string.Equals(_sortBy, column, StringComparison.Ordinal))
        {
            _sortDesc = !_sortDesc;
        }
        else
        {
            _sortBy = column;
            _sortDesc = column == nameof(Transaction.Timestamp);
        }
        _currentPage = 1;
    }

    /// <summary>
    /// Returns a visual sort indicator for a column.
    /// </summary>
    protected string SortIndicator(string column)
        => _sortBy == column ? (_sortDesc ? "▼" : "▲") : string.Empty;

    /// <summary>
    /// Loads transactions on initialization and enforces PIN lock.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await PinLock.InitializeAsync();
        if (!PinLock.IsUnlocked)
        {
            Nav.NavigateTo("/", true);
            return;
        }

        try
        {
            var items = await TransactionHistory.GetAllAsync();
            _transactions = items.ToList();
            _currentPage = 1;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load transactions: {ex.Message}";
            Console.Error.WriteLine(ex);
            _transactions = new List<Transaction>();
        }
    }

    /// <summary>
    /// Human-readable label for an account type.
    /// </summary>
    protected static string GetAccountTypeLabel(AccountType type) => type switch
    {
        AccountType.Deposit => "Basic account",
        AccountType.Savings => "Savings account",
        _ => "Unknown"
    };

    /// <summary>
    /// Human-readable label for a transaction type.
    /// </summary>
    protected static string GetTransactionTypeLabel(TransactionType type) => type switch
    {
        TransactionType.Deposit => "Deposit",
        TransactionType.Withdrawal => "Withdrawal",
        TransactionType.Transfer => "Transfer",
        _ => "Unknown"
    };

    /// <summary>
    /// Human-readable label for an expense category.
    /// </summary>
    protected static string GetCategoryLabel(ExpenseCategory category) => category switch
    {
        ExpenseCategory.None => "No category",
        ExpenseCategory.Food => "Food",
        ExpenseCategory.Rent => "Rent",
        ExpenseCategory.Transport => "Transport",
        _ => category.ToString()
    };

    /// <summary>
    /// Chooses which account ID to display for a transaction.
    /// </summary>
    protected static Guid GetDisplayAccountId(Transaction t) => t.Type switch
    {
        TransactionType.Deposit => t.ToAccountId ?? Guid.Empty,
        TransactionType.Withdrawal => t.FromAccountId ?? Guid.Empty,
        TransactionType.Transfer => (t.Amount < 0 ? t.FromAccountId : t.ToAccountId) ?? Guid.Empty,
        _ => t.ToAccountId ?? t.FromAccountId ?? Guid.Empty
    };

    /// <summary>
    /// Shortens a GUID to six characters for display.
    /// </summary>
    protected static string ShortId(Guid id)
    {
        var s = id.ToString("N");
        return s.Length >= 6 ? s.Substring(0, 6) : s;
    }

    /// <summary>
    /// Category filter as string for binding.
    /// </summary>
    protected string CategoryFilterString
    {
        get => _categoryFilter?.ToString() ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) _categoryFilter = null;
            else if (Enum.TryParse<ExpenseCategory>(value, out var parsed)) _categoryFilter = parsed;
            _currentPage = 1;
        }
    }
}
