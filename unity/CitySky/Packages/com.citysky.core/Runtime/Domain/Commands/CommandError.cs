namespace CitySky.Domain
{
    /// <summary>Erros tipados de comando (falhas esperadas, não exceções). ARCHITECTURE §9.</summary>
    public enum CommandError
    {
        None = 0,
        InsufficientFunds = 1,
        InvalidTarget = 2,
        NotUnlocked = 3,
        Rejected = 4,
    }
}
