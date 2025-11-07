using System.Text.Json.Serialization;

namespace BankAccount_Final.Domain;

public class BankAccount : IBankAccount
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public AccountType AccountType { get; private set; }
    public CurrencyType Currency { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime LastUpdated { get; private set; }

    /// <summary>
    /// Creates a new bank account instance.
    /// </summary>
    /// <param name="name">Account name.</param>
    /// <param name="accountType">Type of account.</param>
    /// <param name="currency">Account currency.</param>
    /// <param name="initialBalance">Initial balance.</param>
    public BankAccount(string name, AccountType accountType, CurrencyType currency, decimal initialBalance)
    {
        Id = Guid.NewGuid();
        Name = name;
        AccountType = accountType;
        Currency = currency;
        Balance = initialBalance;
        LastUpdated = DateTime.Now;
    }

    [JsonConstructor]
    /// <summary>
    /// Reconstructs an account from serialized data.
    /// </summary>
    public BankAccount(Guid id, string name, AccountType accountType, CurrencyType currency, decimal balance, DateTime lastUpdated)
    {
        Id = id == default ? Guid.NewGuid() : id;
        Name = name;
        AccountType = accountType;
        Currency = currency;
        Balance = balance;
        LastUpdated = lastUpdated == default ? DateTime.Now : lastUpdated;
    }

    /// <summary>
    /// Withdraws the specified amount from the account.
    /// </summary>
    /// <param name="amount">Amount to withdraw (must be positive and not exceed balance).</param>
    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
        if (amount > Balance) throw new InvalidOperationException("Insufficient funds.");
        Balance -= amount;
        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Deposits the specified amount into the account.
    /// </summary>
    /// <param name="amount">Amount to deposit (must be positive).</param>
    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
        Balance += amount;
        LastUpdated = DateTime.Now;
    }
}
