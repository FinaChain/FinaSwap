namespace FinaSwap.Api.Services;

public interface IBase58ValidationService
{
    bool IsValid(string input);
    bool IsValidPlain(string input);
}