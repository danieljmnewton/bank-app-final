namespace Bankkonto_Final.Domain;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? FromAccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public CurrencyType Currency { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public TransactionType Type { get; set; }
    public string? Note { get; set; }
    public ExpenseCategory Category { get; set; } = ExpenseCategory.None;

    public static async Task<Transaction> DepositAsync(
        IAccountService accountService,
        ITransactionService transactionService,
        IBankAccount account,
        decimal amount)
    {
        await accountService.DepositAsync(account.Id, amount);

        var record = new Transaction
        {
            ToAccountId = account.Id,
            AccountName = account.Name,
            AccountType = account.AccountType,
            Currency = account.Currency,
            Amount = amount,
            BalanceAfter = account.Balance,
            Timestamp = DateTime.Now,
            Type = TransactionType.Deposit
        };

        await transactionService.AddAsync(record);
        return record;
    }

    public static async Task<Transaction> WithdrawAsync(
        IAccountService accountService,
        ITransactionService transactionService,
        IBankAccount account,
        decimal amount,
        ExpenseCategory category = ExpenseCategory.None)
    {
        await accountService.WithdrawAsync(account.Id, amount);

        var record = new Transaction
        {
            FromAccountId = account.Id,
            AccountName = account.Name,
            AccountType = account.AccountType,
            Currency = account.Currency,
            Amount = amount,
            BalanceAfter = account.Balance,
            Timestamp = DateTime.Now,
            Type = TransactionType.Withdrawal,
            Category = category
        };

        await transactionService.AddAsync(record);
        return record;
    }

    public static async Task<(Transaction debit, Transaction credit)> TransferAsync(
        IAccountService accountService,
        ITransactionService transactionService,
        IBankAccount from,
        IBankAccount to,
        decimal amount,
        string? note = null,
        ExpenseCategory category = ExpenseCategory.None)
    {
        await accountService.TransferAsync(from.Id, to.Id, amount);

        var debit = new Transaction
        {
            FromAccountId = from.Id,
            ToAccountId = to.Id,
            AccountName = from.Name,
            AccountType = from.AccountType,
            Currency = from.Currency,
            Amount = -amount,
            BalanceAfter = from.Balance,
            Timestamp = DateTime.Now,
            Type = TransactionType.Transfer,
            Note = note,
            Category = category
        };

        var credit = new Transaction
        {
            FromAccountId = from.Id,
            ToAccountId = to.Id,
            AccountName = to.Name,
            AccountType = to.AccountType,
            Currency = to.Currency,
            Amount = amount,
            BalanceAfter = to.Balance,
            Timestamp = DateTime.Now,
            Type = TransactionType.Transfer,
            Note = note,
            Category = category
        };

        await transactionService.AddAsync(debit);
        await transactionService.AddAsync(credit);
        return (debit, credit);
    }
}
