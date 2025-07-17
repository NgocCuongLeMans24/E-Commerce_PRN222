using BUS_Group4_E_Commerce;
using DAL_Group4_E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace Controllers
{
    //[Authorize(Policy = "EmployeeOnly")]
    public class AdminController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IEmailService _emailService;

        public AdminController(
            ICustomerService customerService,
            IEmployeeService employeeService,
            IPasswordHashingService passwordHashingService,
            IEmailService emailService)
        {
            _customerService = customerService;
            _employeeService = employeeService;
            _passwordHashingService = passwordHashingService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var employees = await _employeeService.GetAllEmployeesAsync();
            var activeCustomers = await _customerService.GetActiveCustomersAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalCustomers = customers.Count(),
                ActiveCustomers = activeCustomers.Count(),
                InactiveCustomers = customers.Count() - activeCustomers.Count(),
                TotalEmployees = employees.Count(),
                TotalOrders = 0, // TODO: Implement when Order service is available
                TotalProducts = 0, // TODO: Implement when Product service is available
                RecentCustomers = customers.OrderByDescending(c => c.CustomerId).Take(5).ToList(),
                RecentEmployees = employees.OrderByDescending(e => e.EmployeeId).Take(5).ToList()
            };

            return View(viewModel);
        }

        #region Customer Management

        public async Task<IActionResult> Customers(CustomerManagementViewModel model)
        {
            var customers = await _customerService.GetAllCustomersAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(model.SearchTerm))
            {
                customers = customers.Where(c =>
                    c.FullName.Contains(model.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(model.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.CustomerId.Contains(model.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply status filter
            if (model.StatusFilter != "All")
            {
                bool isActive = model.StatusFilter == "Active";
                customers = customers.Where(c => c.IsActive == isActive);
            }

            // Apply sorting
            customers = model.SortBy.ToLower() switch
            {
                "email" => model.SortOrder == "desc"
                    ? customers.OrderByDescending(c => c.Email)
                    : customers.OrderBy(c => c.Email),
                "birthdate" => model.SortOrder == "desc"
                    ? customers.OrderByDescending(c => c.BirthDate)
                    : customers.OrderBy(c => c.BirthDate),
                _ => model.SortOrder == "desc"
                    ? customers.OrderByDescending(c => c.FullName)
                    : customers.OrderBy(c => c.FullName)
            };

            model.TotalCount = customers.Count();

            // Apply pagination
            var pagedCustomers = customers
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList();

            model.Customers = pagedCustomers;

            return View(model);
        }

        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(CreateCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if Customer ID already exists
                if (await _customerService.GetCustomerByIdAsync(model.CustomerID) != null)
                {
                    ModelState.AddModelError("CustomerID", "Customer ID already exists.");
                    return View(model);
                }

                // Check if email already exists
                if (await _customerService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already registered.");
                    return View(model);
                }

                var customer = new Customer
                {
                    CustomerId = model.CustomerID,
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password, // Will be hashed in the service
                    Phone = model.Phone,
                    Address = model.Address,
                    Gender = model.Gender,
                    BirthDate = model.BirthDate,
                    IsActive = model.IsActive,
                    Photo = "Photo.gif",
                    Role = 0,
                    RandomKey = Guid.NewGuid().ToString()
                };

                var result = await _customerService.RegisterCustomerAsync(customer);
                if (result)
                {
                    // Send welcome email
                    await _emailService.SendWelcomeEmailAsync(customer.Email, customer.FullName);

                    TempData["SuccessMessage"] = "Customer created successfully!";
                    return RedirectToAction("Customers");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create customer. Please try again.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var model = new EditCustomerViewModel
            {
                CustomerID = customer.CustomerId,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                Gender = (bool)customer.Gender,
                BirthDate = (DateTime)customer.BirthDate,
                IsActive = (bool)customer.IsActive,
                Photo = customer.Photo
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(EditCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = await _customerService.GetCustomerByIdAsync(model.CustomerID);
                if (customer == null)
                {
                    return NotFound();
                }

                // Check if email is being changed and if it already exists
                if (customer.Email != model.Email && await _customerService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already in use.");
                    return View(model);
                }

                customer.FullName = model.FullName;
                customer.Email = model.Email;
                customer.Phone = model.Phone;
                customer.Address = model.Address;
                customer.Gender = model.Gender;
                customer.BirthDate = model.BirthDate;
                customer.IsActive = model.IsActive;
                customer.Photo = model.Photo;

                var result = await _customerService.UpdateCustomerAsync(customer);
                if (result)
                {
                    TempData["SuccessMessage"] = "Customer updated successfully!";
                    return RedirectToAction("Customers");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to update customer. Please try again.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid customer ID." });
            }

            var result = await _customerService.DeactivateCustomerAsync(id);
            if (result)
            {
                return Json(new { success = true, message = "Customer deactivated successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to deactivate customer." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCustomerStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid customer ID." });
            }

            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found." });
            }

            bool result;
            string message;

            if ((bool)customer.IsActive)
            {
                result = await _customerService.DeactivateCustomerAsync(id);
                message = result ? "Customer deactivated successfully!" : "Failed to deactivate customer.";
                if (result) TempData["SuccessMessage"] = "Customer deactivated successfully!";
            }
            else
            {
                result = await _customerService.ActivateCustomerAsync(id);
                message = result ? "Customer activated successfully!" : "Failed to activate customer.";
                if (result) TempData["SuccessMessage"] = "Customer activated successfully!";
            }

            
            return Json(new { success = result, message = message });
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerModal(string id = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Create new customer
                var createModel = new CreateCustomerViewModel
                {
                    BirthDate = DateTime.Now.AddYears(-18),
                    IsActive = true
                };
                return PartialView("_CreateCustomerModal", createModel);
            }
            else
            {
                // Edit existing customer
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                var editModel = new EditCustomerViewModel
                {
                    CustomerID = customer.CustomerId,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    Gender = (bool)customer.Gender,
                    BirthDate = (DateTime)customer.BirthDate,
                    IsActive = (bool)customer.IsActive,
                    Photo = customer.Photo
                };
                return PartialView("_EditCustomerModal", editModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomerModal(CreateCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if Customer ID already exists
                if (await _customerService.GetCustomerByIdAsync(model.CustomerID) != null)
                {
                    return Json(new { success = false, message = "Customer ID already exists." });
                }

                // Check if email already exists
                if (await _customerService.EmailExistsAsync(model.Email))
                {
                    return Json(new { success = false, message = "Email address is already registered." });
                }

                var customer = new Customer
                {
                    CustomerId = model.CustomerID,
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    Phone = model.Phone,
                    Address = model.Address,
                    Gender = model.Gender,
                    BirthDate = model.BirthDate,
                    IsActive = model.IsActive,
                    Photo = "default-avatar",
                    Role = 0,
                    RandomKey = Guid.NewGuid().ToString()
                };

                var result = await _customerService.RegisterCustomerAsync(customer);
                if (result)
                {
                    await _emailService.SendWelcomeEmailAsync(customer.Email, customer.FullName);
                    TempData["SuccessMessage"] = "Customer created successfully!";
                    return Json(new { success = true, message = "Customer created successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create customer. Please try again." });
                }
            }

            return PartialView("_CreateCustomerModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomerModal(EditCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = await _customerService.GetCustomerByIdAsync(model.CustomerID);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found." });
                }

                // Check if email is being changed and if it already exists
                if (customer.Email != model.Email && await _customerService.EmailExistsAsync(model.Email))
                {
                    return Json(new { success = false, message = "Email address is already in use." });
                }

                customer.FullName = model.FullName;
                customer.Email = model.Email;
                customer.Phone = model.Phone;
                customer.Address = model.Address;
                customer.Gender = model.Gender;
                customer.BirthDate = model.BirthDate;
                customer.IsActive = model.IsActive;
                customer.Photo = model.Photo;

                var result = await _customerService.UpdateCustomerAsync(customer);
                if (result)
                {
                    TempData["SuccessMessage"] = "Customer updated successfully!";
                    return Json(new { success = true, message = "Customer updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update customer. Please try again." });
                }
            }

            return PartialView("_EditCustomerModal", model);
        }

        #endregion

        #region Employee Management

        public async Task<IActionResult> Employees(EmployeeManagementViewModel model)
        {
            var employees = await _employeeService.GetAllEmployeesAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(model.SearchTerm))
            {
                employees = employees.Where(e =>
                    e.FullName.Contains(model.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.Email.Contains(model.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.EmployeeId.Contains(model.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            employees = model.SortBy.ToLower() switch
            {
                "email" => model.SortOrder == "desc"
                    ? employees.OrderByDescending(e => e.Email)
                    : employees.OrderBy(e => e.Email),
                _ => model.SortOrder == "desc"
                    ? employees.OrderByDescending(e => e.FullName)
                    : employees.OrderBy(e => e.FullName)
            };

            model.TotalCount = employees.Count();

            // Apply pagination
            var pagedEmployees = employees
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList();

            model.Employees = pagedEmployees;

            return View(model);
        }

        [HttpGet]
        public IActionResult CreateEmployee()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if Employee ID already exists
                if (await _employeeService.GetEmployeeByIdAsync(model.EmployeeID) != null)
                {
                    ModelState.AddModelError("EmployeeID", "Employee ID already exists.");
                    return View(model);
                }

                // Check if email already exists
                if (await _employeeService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already registered.");
                    return View(model);
                }

                var employee = new Employee
                {
                    EmployeeId = model.EmployeeID,
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password // Will be hashed in the service
                };

                var result = await _employeeService.CreateEmployeeAsync(employee, model.Password);
                if (result)
                {
                    TempData["SuccessMessage"] = "Employee created successfully!";
                    return RedirectToAction("Employees");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create employee. Please try again.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditEmployee(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new EditEmployeeViewModel
            {
                EmployeeID = employee.EmployeeId,
                FullName = employee.FullName,
                Email = employee.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(EditEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeID);
                if (employee == null)
                {
                    return NotFound();
                }

                // Check if email is being changed and if it already exists
                if (employee.Email != model.Email && await _employeeService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already in use.");
                    return View(model);
                }

                employee.FullName = model.FullName;
                employee.Email = model.Email;

                var result = await _employeeService.UpdateEmployeeAsync(employee);
                if (result)
                {
                    TempData["SuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction("Employees");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to update employee. Please try again.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid employee ID." });
            }

            var result = await _employeeService.DeleteAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Employee deleted successfully!";
                return Json(new { success = true, message = "Employee deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete employee." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeModal(string id = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Create new employee
                var createModel = new CreateEmployeeViewModel();
                return PartialView("_CreateEmployeeModal", createModel);
            }
            else
            {
                // Edit existing employee
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }

                var editModel = new EditEmployeeViewModel
                {
                    EmployeeID = employee.EmployeeId,
                    FullName = employee.FullName,
                    Email = employee.Email
                };
                return PartialView("_EditEmployeeModal", editModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployeeModal(CreateEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if Employee ID already exists
                if (await _employeeService.GetEmployeeByIdAsync(model.EmployeeID) != null)
                {
                    return Json(new { success = false, message = "Employee ID already exists." });
                }

                // Check if email already exists
                if (await _employeeService.EmailExistsAsync(model.Email))
                {
                    return Json(new { success = false, message = "Email address is already registered." });
                }

                var employee = new Employee
                {
                    EmployeeId = model.EmployeeID,
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password
                };

                var result = await _employeeService.CreateEmployeeAsync(employee, model.Password);
                if (result)
                {
                    TempData["SuccessMessage"] = "Employee created successfully!";
                    return Json(new { success = true, message = "Employee created successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create employee. Please try again." });
                }
            }
            return PartialView("_CreateEmployeeModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployeeModal(EditEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeID);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Employee not found." });
                }

                // Check if email is being changed and if it already exists
                if (employee.Email != model.Email && await _employeeService.EmailExistsAsync(model.Email))
                {
                    return Json(new { success = false, message = "Email address is already in use." });
                }

                employee.FullName = model.FullName;
                employee.Email = model.Email;

                var result = await _employeeService.UpdateEmployeeAsync(employee);
                if (result)
                {
                    TempData["SuccessMessage"] = "Employee created successfully!";
                    return Json(new { success = true, message = "Employee updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update employee. Please try again." });
                }
            }

            return PartialView("_EditEmployeeModal", model);
        }

        #endregion
    }
}
