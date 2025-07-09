using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendOtpEmailAsync(string email, string otp, string userName)
        {
            try
            {
                var subject = "Password Reset OTP - Ecommerce Store";
                var body = GenerateOtpEmailBody(userName, otp);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {email}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetConfirmationAsync(string email, string userName)
        {
            try
            {
                var subject = "Password Reset Successful - Ecommerce Store";
                var body = GeneratePasswordResetConfirmationBody(userName);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset confirmation to {email}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string userName)
        {
            try
            {
                var subject = "Welcome to Ecommerce Store! 🎉";
                var body = GenerateWelcomeEmailBody(userName);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {email}");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) ||
                    string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("Email configuration is incomplete. Email not sent.");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;
                message.SubjectEncoding = Encoding.UTF8;

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                return false;
            }
        }

        private string GenerateOtpEmailBody(string userName, string otp)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Password Reset OTP</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 60px;
                        height: 60px;
                        background: linear-gradient(135deg, #f59e0b, #d97706);
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 30px;
                        height: 30px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 24px;
                    }}
                    .otp-container {{
                        background: linear-gradient(135deg, #fef3c7, #fde68a);
                        padding: 25px;
                        border-radius: 8px;
                        text-align: center;
                        margin: 25px 0;
                        border-left: 4px solid #f59e0b;
                    }}
                    .otp-code {{
                        font-size: 32px;
                        font-weight: bold;
                        color: #92400e;
                        letter-spacing: 8px;
                        margin: 10px 0;
                        font-family: 'Courier New', monospace;
                    }}
                    .warning {{
                        background: #fef2f2;
                        border-left: 4px solid #f87171;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .warning-text {{
                        color: #dc2626;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background: linear-gradient(135deg, #f59e0b, #d97706);
                        color: white;
                        text-decoration: none;
                        border-radius: 6px;
                        font-weight: 600;
                        margin: 15px 0;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <svg fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z'></path>
                            </svg>
                        </div>
                        <h1>Password Reset Request</h1>
                    </div>
        
                    <p>Hello <strong>{userName}</strong>,</p>
        
                    <p>We received a request to reset your password for your Ecommerce Store account. Use the OTP code below to proceed with resetting your password:</p>
        
                    <div class='otp-container'>
                        <p style='margin: 0; font-size: 16px; color: #92400e;'>Your OTP Code:</p>
                        <div class='otp-code'>{otp}</div>
                        <p style='margin: 0; font-size: 14px; color: #92400e;'>This code expires in 5 minutes</p>
                    </div>
        
                    <p>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
        
                    <div class='warning'>
                        <p class='warning-text'>
                            <strong>Security Notice:</strong> Never share this OTP with anyone. Our team will never ask for your OTP via phone or email.
                        </p>
                    </div>
        
                    <div class='footer'>
                        <p>Best regards,<br>
                        <strong>Your Ecommerce Store Team</strong></p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GeneratePasswordResetConfirmationBody(string userName)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Password Reset Successful</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 60px;
                        height: 60px;
                        background: linear-gradient(135deg, #84cc16, #65a30d);
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 30px;
                        height: 30px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 24px;
                    }}
                    .success-container {{
                        background: linear-gradient(135deg, #ecfdf5, #d1fae5);
                        padding: 25px;
                        border-radius: 8px;
                        text-align: center;
                        margin: 25px 0;
                        border-left: 4px solid #10b981;
                    }}
                    .success-icon {{
                        font-size: 48px;
                        margin-bottom: 15px;
                    }}
                    .info {{
                        background: #eff6ff;
                        border-left: 4px solid #3b82f6;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .info-text {{
                        color: #1e40af;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background: linear-gradient(135deg, #84cc16, #65a30d);
                        color: white;
                        text-decoration: none;
                        border-radius: 6px;
                        font-weight: 600;
                        margin: 15px 0;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <svg fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z'></path>
                            </svg>
                        </div>
                        <h1>Password Reset Successful</h1>
                    </div>
        
                    <p>Hello <strong>{userName}</strong>,</p>
        
                    <div class='success-container'>
                        <div class='success-icon'>✅</div>
                        <p style='margin: 0; font-size: 18px; color: #047857; font-weight: 600;'>
                            Your password has been successfully reset!
                        </p>
                    </div>
        
                    <p>Your account password has been changed successfully. You can now log in to your account using your new password.</p>
        
                    <div class='info'>
                        <p class='info-text'>
                            <strong>Security Tip:</strong> If you didn't make this change, please contact our support team immediately.
                        </p>
                    </div>
        
                    <div style='text-align: center;'>
                        <a href='#' class='button'>Login to Your Account</a>
                    </div>
        
                    <div class='footer'>
                        <p>Best regards,<br>
                        <strong>Your Ecommerce Store Team</strong></p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateWelcomeEmailBody(string userName)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Welcome to Your Ecommerce Store</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 80px;
                        height: 80px;
                        background: linear-gradient(135deg, #059669, #0d9488);
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 40px;
                        height: 40px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 28px;
                    }}
                    .welcome-container {{
                        background: linear-gradient(135deg, #ecfdf5, #d1fae5);
                        padding: 30px;
                        border-radius: 8px;
                        text-align: center;
                        margin: 25px 0;
                        border-left: 4px solid #059669;
                    }}
                    .welcome-icon {{
                        font-size: 64px;
                        margin-bottom: 15px;
                    }}
                    .features {{
                        display: grid;
                        grid-template-columns: 1fr 1fr;
                        gap: 20px;
                        margin: 30px 0;
                    }}
                    .feature {{
                        background: #f8fafc;
                        padding: 20px;
                        border-radius: 8px;
                        text-align: center;
                        border: 1px solid #e2e8f0;
                    }}
                    .feature-icon {{
                        font-size: 32px;
                        margin-bottom: 10px;
                    }}
                    .feature h3 {{
                        margin: 0 0 10px 0;
                        color: #2d3748;
                        font-size: 16px;
                    }}
                    .feature p {{
                        margin: 0;
                        color: #64748b;
                        font-size: 14px;
                    }}
                    .cta {{
                        background: linear-gradient(135deg, #059669, #0d9488);
                        padding: 25px;
                        border-radius: 8px;
                        text-align: center;
                        margin: 25px 0;
                    }}
                    .cta h2 {{
                        color: white;
                        margin: 0 0 15px 0;
                        font-size: 20px;
                    }}
                    .cta p {{
                        color: #a7f3d0;
                        margin: 0 0 20px 0;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background: white;
                        color: #059669;
                        text-decoration: none;
                        border-radius: 6px;
                        font-weight: 600;
                        margin: 10px 0;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                    .social-links {{
                        margin: 20px 0;
                    }}
                    .social-links a {{
                        display: inline-block;
                        margin: 0 10px;
                        color: #6b7280;
                        text-decoration: none;
                    }}
                    @media (max-width: 600px) {{
                        .features {{
                            grid-template-columns: 1fr;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <svg fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z'></path>
                            </svg>
                        </div>
                        <h1>Welcome to Our Store! 🎉</h1>
                    </div>
        
                    <div class='welcome-container'>
                        <div class='welcome-icon'>🛍️</div>
                        <p style='margin: 0; font-size: 20px; color: #047857; font-weight: 600;'>
                            Hello {userName}!
                        </p>
                        <p style='margin: 10px 0 0 0; color: #059669;'>
                            Thank you for joining our community of happy shoppers!
                        </p>
                    </div>
        
                    <p>We're thrilled to have you on board! Your account has been successfully created and you're now ready to explore our amazing collection of products.</p>
        
                    <div class='features'>
                        <div class='feature'>
                            <div class='feature-icon'>🚚</div>
                            <h3>Fast Shipping</h3>
                            <p>Free delivery on orders over $50</p>
                        </div>
                        <div class='feature'>
                            <div class='feature-icon'>🔒</div>
                            <h3>Secure Shopping</h3>
                            <p>Your data is protected with us</p>
                        </div>
                        <div class='feature'>
                            <div class='feature-icon'>💎</div>
                            <h3>Quality Products</h3>
                            <p>Curated selection of premium items</p>
                        </div>
                        <div class='feature'>
                            <div class='feature-icon'>🎁</div>
                            <h3>Special Offers</h3>
                            <p>Exclusive deals for members</p>
                        </div>
                    </div>
        
                    <div class='cta'>
                        <h2>Ready to Start Shopping?</h2>
                        <p>Discover thousands of products at unbeatable prices!</p>
                        <a href='#' class='button'>Browse Products</a>
                    </div>
        
                    <p><strong>What's next?</strong></p>
                    <ul>
                        <li>Complete your profile to get personalized recommendations</li>
                        <li>Subscribe to our newsletter for exclusive deals</li>
                        <li>Follow us on social media for the latest updates</li>
                        <li>Invite friends and earn rewards</li>
                    </ul>
        
                    <div class='footer'>
                        <div class='social-links'>
                            <a href='#'>Facebook</a>
                            <a href='#'>Twitter</a>
                            <a href='#'>Instagram</a>
                            <a href='#'>YouTube</a>
                        </div>
                        <p>Best regards,<br>
                        <strong>Your Ecommerce Store Team</strong></p>
                        <p>Need help? Contact us at support@yourecommercestore.com</p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}
