using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ViewModels;
using BUS_Group4_E_Commerce;
using DAL_Group4_E_Commerce.Models;
using GUI_Group4_ECommerce.Helpers;

namespace Controllers
{
    public class AccountController : Controller
    {
        private readonly BUS_Group4_E_Commerce.IAuthenticationService _authService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IConfiguration _configuration;

        public AccountController(BUS_Group4_E_Commerce.IAuthenticationService authService, ICloudinaryService cloudinaryService, IConfiguration configuration)
        {
            _authService = authService;
            _cloudinaryService = cloudinaryService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                if (model.UserType == "Admin")
                {
                    var adminEmail = _configuration["AdminAccount:Email"];
                    var adminPassword = _configuration["AdminAccount:Password"];
                    if (model.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) &&
                    model.Password == adminPassword)
                    {
                        await SignInUserAsync("0", "Admin", model.Email, "Admin", model.RememberMe);
                        return RedirectToLocal(returnUrl ?? "/Admin");
                    }
                }
                else if (model.UserType == "Customer")
                {
                    var customer = await _authService.AuthenticateCustomerAsync(model.Email, model.Password);
                    if (customer != null)
                    {
                        await SignInUserAsync(customer.CustomerId, customer.FullName, customer.Email, "Customer", model.RememberMe);
                        return RedirectToLocal(returnUrl ?? "/");
                    }
                }
                else if (model.UserType == "Supplier")
                {
                    var supplier = await _authService.AuthenticateSupplierAsync(model.Email, model.Password);
                    if (supplier != null)
                    {
                        await SignInUserAsync(supplier.SupplierId, supplier.CompanyName, supplier.Email, "Supplier", model.RememberMe);
                        return RedirectToLocal(returnUrl ?? "/");
                    }
                }
                else if (model.UserType == "Employee")
                {
                    var employee = await _authService.AuthenticateEmployeeAsync(model.Email, model.Password);
                    if (employee != null)
                    {
                        await SignInUserAsync(employee.EmployeeId, employee.FullName, employee.Email, "Employee", model.RememberMe);
                        return RedirectToLocal(returnUrl ?? "/Admin");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customerService = HttpContext.RequestServices.GetRequiredService<ICustomerService>();
                if (await customerService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already registered.");
                    return View(model);
                }

                string? photoUrl = null;
                // Handle photo upload
                if (model.ProfilePhoto != null)
                {
                    try
                    {
                        var uploadResult = await _cloudinaryService.UploadImageAsync(model.ProfilePhoto, "customer-profiles");
                        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            photoUrl = uploadResult.PublicId;
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ProfilePhoto", $"Photo upload failed: {ex.Message}");
                        return View(model);
                    }
                }

                var customer = new Customer
                {
                    CustomerId = GenerateCustomerId(),
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    Phone = model.Phone,
                    Address = model.Address,
                    Gender = model.Gender,
                    BirthDate = model.BirthDate,
                    Photo = photoUrl ?? "default-avatar"
                };

                var result = await _authService.RegisterCustomerAsync(customer);
                if (result)
                {
                    // Send welcome email
                    var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
                    await emailService.SendWelcomeEmailAsync(customer.Email, customer.FullName);

                    TempData["SuccessMessage"] = "Registration successful! Please login with your credentials.";
                    return RedirectToAction("Login");
                }
                else
                {
                    // If registration failed and we uploaded a photo, delete it
                    if (!string.IsNullOrEmpty(photoUrl))
                    {
                        try
                        {
                            await _cloudinaryService.DeleteImageAsync(photoUrl);
                        }
                        catch
                        {
                            // Log error but don't fail the request
                        }
                    }
                    ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var otpService = HttpContext.RequestServices.GetRequiredService<IOtpService>();
                var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();

                // Check if user exists
                bool userExists = false;
                string userName = string.Empty;

                if (model.UserType == "Customer")
                {
                    var customerService = HttpContext.RequestServices.GetRequiredService<ICustomerService>();
                    var customer = await customerService.GetCustomerByEmailAsync(model.Email);
                    if (customer != null)
                    {
                        userExists = true;
                        userName = customer.FullName;
                    }
                }
                else if (model.UserType == "Employee")
                {
                    var employeeService = HttpContext.RequestServices.GetRequiredService<IEmployeeService>();
                    var employee = await employeeService.GetEmployeeByEmailAsync(model.Email);
                    if (employee != null)
                    {
                        userExists = true;
                        userName = employee.FullName;
                    }
                }

                if (userExists)
                {
                    // Generate and send OTP
                    var otp = otpService.GenerateOtp();
                    await otpService.StoreOtpAsync(model.Email, otp, model.UserType);
                    await emailService.SendOtpEmailAsync(model.Email, otp, userName);
                    
                    TempData["SuccessMessage"] = "OTP has been sent to your email address.";
                    TempData["Email"] = model.Email;
                    TempData["UserType"] = model.UserType;
                    return RedirectToAction("VerifyOtp");
                }
                else
                {
                    // Don't reveal if email exists or not for security
                    TempData["SuccessMessage"] = "If the email exists in our system, an OTP has been sent.";
                    TempData["Email"] = model.Email;
                    TempData["UserType"] = model.UserType;
                    return RedirectToAction("VerifyOtp");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var model = new VerifyOtpViewModel
            {
                Email = TempData["Email"]?.ToString() ?? string.Empty,
                UserType = TempData["UserType"]?.ToString() ?? "Customer"
            };

            if (string.IsNullOrEmpty(model.Email))
            {
                return RedirectToAction("ForgotPassword");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var otpService = HttpContext.RequestServices.GetRequiredService<IOtpService>();

                var isValidOtp = await otpService.ValidateOtpAsync(model.Email, model.Otp, model.UserType);

                if (isValidOtp)
                {
                    // Generate reset token
                    var resetToken = await otpService.GenerateResetTokenAsync(model.Email, model.UserType);

                    TempData["ResetToken"] = resetToken;
                    TempData["Email"] = model.Email;
                    TempData["UserType"] = model.UserType;

                    return RedirectToAction("ResetPassword");
                }
                else
                {
                    ModelState.AddModelError("Otp", "Invalid or expired OTP. Please try again.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var model = new ResetPasswordViewModel
            {
                Email = TempData["Email"]?.ToString() ?? string.Empty,
                Token = TempData["ResetToken"]?.ToString() ?? string.Empty,
                UserType = TempData["UserType"]?.ToString() ?? "Customer"
            };

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Token))
            {
                return RedirectToAction("ForgotPassword");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var otpService = HttpContext.RequestServices.GetRequiredService<IOtpService>();
                var passwordHashingService = HttpContext.RequestServices.GetRequiredService<IPasswordHashingService>();
                var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();

                // Validate reset token
                var isValidToken = await otpService.ValidateResetTokenAsync(model.Email, model.Token, model.UserType);

                if (isValidToken)
                {
                    bool passwordUpdated = false;
                    string userName = string.Empty;

                    if (model.UserType == "Customer")
                    {
                        var customerService = HttpContext.RequestServices.GetRequiredService<ICustomerService>();
                        var customer = await customerService.GetCustomerByEmailAsync(model.Email);
                        if (customer != null)
                        {
                            customer.Password = passwordHashingService.HashPassword(model.NewPassword);
                            passwordUpdated = await customerService.UpdateCustomerAsync(customer);
                            userName = customer.FullName;
                        }
                    }
                    else if (model.UserType == "Employee")
                    {
                        var employeeService = HttpContext.RequestServices.GetRequiredService<IEmployeeService>();
                        var employee = await employeeService.GetEmployeeByEmailAsync(model.Email);
                        if (employee != null)
                        {
                            employee.Password = passwordHashingService.HashPassword(model.NewPassword);
                            passwordUpdated = await employeeService.UpdateEmployeeAsync(employee);
                            userName = employee.FullName;
                        }
                    }

                    if (passwordUpdated)
                    {
                        // Clear OTP and token data
                        await otpService.ClearOtpAsync(model.Email, model.UserType);

                        // Send confirmation email
                        await emailService.SendPasswordResetConfirmationAsync(model.Email, userName);

                        TempData["SuccessMessage"] = "Your password has been reset successfully. You can now login with your new password.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to reset password. Please try again.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid or expired reset token. Please start the process again.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUserAsync(string userId, string fullName, string email, string userType, bool rememberMe)
        {
            Console.WriteLine("Signing in: " + userId); // hoặc Debug.WriteLine

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Email, email),
                new Claim("UserType", userType),
                new Claim(MySetting.CLAIM_CUSTOMERID, userId)

            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private string GenerateCustomerId()
        {
            return "CUS" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(10000, 99999);
        }
    }
}
