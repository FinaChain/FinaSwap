namespace FinaSwap.Api.Services;

public interface IEtherSendService
{
    Task Send(string address);
}