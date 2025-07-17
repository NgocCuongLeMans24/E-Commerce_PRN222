using GUI_Group4_ECommerce.ViewModels;

namespace GUI_Group4_ECommerce.Services
{
    public interface IVnPayService
{
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
