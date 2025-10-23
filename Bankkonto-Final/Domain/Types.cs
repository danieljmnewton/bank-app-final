namespace Bankkonto_Final.Domain;

public enum AccountType
{
    None,
    Savings,
    Deposit
}

public enum CurrencyType
{
    None,
    SEK
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer
}