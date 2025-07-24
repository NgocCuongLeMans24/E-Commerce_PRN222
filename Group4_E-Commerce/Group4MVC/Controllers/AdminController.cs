using BUS_Group4_E_Commerce;
using DAL_Group4_E_Commerce.Models;
using Group4MVC.Attributes;
using Group4MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViewModels;

namespace Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IEmailService _emailService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly EcommercePrn222Context _context;
        private readonly IPermissionService _permissionService;
        private readonly ISupplierService _supplierService;

        public AdminController(
            ICustomerService customerService,
            IEmployeeService employeeService,
            IPasswordHashingService passwordHashingService,
            IEmailService emailService,
            IOrderService orderService,
            IProductService productService,
            EcommercePrn222Context context,
            IPermissionService permissionService,
            ISupplierService supplierService)
        {
            _customerService = customerService;
            _employeeService = employeeService;
            _passwordHashingService = passwordHashingService;
            _emailService = emailService;
            _orderService = orderService;
            _productService = productService;
            _context = context;
            _permissionService = permissionService;
            _supplierService = supplierService;
		}

        [RequirePermission("view", "/admin")]
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var employees = await _employeeService.GetAllEmployeesAsync();
            var activeCustomers = await _customerService.GetActiveCustomersAsync();
            var orders = await _orderService.GetAllOrdersAsync(); 
            var products =  _productService.GetAll();


			var viewModel = new AdminDashboardViewModel
            {
                TotalCustomers = customers.Count(),
                ActiveCustomers = activeCustomers.Count(),
                InactiveCustomers = customers.Count() - activeCustomers.Count(),
                TotalEmployees = employees.Count(),
                TotalOrders = orders.Count(),
				TotalProducts = products.Count(), // TODO: Implement when Product service is available
                RecentCustomers = customers.OrderByDescending(c => c.CustomerId).Take(5).ToList(),
                RecentEmployees = employees.OrderByDescending(e => e.EmployeeId).Take(5).ToList()
            };

            return View(viewModel);
        }

        #region Customer Management

        [RequirePermission("view", "/admin/customers")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("delete", "/admin/customers")]
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
        [RequirePermission("add", "/admin/customers")]
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
        [RequirePermission("edit", "/admin/customers")]
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

        [RequirePermission("view", "/admin/employees")]
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

        [RequirePermission("view", "/admin/permissions")]
        public async Task<IActionResult> Permissions(int pageNumber = 1, int pageSize = 12, string searchTerm = "", int? pageFilter = null)
        {

            // Check permissions for actions
            var currentEmployeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.CanEditPermissions = currentEmployeeId == "0" ? true : await _permissionService.HasPermissionAsync(currentEmployeeId, "/admin/permissions", "edit");

            var employees = await _context.Employees.ToListAsync();
            var webPages = await _context.WebPages.ToListAsync();
            var permissions = await _context.Permissions.ToListAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                employees = employees.Where(e =>
                    e.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (pageFilter.HasValue)
            {
                var employeeIdsWithPageAccess = permissions
                    .Where(p => p.PageId == pageFilter.Value && p.CanView)
                    .Select(p => p.EmployeeId)
                    .ToList();
                employees = employees.Where(e => employeeIdsWithPageAccess.Contains(e.EmployeeId)).ToList();
            }

            var totalCount = employees.Count();
            var pagedEmployees = employees.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new EmployeePermissionViewModel
            {
                Employees = pagedEmployees,
                WebPages = webPages,
                Permissions = permissions,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                PageFilter = pageFilter
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeePermissions(string employeeId)
        {
            try
            {
                var permissions = await _context.Permissions
                    .Include(p => p.Page)
                    .Where(p => p.EmployeeId == employeeId)
                    .Select(p => new EmployeePermissionDto
                    {
                        PageId = p.PageId ?? 0,
                        PageName = p.Page != null ? p.Page.PageName : "",
                        PageUrl = p.Page != null ? p.Page.Url : "",
                        CanView = p.CanView,
                        CanAdd = p.CanAdd,
                        CanEdit = p.CanEdit,
                        CanDelete = p.CanDelete
                    })
                    .ToListAsync();

                return Json(permissions);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        [RequirePermission("edit", "/admin/permissions")]
        public async Task<IActionResult> UpdatePermissions(UpdateEmployeePermissionsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Invalid data provided.";
                    return RedirectToAction("Permissions");
                }

                var success = await _permissionService.UpdateEmployeePermissionsAsync(model.EmployeeId, model.Permissions);

                if (success)
                {
                    TempData["SuccessMessage"] = "Permissions updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update permissions.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating permissions: {ex.Message}";
            }

            return RedirectToAction("Permissions");
        }

        [RequirePermission("view", "/admin/webpages")]
        public async Task<IActionResult> WebPages(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortBy = "PageName", string sortOrder = "asc")
        {

            // Check permissions for actions
            var employeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.CanAddWebPage = employeeId == "0" ? true : await _permissionService.HasPermissionAsync(employeeId, "/admin/webpages", "add");
            ViewBag.CanEditWebPage = employeeId == "0" ? true : await _permissionService.HasPermissionAsync(employeeId, "/admin/webpages", "edit");
            ViewBag.CanDeleteWebPage = employeeId == "0" ? true : await _permissionService.HasPermissionAsync(employeeId, "/admin/webpages", "delete");

            var webPagesQuery = _context.WebPages.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                webPagesQuery = webPagesQuery.Where(w =>
                    w.PageName.Contains(searchTerm) ||
                    w.Url.Contains(searchTerm));
            }

            // Apply sorting
            webPagesQuery = sortBy.ToLower() switch
            {
                "url" => sortOrder == "desc" ? webPagesQuery.OrderByDescending(w => w.Url) : webPagesQuery.OrderBy(w => w.Url),
                "pageid" => sortOrder == "desc" ? webPagesQuery.OrderByDescending(w => w.PageId) : webPagesQuery.OrderBy(w => w.PageId),
                _ => sortOrder == "desc" ? webPagesQuery.OrderByDescending(w => w.PageName) : webPagesQuery.OrderBy(w => w.PageName)
            };

            var totalCount = await webPagesQuery.CountAsync();
            var webPages = await webPagesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var permissions = await _context.Permissions.ToListAsync();

            var viewModel = new WebPageManagementViewModel
            {
                WebPages = webPages,
                Permissions = permissions,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("add", "/admin/webpages")]
        public async Task<IActionResult> CreateWebPage(CreateWebPageViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Normalize URL
                    var normalizedUrl = model.Url.Trim().ToLower();
                    if (!normalizedUrl.StartsWith("/"))
                        normalizedUrl = "/" + normalizedUrl;

                    // Check if URL already exists
                    var existingPage = await _context.WebPages
                        .FirstOrDefaultAsync(w => w.Url.ToLower() == normalizedUrl);

                    if (existingPage != null)
                    {
                        TempData["ErrorMessage"] = "A page with this URL already exists.";
                        return RedirectToAction("WebPages");
                    }

                    var webPage = new WebPage
                    {
                        PageName = model.PageName.Trim(),
                        Url = normalizedUrl
                    };

                    _context.WebPages.Add(webPage);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Web page created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating web page: {ex.Message}";
            }

            return RedirectToAction("WebPages");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("edit", "/admin/webpages")]
        public async Task<IActionResult> UpdateWebPage(EditWebPageViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var webPage = await _context.WebPages.FindAsync(model.PageId);
                    if (webPage == null)
                    {
                        TempData["ErrorMessage"] = "Web page not found.";
                        return RedirectToAction("WebPages");
                    }

                    // Normalize URL
                    var normalizedUrl = model.Url.Trim().ToLower();
                    if (!normalizedUrl.StartsWith("/"))
                        normalizedUrl = "/" + normalizedUrl;

                    // Check if URL already exists (excluding current page)
                    var existingPage = await _context.WebPages
                        .FirstOrDefaultAsync(w => w.Url.ToLower() == normalizedUrl && w.PageId != model.PageId);

                    if (existingPage != null)
                    {
                        TempData["ErrorMessage"] = "A page with this URL already exists.";
                        return RedirectToAction("WebPages");
                    }

                    webPage.PageName = model.PageName.Trim();
                    webPage.Url = normalizedUrl;

                    _context.WebPages.Update(webPage);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Web page updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating web page: {ex.Message}";
            }

            return RedirectToAction("WebPages");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("delete", "/admin/webpages")]
        public async Task<IActionResult> DeleteWebPage(int pageId)
        {
            try
            {
                var webPage = await _context.WebPages.FindAsync(pageId);
                if (webPage == null)
                {
                    TempData["ErrorMessage"] = "Web page not found.";
                    return RedirectToAction("WebPages");
                }

                // Check if page has any permissions assigned
                var hasPermissions = await _context.Permissions.AnyAsync(p => p.PageId == pageId);
                if (hasPermissions)
                {
                    TempData["ErrorMessage"] = "Cannot delete page. It has permissions assigned to employees.";
                    return RedirectToAction("WebPages");
                }

                _context.WebPages.Remove(webPage);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Web page deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting web page: {ex.Message}";
            }

            return RedirectToAction("WebPages");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("delete", "/admin/employees")]
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
        [RequirePermission("add", "/admin/employees")]
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
        [RequirePermission("edit", "/admin/employees")]
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
        [RequirePermission("view", "/admin/suppliers")]
        public async Task<IActionResult> Suppliers(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortBy = "CompanyName", string sortOrder = "asc")
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();

            // Check permissions for actions
            var employeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.CanAddSupplier = employeeId == "0" ? true : await _permissionService.HasPermissionAsync(employeeId, "/admin/suppliers", "add");
            ViewBag.CanEditSupplier = employeeId == "0" ? true : await _permissionService.HasPermissionAsync(employeeId, "/admin/suppliers", "edit");
            ViewBag.CanDeleteSupplier = employeeId == "0" ? true : await _permissionService.HasPermissionAsync(employeeId, "/admin/suppliers", "delete");

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                suppliers = suppliers.Where(s =>
                    s.CompanyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (s.ContactName != null && s.ContactName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            // Apply sorting
            suppliers = sortBy.ToLower() switch
            {
                "email" => sortOrder == "desc" ? suppliers.OrderByDescending(s => s.Email).ToList() : suppliers.OrderBy(s => s.Email).ToList(),
                "contactname" => sortOrder == "desc" ? suppliers.OrderByDescending(s => s.ContactName ?? "").ToList() : suppliers.OrderBy(s => s.ContactName ?? "").ToList(),
                "productcount" => sortOrder == "desc" ? suppliers.OrderByDescending(s => s.Products.Count).ToList() : suppliers.OrderBy(s => s.Products.Count).ToList(),
                _ => sortOrder == "desc" ? suppliers.OrderByDescending(s => s.CompanyName).ToList() : suppliers.OrderBy(s => s.CompanyName).ToList()
            };

            var totalCount = suppliers.Count();
            var pagedSuppliers = suppliers.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new SupplierManagementViewModel
            {
                Suppliers = pagedSuppliers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplier(string supplierId)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(supplierId);
                if (supplier == null)
                {
                    return NotFound();
                }

                return Json(new
                {
                    supplierId = supplier.SupplierId,
                    companyName = supplier.CompanyName,
                    email = supplier.Email,
                    contactName = supplier.ContactName,
                    phone = supplier.Phone,
                    address = supplier.Address,
                    description = supplier.Description,
                    logo = supplier.Logo
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("add", "/admin/suppliers")]
        public async Task<IActionResult> CreateSupplier(CreateSupplierViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var supplier = new Supplier
                    {
                        SupplierId = model.SupplierId.Trim(),
                        CompanyName = model.CompanyName.Trim(),
                        Email = model.Email.Trim().ToLower(),
                        ContactName = string.IsNullOrEmpty(model.ContactName) ? null : model.ContactName.Trim(),
                        Phone = string.IsNullOrEmpty(model.Phone) ? null : model.Phone.Trim(),
                        Address = string.IsNullOrEmpty(model.Address) ? null : model.Address.Trim(),
                        Description = string.IsNullOrEmpty(model.Description) ? null : model.Description.Trim(),
                        Logo = model.Logo,
                        Password = model.Password
                    };

                    var result = await _supplierService.CreateSupplierAsync(supplier);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Supplier created successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to create supplier. ID or email might already exist.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating supplier: {ex.Message}";
            }

            return RedirectToAction("Suppliers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("edit", "/admin/suppliers")]
        public async Task<IActionResult> UpdateSupplier(EditSupplierViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var supplier = await _supplierService.GetSupplierByIdAsync(model.SupplierId);
                    if (supplier == null)
                    {
                        TempData["ErrorMessage"] = "Supplier not found.";
                        return RedirectToAction("Suppliers");
                    }

                    supplier.CompanyName = model.CompanyName.Trim();
                    supplier.Email = model.Email.Trim().ToLower();
                    supplier.ContactName = string.IsNullOrEmpty(model.ContactName) ? null : model.ContactName.Trim();
                    supplier.Phone = string.IsNullOrEmpty(model.Phone) ? null : model.Phone.Trim();
                    supplier.Address = string.IsNullOrEmpty(model.Address) ? null : model.Address.Trim();
                    supplier.Description = string.IsNullOrEmpty(model.Description) ? null : model.Description.Trim();
                    supplier.Logo = model.Logo;

                    var result = await _supplierService.UpdateSupplierAsync(supplier);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Supplier updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update supplier. Email might already exist.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating supplier: {ex.Message}";
            }

            return RedirectToAction("Suppliers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("delete", "/admin/suppliers")]
        public async Task<IActionResult> DeleteSupplier(string supplierId)
        {
            try
            {
                var result = await _supplierService.DeleteSupplierAsync(supplierId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Supplier deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete supplier. It might have products assigned.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting supplier: {ex.Message}";
            }

            return RedirectToAction("Suppliers");
        }
        #region Order Management
        [RequirePermission("view", "/admin/orders")]
        public async Task<IActionResult> Orders(OrderManagementViewModel model)
		{
			// 1. Lấy dữ liệu gốc
			var orders = await _orderService.GetAllOrdersAsync();

			// 2. Search
			if (!string.IsNullOrWhiteSpace(model.SearchQuery))
			{
				var kw = model.SearchQuery.ToLower();
				orders = orders.Where(o =>
					o.OrderId.ToString().Contains(kw) ||
					(o.Customer != null && (
						(!string.IsNullOrEmpty(o.Customer.FullName) && o.Customer.FullName.ToLower().Contains(kw)) ||
						(!string.IsNullOrEmpty(o.Customer.Email) && o.Customer.Email.ToLower().Contains(kw))
					)) ||
					(!string.IsNullOrEmpty(o.Phone) && o.Phone.ToLower().Contains(kw))
				);
			}

			// 3. Sort
			orders = model.SortBy?.ToLower() switch
			{
				"customer" => model.SortOrder == "desc"
					? orders.OrderByDescending(o => o.Customer.FullName)
					: orders.OrderBy(o => o.Customer.FullName),

				"freight" => model.SortOrder == "desc"
					? orders.OrderByDescending(o => o.Freight)
					: orders.OrderBy(o => o.Freight),

				"date" => model.SortOrder == "desc"
					? orders.OrderByDescending(o => o.OrderDate)
					: orders.OrderBy(o => o.OrderDate),

				_ => model.SortOrder == "desc"
					? orders.OrderByDescending(o => o.OrderId)
					: orders.OrderBy(o => o.OrderId)
			};

			// 4. Paging + đổ ra ViewModel
			model.TotalItems = orders.Count();

			model.Orders = orders
				.Skip((model.CurrentPage - 1) * model.PageSize)
				.Take(model.PageSize)
				.ToList();

			return View(model);
		}

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound();

            if (order.StatusId == 1) // Pending → Processing
            {
                order.StatusId = 2;
            }
            else if (order.StatusId == 2) // Processing → Completed
            {
                order.StatusId = 3;
            }
            else
            {
                // Already Completed → Do nothing
                TempData["Message"] = "Order is already completed and cannot be changed.";
                return RedirectToAction("Orders");
            }

            await _orderService.UpdateOrderAsync(order);
            TempData["Message"] = "Order status updated successfully.";
            return RedirectToAction("Orders");
        }


        #endregion

        #region Product Management


        public IActionResult Products(ProductManagementViewModel model)
		{
			var products = _productService.GetAll();

			// ✅ Filter theo Category
			if (model.CategoryId.HasValue)
			{
				products = products.Where(p => p.CategoryId == model.CategoryId.Value).ToList();
			}

			// ✅ Filter theo Supplier
			if (!string.IsNullOrEmpty(model.SupplierId))
			{
				products = products.Where(p => p.SupplierId == model.SupplierId).ToList();
			}

			// 🔍 Keyword search
			if (!string.IsNullOrEmpty(model.Keyword))
			{
				var keyword = model.Keyword.ToLower();
				products = products.Where(p =>
					p.ProductId.ToString().Contains(keyword) ||
					(!string.IsNullOrEmpty(p.ProductName) && p.ProductName.ToLower().Contains(keyword))
				).ToList();
			}

			// 🔃 Sort
			products = (model.SortBy?.ToLower()) switch
			{
				"name" => model.SortOrder == "desc"
					? products.OrderByDescending(p => p.ProductName).ToList()
					: products.OrderBy(p => p.ProductName).ToList(),

				"price" => model.SortOrder == "desc"
					? products.OrderByDescending(p => p.UnitPrice).ToList()
					: products.OrderBy(p => p.UnitPrice).ToList(),

				_ => model.SortOrder == "desc"
					? products.OrderByDescending(p => p.ProductId).ToList()
					: products.OrderBy(p => p.ProductId).ToList()
			};

			// 📄 Pagination
			model.TotalCount = products.Count();
			model.Products = products
				.Skip((model.PageNumber - 1) * model.PageSize)
				.Take(model.PageSize)
				.ToList();

			// 📋 Load categories and suppliers for dropdown
			model.Categories = _productService.GetCategories();
			model.Suppliers = _productService.GetSuppliers();

			return View(model);
		}




        #endregion
        [RequirePermission("view", "/admin/categories")]
        public async Task<IActionResult> Categories(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortBy = "CategoryName", string sortOrder = "asc")
        {
            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();

            // Check permissions for actions
            var employeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.CanAddCategory = employeeId == "0" ? true : await _permissionService.HasPermissionAsync(employeeId, "/admin/categories", "add");
            ViewBag.CanEditCategory = employeeId == "0" ? true : await _permissionService.HasPermissionAsync(employeeId, "/admin/categories", "edit");
            ViewBag.CanDeleteCategory = employeeId == "0" ? true : await _permissionService.HasPermissionAsync(employeeId, "/admin/categories", "delete");

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                categories = categories.Where(c =>
                    c.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply sorting
            categories = sortBy.ToLower() switch
            {
                "productcount" => sortOrder == "desc" ? categories.OrderByDescending(c => c.Products.Count).ToList() : categories.OrderBy(c => c.Products.Count).ToList(),
                "categoryid" => sortOrder == "desc" ? categories.OrderByDescending(c => c.CategoryId).ToList() : categories.OrderBy(c => c.CategoryId).ToList(),
                _ => sortOrder == "desc" ? categories.OrderByDescending(c => c.CategoryName).ToList() : categories.OrderBy(c => c.CategoryName).ToList()
            };

            var totalCount = categories.Count();
            var pagedCategories = categories.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new CategoryManagementViewModel
            {
                Categories = pagedCategories,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("add", "/admin/categories")]
        public async Task<IActionResult> CreateCategory(CreateCategoryViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if category name already exists
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == model.CategoryName.ToLower());

                    if (existingCategory != null)
                    {
                        TempData["ErrorMessage"] = "A category with this name already exists.";
                        return RedirectToAction("Categories");
                    }

                    var category = new Category
                    {
                        CategoryName = model.CategoryName.Trim(),
                        Alias = string.IsNullOrEmpty(model.Alias) ? null : model.Alias.Trim(),
                        Description = string.IsNullOrEmpty(model.Description) ? null : model.Description.Trim(),
                        Image = string.IsNullOrEmpty(model.Image) ? null : model.Image.Trim()
                    };

                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Category created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating category: {ex.Message}";
            }

            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("edit", "/admin/categories")]
        public async Task<IActionResult> UpdateCategory(EditCategoryViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var category = await _context.Categories.FindAsync(model.CategoryId);
                    if (category == null)
                    {
                        TempData["ErrorMessage"] = "Category not found.";
                        return RedirectToAction("Categories");
                    }

                    // Check if category name already exists (excluding current category)
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == model.CategoryName.ToLower() && c.CategoryId != model.CategoryId);

                    if (existingCategory != null)
                    {
                        TempData["ErrorMessage"] = "A category with this name already exists.";
                        return RedirectToAction("Categories");
                    }

                    category.CategoryName = model.CategoryName.Trim();
                    category.Alias = string.IsNullOrEmpty(model.Alias) ? null : model.Alias.Trim();
                    category.Description = string.IsNullOrEmpty(model.Description) ? null : model.Description.Trim();
                    category.Image = string.IsNullOrEmpty(model.Image) ? null : model.Image.Trim();

                    _context.Categories.Update(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Category updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating category: {ex.Message}";
            }

            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("delete", "/admin/categories")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CategoryId == categoryId);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction("Categories");
                }

                // Check if category has products
                if (category.Products.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete category. It has products assigned to it.";
                    return RedirectToAction("Categories");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting category: {ex.Message}";
            }

            return RedirectToAction("Categories");
        }
    }
}
