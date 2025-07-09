using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task<bool> StoreOtpAsync(string email, string otp, string userType);
        Task<bool> ValidateOtpAsync(string email, string otp, string userType);
        Task<string> GenerateResetTokenAsync(string email, string userType);
        Task<bool> ValidateResetTokenAsync(string email, string token, string userType);
        Task ClearOtpAsync(string email, string userType);
    }
}
