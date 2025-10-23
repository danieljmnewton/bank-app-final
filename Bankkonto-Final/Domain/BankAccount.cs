using System.Text.Json.Serialization;

namespace Bankkonto_Final.Domain;

public class Bankkonto : IBankAccount
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public AccountType AccountType { get; private set; }
    public CurrencyType Currency { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime LastUpdated { get; private set; }

    public Bankkonto(string name, AccountType accountType, CurrencyType currency, decimal initialBalance)
    {
        Id = Guid.NewGuid();
        Name = name;
        AccountType = accountType;
        Currency = currency;
        Balance = initialBalance;
        LastUpdated = DateTime.Now;
    }

    [JsonConstructor]

    public Bankkonto(Guid id, string name, AccountType accountType, CurrencyType currency, decimal balance, DateTime lastUpdated)
    {
        Id = id == default ? Guid.NewGuid() : id;
        Name = name;
        AccountType = accountType;
        Currency = currency;
        Balance = balance;
        LastUpdated = lastUpdated == default ? DateTime.Now : lastUpdated;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
        if (amount > Balance) throw new InvalidOperationException("Insufficient funds.");
        Balance -= amount;
        LastUpdated = DateTime.Now;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
        Balance += amount;
        LastUpdated = DateTime.Now;
    }
}