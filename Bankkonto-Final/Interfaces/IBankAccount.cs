public interface IBankAccount
{
    Guid Id { get; }
    string Name { get; }
    AccountType AccountType { get; }
    CurrencyType Currency { get; }
    decimal Balance { get; }
    DateTime LastUpdated { get; }

    void Withdraw(decimal amount);
    void Deposit(decimal amount);
}
