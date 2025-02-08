using E_handel.Payment.Models;

namespace E_handel.Payment.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreateOrderAsync(OrderRequest order);
    }
}