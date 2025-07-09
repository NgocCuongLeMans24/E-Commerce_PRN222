using BUS_Group4_E_Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using ViewModels;

namespace Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly ICloudinaryService _cloudinaryService;

        public ProfileController(
            ICustomerService customerService,
            IEmployeeService employeeService,
            IPasswordHashingService passwordHashingService,
            ICloudinaryService cloudinaryService)
        {
            _customerService = customerService;
            _employeeService = employeeService;
            _passwordHashingService = passwordHashingService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = User.FindFirst("UserType")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            {
                return RedirectToAction("Login", "Account");
            }

            var profileViewModel = await GetProfileViewModelAsync(userId, userType);
            return View(profileViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = User.FindFirst("UserType")?.Value;

            if (userId != model.UserId || userType != model.UserType)
            {
                return BadRequest();
            }

            // Remove password-related validation errors since we're only updating profile
            ModelState.Remove("CurrentPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                bool updateResult = false;
                string? newPhotoUrl = null;
                string? oldPhotoUrl = null;

                // Handle photo upload
                if (model.ProfilePhoto != null)
                {
                    try
                    {
                        var uploadResult = await _cloudinaryService.UploadImageAsync(model.ProfilePhoto, "customer-profiles");
                        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Debug.WriteLine("Upload to Cloudinary failed");
                            newPhotoUrl = uploadResult.PublicId;
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ProfilePhoto", $"Photo upload failed: {ex.Message}");
                        return View("Index", model);
                    }
                }

                if (model.UserType == "Customer")
                {
                    var customer = await _customerService.GetCustomerByIdAsync(model.UserId);
                    if (customer != null)
                    {
                        // Check if email is being changed and if it already exists
                        if (customer.Email != model.Email)
                        {
                            if (await _customerService.EmailExistsAsync(model.Email))
                            {
                                ModelState.AddModelError("Email", "Email address is already in use.");
                                return View("Index", model);
                            }
                        }

                        // Store old photo URL for deletion
                        oldPhotoUrl = customer.Photo;
                        customer.FullName = model.FullName;
                        customer.Email = model.Email;
                        customer.Phone = model.Phone;
                        customer.Address = model.Address;
                        if (model.Gender.HasValue)
                            customer.Gender = model.Gender.Value;
                        if (model.BirthDate.HasValue)
                            customer.BirthDate = model.BirthDate.Value;

                        // Update photo if new one was uploaded
                        if (!string.IsNullOrEmpty(newPhotoUrl))
                        {
                            customer.Photo = newPhotoUrl;
                        }

                        updateResult = await _customerService.UpdateCustomerAsync(customer);
                    }
                }
                else if (model.UserType == "Employee")
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(model.UserId);
                    if (employee != null)
                    {
                        // Check if email is being changed and if it already exists
                        if (employee.Email != model.Email)
                        {
                            if (await _employeeService.EmailExistsAsync(model.Email))
                            {
                                ModelState.AddModelError("Email", "Email address is already in use.");
                                return View("Index", model);
                            }
                        }

                        employee.FullName = model.FullName;
                        employee.Email = model.Email;

                        updateResult = await _employeeService.UpdateEmployeeAsync(employee);
                    }
                }

                if (updateResult)
                {
                    // Delete old photo if a new one was uploaded and old one exists
                    if (!string.IsNullOrEmpty(newPhotoUrl) && !string.IsNullOrEmpty(oldPhotoUrl) &&
                        oldPhotoUrl != "default-avatar" && oldPhotoUrl != newPhotoUrl)
                    {
                        try
                        {
                            await _cloudinaryService.DeleteImageAsync(oldPhotoUrl);
                        }
                        catch
                        {
                            // Log error but don't fail the request
                        }
                    }

                    TempData["ProfileSuccessMessage"] = "Profile updated successfully!";
                }
                else
                {
                    // If update failed and we uploaded a new photo, delete it
                    if (!string.IsNullOrEmpty(newPhotoUrl))
                    {
                        try
                        {
                            await _cloudinaryService.DeleteImageAsync(newPhotoUrl);
                        }
                        catch
                        {
                            // Log error but don't fail the request
                        }
                    }
                    TempData["ProfileErrorMessage"] = "Failed to update profile. Please try again.";
                }
            }
            else
            {
                TempData["ProfileErrorMessage"] = "Please correct the errors and try again.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfileViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = User.FindFirst("UserType")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            {
                return RedirectToAction("Login", "Account");
            }

            // Ensure the model has the correct user info
            model.UserId = userId;
            model.UserType = userType;

            // Remove profile-related validation errors since we're only changing password
            var profileFields = new[] { "FullName", "Email", "Phone", "Address", "Gender", "BirthDate" };
            foreach (var field in profileFields)
            {
                ModelState.Remove(field);
            }

            // Validate password fields
            if (string.IsNullOrEmpty(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is required");
            }

            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "New password is required");
            }
            else if (model.NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "Password must be at least 6 characters long");
            }

            if (string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.AddModelError("ConfirmPassword", "Confirm password is required");
            }
            else if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "New password and confirmation password do not match");
            }

            if (ModelState.IsValid)
            {
                bool passwordChangeResult = false;
                bool currentPasswordValid = false;

                if (userType == "Customer")
                {
                    var customer = await _customerService.GetCustomerByIdAsync(userId);
                    if (customer != null && !string.IsNullOrEmpty(customer.Password))
                    {
                        // Verify current password
                        if (_passwordHashingService.VerifyPassword(model.CurrentPassword, customer.Password))
                        {
                            currentPasswordValid = true;
                            customer.Password = _passwordHashingService.HashPassword(model.NewPassword);
                            passwordChangeResult = await _customerService.UpdateCustomerAsync(customer);
                        }
                    }
                }
                else if (userType == "Employee")
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(userId);
                    if (employee != null && !string.IsNullOrEmpty(employee.Password))
                    {
                        // Verify current password
                        if (_passwordHashingService.VerifyPassword(model.CurrentPassword, employee.Password))
                        {
                            currentPasswordValid = true;
                            employee.Password = _passwordHashingService.HashPassword(model.NewPassword);
                            passwordChangeResult = await _employeeService.UpdateEmployeeAsync(employee);
                        }
                    }
                }

                if (!currentPasswordValid)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                    TempData["ShowPasswordTab"] = "true";
                    return View("Index", model);
                }
                else if (passwordChangeResult)
                {
                    TempData["PasswordSuccessMessage"] = "Password changed successfully!";
                    TempData["ShowPasswordTab"] = "true";

                    // Clear password fields after successful change
                    model.CurrentPassword = null;
                    model.NewPassword = null;
                    model.ConfirmPassword = null;

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to change password. Please try again.");
                    TempData["ShowPasswordTab"] = "true";
                    return View("Index", model);
                }
            }
            else
            {
                // Return to view with validation errors
                TempData["ShowPasswordTab"] = "true";
                return View("Index", model);
            }
        }

        private async Task<ProfileViewModel> GetProfileViewModelAsync(string userId, string userType)
        {
            var profileViewModel = new ProfileViewModel
            {
                UserId = userId,
                UserType = userType
            };

            if (userType == "Customer")
            {
                var customer = await _customerService.GetCustomerByIdAsync(userId);
                if (customer != null)
                {
                    profileViewModel.FullName = customer.FullName;
                    profileViewModel.Email = customer.Email;
                    profileViewModel.Phone = customer.Phone;
                    profileViewModel.Address = customer.Address;
                    profileViewModel.Gender = customer.Gender;
                    profileViewModel.BirthDate = customer.BirthDate;
                    profileViewModel.Photo = customer.Photo;
                    profileViewModel.IsActive = customer.IsActive;

                    // Get optimized photo URL
                    if (!string.IsNullOrEmpty(customer.Photo) && customer.Photo != "default-avatar")
                    {
                        profileViewModel.CurrentPhotoUrl = _cloudinaryService.GetOptimizedImageUrl(customer.Photo);
                    }
                }
            }
            else if (userType == "Employee")
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(userId);
                if (employee != null)
                {
                    profileViewModel.FullName = employee.FullName;
                    profileViewModel.Email = employee.Email;
                }
            }

            return profileViewModel;
        }
    }
}
