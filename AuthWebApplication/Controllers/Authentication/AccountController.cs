using AuthWebApp.DataAccess.Repository.IRepository;
using AuthWebApp.Models;
using AuthWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApplication.Controllers.Authentication
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public AccountController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            ViewData["ActiveTab"] = "Register";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            ResponseVM responseVM = new ResponseVM();

            try
            {
                if (ModelState.IsValid)
                {
                    var checkEmail = await _userManager.FindByEmailAsync(registerVM.Email);

                    if (checkEmail != null)
                    {
                        ModelState.AddModelError("", "Email already exist.");
                        ViewData["ActiveTab"] = "Register";
                        return View(registerVM);
                    }

                    var isUsernameExist = _unitOfWork.ApplicationUser.Get(u => u.UserName == registerVM.Username);

                    if (isUsernameExist != null)
                    {
                        ModelState.AddModelError("", "Username not available.");
                        ViewData["ActiveTab"] = "Register";
                        return View(registerVM);
                    }

                    var user = new ApplicationUser
                    {
                        UserName = registerVM.Username,
                        Email = registerVM.Email,
                        FirstName = registerVM.FirstName,
                        LastName = registerVM.LastName,
                    };

                    var result = await _userManager.CreateAsync(user, registerVM.Password);

                    if (result.Succeeded)
                    {
                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("ConfirmMail", "Account", new { UserId = userId, Token = code }, protocol: Request.Scheme);

                        var userFullName = $"{registerVM.FirstName} {registerVM.LastName}";

                        string emailBody = GetEmailBody(userFullName, "Email Confirmation", confirmationLink, "EmailConfirmation");
                        bool status = await _emailSender.EamilSendAsynce(registerVM.Email, "Email Confirmation", emailBody);

                        if (status)
                        {
                            responseVM.Message = "Please check your email for the verification.";
                            return RedirectToAction("Confirmation", "Account", responseVM);
                        }

                        //await _signInManager.SignInAsync(user, isPersistent: false);
                        //return RedirectToAction("Index", "Home");
                        return RedirectToAction("Login", "Account");
                    }

                    if (result.Errors.Count() > 0)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            ViewData["ActiveTab"] = "Register";
            return View(registerVM);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmMail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account"); // Handle invalid links
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Handle user not found
            }

            if (user.EmailConfirmed)
            {
                return RedirectToAction("Confirmation", "Account", new ResponseVM
                {
                    Message = "Your email is already verified."
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return RedirectToAction("Confirmation", "Account", new ResponseVM
                {
                    Message = "Thank you for confirming your E-mail."
                });
            }

            return RedirectToAction("Confirmation", "Account", new ResponseVM
            {
                Message = "The confirmation link is invalid or has expired."
            });
        }


        public string GetEmailBody(string? name, string? title, string? callbackUrl, string? EmailTemplateName)
        {
            string url = _configuration.GetValue<string>("Urls:LoginUrl") ?? string.Empty;

            string path = Path.Combine(_webHostEnvironment.WebRootPath, "Template/" + EmailTemplateName + ".cshtml");
            string htmlString = System.IO.File.ReadAllText(path);
            htmlString = htmlString.Replace("{{title}}", title);
            htmlString = htmlString.Replace("{{name}}", name);
            htmlString = htmlString.Replace("{{url}}", url);
            htmlString = htmlString.Replace("{{callbackUrl}}", callbackUrl);
            return htmlString;
        }

        public IActionResult Login()
        {
            ViewData["ActiveTab"] = "Login";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var checkEmail = await _userManager.FindByEmailAsync(loginVM.Email);

                    if (checkEmail == null)
                    {
                        ModelState.AddModelError("", "Email not found.");
                        ViewData["ActiveTab"] = "Login";
                        return View(loginVM);
                    }

                    if (await _userManager.CheckPasswordAsync(checkEmail, loginVM.Password) == false)
                    {
                        ModelState.AddModelError("", "Incorrect Password");
                        ViewData["ActiveTab"] = "Login";
                        return View(loginVM);
                    }

                    bool confirmStatus = await _userManager.IsEmailConfirmedAsync(checkEmail);

                    if (!confirmStatus)
                    {
                        ModelState.AddModelError("", "Email not confirmed.");
                        ViewData["ActiveTab"] = "Login";
                        return View(loginVM);
                    }
                    else
                    {
                        var result = await _signInManager.PasswordSignInAsync(checkEmail.UserName, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            return RedirectToAction("Index", "Home");
                        }

                        ModelState.AddModelError("", "Invalid login attempt.");
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            ViewData["ActiveTab"] = "Login";
            return View(loginVM);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Confirmation(ResponseVM responseVM)
        {
            return View(responseVM);
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswrodVM)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("Token");

            if (!ModelState.IsValid)
            {
                return View(forgetPasswrodVM);
            }

            var user = await _userManager.FindByEmailAsync(forgetPasswrodVM.Email);

            if (user != null)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, Token = code }, protocol: Request.Scheme);

                var userFullName = $"{user.FirstName} {user.LastName}";

                string emailBody = GetEmailBody(userFullName, "Reset Password", callbackUrl, "ResetPassword");

                bool isSendEmail = await _emailSender.EamilSendAsynce(forgetPasswrodVM.Email, "Reset Password", emailBody);


                if (isSendEmail)
                {
                    ResponseVM responseVM = new ResponseVM();
                    responseVM.Message = "Reset Password Link";
                    return RedirectToAction("Confirmation", "Account", responseVM);
                }
            }

            return View();
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            // Check if the parameters are null or empty
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                // Redirect to the Login action
                return RedirectToAction("Login", "Account");
            }

            var forgetPasswrodVM = new ForgetPasswordVM
            {
                UserId = userId,
                Token = token
            };

            return View(forgetPasswrodVM);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            ResponseVM responseVM = new ResponseVM();

            ModelState.Remove("Email");

            if (!ModelState.IsValid)
            {
                responseVM.Message = "Something went wrong! Please try again.";
                return RedirectToAction("Confirmation", responseVM);
            }

            var user = await _userManager.FindByIdAsync(forgetPasswordVM.UserId);

            if (user == null)
            {
                return View(forgetPasswordVM);
            }

            var result = await _userManager.ResetPasswordAsync(user, forgetPasswordVM.Token, forgetPasswordVM.Password);

            if (result.Succeeded)
            {
                responseVM.Message = "Your password has been successfully reset.";
                return RedirectToAction("Confirmation", responseVM);
            }

            return View(forgetPasswordVM);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = _userManager.GetUserId(User); // Get the currently logged-in user's ID
            var user = await _userManager.FindByIdAsync(userId); // Fetch the user from the database

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Map the ApplicationUser properties to a new ApplicationUser instance or a ViewModel
            var model = new ApplicationUser
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ImageUrl = user.ImageUrl
            };

            return View(model); // Pass the model to the view
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(ApplicationUser applicationUser, IFormFile? ProfileImage, string OldPassword, string NewPassword, string ConfirmPassword)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update user profile fields
            user.FirstName = applicationUser.FirstName;
            user.LastName = applicationUser.LastName;
            user.Gender = applicationUser.Gender;
            user.BirthDate = applicationUser.BirthDate;
            user.ModifiedOn = DateTime.Now;

            // Handle profile image upload
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Save the file (this example assumes a `wwwroot/images/profiles` directory)
                var filePath = Path.Combine("wwwroot/images/profiles", ProfileImage.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }
                user.ImageUrl = $"/images/profiles/{ProfileImage.FileName}";
            }

            // Handle password change
            if (!string.IsNullOrEmpty(OldPassword) && !string.IsNullOrEmpty(NewPassword))
            {
                if (NewPassword != ConfirmPassword)
                {
                    ModelState.AddModelError("", "New password and confirm password do not match.");
                    return View(applicationUser);
                }

                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);
                if (!passwordChangeResult.Succeeded)
                {
                    foreach (var error in passwordChangeResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(applicationUser);
                }
            }

            // Update the user in the database
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(applicationUser);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("Index", "Home");
        }

    }
}
