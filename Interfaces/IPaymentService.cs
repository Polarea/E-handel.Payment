namespace E_handel.Payment.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreateOrderAsync(object order);
    }
}