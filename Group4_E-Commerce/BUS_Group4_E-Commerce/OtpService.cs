using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public class OtpService : IOtpService
    {
        private readonly ConcurrentDictionary<string, OtpData> _otpStorage = new();
        private readonly ConcurrentDictionary<string, ResetTokenData> _tokenStorage = new();
        private readonly ILogger<OtpService> _logger;

        public OtpService(ILogger<OtpService> logger)
        {
            _logger = logger;
        }

        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<bool> StoreOtpAsync(string email, string otp, string userType)
        {
            try
            {
                var key = $"{email}_{userType}";
                var otpData = new OtpData
                {
                    Otp = otp,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5), // OTP expires in 5 minutes
                    AttemptCount = 0
                };

                _otpStorage.AddOrUpdate(key, otpData, (k, v) => otpData);

                // Clean up expired OTPs
                _ = Task.Run(CleanupExpiredOtps);

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to store OTP for {email}");
                return false;
            }
        }

        public async Task<bool> ValidateOtpAsync(string email, string otp, string userType)
        {
            try
            {
                var key = $"{email}_{userType}";

                if (_otpStorage.TryGetValue(key, out var otpData))
                {
                    if (DateTime.UtcNow > otpData.ExpiryTime)
                    {
                        _otpStorage.TryRemove(key, out _);
                        return false; // OTP expired
                    }

                    if (otpData.AttemptCount >= 3)
                    {
                        _otpStorage.TryRemove(key, out _);
                        return false; // Too many attempts
                    }

                    otpData.AttemptCount++;

                    if (otpData.Otp == otp)
                    {
                        return await Task.FromResult(true);
                    }
                }

                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to validate OTP for {email}");
                return false;
            }
        }

        public async Task<string> GenerateResetTokenAsync(string email, string userType)
        {
            try
            {
                var token = Guid.NewGuid().ToString("N");
                var key = $"{email}_{userType}";

                var tokenData = new ResetTokenData
                {
                    Token = token,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(15) // Token expires in 15 minutes
                };

                _tokenStorage.AddOrUpdate(key, tokenData, (k, v) => tokenData);

                // Clean up expired tokens
                _ = Task.Run(CleanupExpiredTokens);

                return await Task.FromResult(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate reset token for {email}");
                return string.Empty;
            }
        }

        public async Task<bool> ValidateResetTokenAsync(string email, string token, string userType)
        {
            try
            {
                var key = $"{email}_{userType}";

                if (_tokenStorage.TryGetValue(key, out var tokenData))
                {
                    if (DateTime.UtcNow > tokenData.ExpiryTime)
                    {
                        _tokenStorage.TryRemove(key, out _);
                        return false; // Token expired
                    }

                    return await Task.FromResult(tokenData.Token == token);
                }

                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to validate reset token for {email}");
                return false;
            }
        }

        public async Task ClearOtpAsync(string email, string userType)
        {
            var key = $"{email}_{userType}";
            _otpStorage.TryRemove(key, out _);
            _tokenStorage.TryRemove(key, out _);
            await Task.CompletedTask;
        }

        private void CleanupExpiredOtps()
        {
            var expiredKeys = _otpStorage
                .Where(kvp => DateTime.UtcNow > kvp.Value.ExpiryTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _otpStorage.TryRemove(key, out _);
            }
        }

        private void CleanupExpiredTokens()
        {
            var expiredKeys = _tokenStorage
                .Where(kvp => DateTime.UtcNow > kvp.Value.ExpiryTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _tokenStorage.TryRemove(key, out _);
            }
        }

        private class OtpData
        {
            public string Otp { get; set; } = string.Empty;
            public DateTime ExpiryTime { get; set; }
            public int AttemptCount { get; set; }
        }

        private class ResetTokenData
        {
            public string Token { get; set; } = string.Empty;
            public DateTime ExpiryTime { get; set; }
        }
    }
}
