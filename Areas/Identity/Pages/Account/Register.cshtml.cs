using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NguyenNgocThien_Buoi4.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace NguyenNgocThien_BanHang.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập tuổi")]
            [Range(18, 100, ErrorMessage = "Tuổi phải từ 18 đến 100")]
            [Display(Name = "Tuổi")]
            public int Age { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
            [Display(Name = "Địa chỉ")]
            public string Address { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Vai trò")]
            public string Role { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự và tối đa {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
            public string ConfirmPassword { get; set; }

            public List<SelectListItem> RoleList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Tạo danh sách vai trò
            Input = new InputModel
            {
                RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList()
            };

            // Nếu không có vai trò nào, thêm vai trò mặc định
            if (Input.RoleList == null || !Input.RoleList.Any())
            {
                Input.RoleList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Customer", Value = "Customer" }
                };
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Attempting to create a new user with email: {Email}", Input.Email);

                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.FullName,
                    Age = Input.Age,
                    Address = Input.Address,
                    PhoneNumber = Input.PhoneNumber,
                    EmailConfirmed = true // Tự động xác nhận email
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} created successfully", Input.Email);

                    // Mặc định gán vai trò Customer
                    string roleName = "Customer";

                    // Kiểm tra xem vai trò đã tồn tại chưa
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                    {
                        _logger.LogInformation("Creating role {Role}", roleName);
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }

                    // Thêm vai trò cho người dùng
                    _logger.LogInformation("Adding user {Email} to role {Role}", Input.Email, roleName);
                    await _userManager.AddToRoleAsync(user, roleName);

                    // Đăng nhập người dùng sau khi đăng ký
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User {Email} signed in after registration", Input.Email);

                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogError("Error creating user {Email}: {Error}", Input.Email, error.Description);
                }
            }
            else
            {
                // Log các lỗi validation
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("Validation error for user {Email}: {Error}", Input.Email, error.ErrorMessage);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
