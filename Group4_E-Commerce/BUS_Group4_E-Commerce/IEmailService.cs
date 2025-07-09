using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string email, string otp, string userName);
        Task<bool> SendPasswordResetConfirmationAsync(string email, string userName);
        Task<bool> SendWelcomeEmailAsync(string email, string userName);
    }
}
