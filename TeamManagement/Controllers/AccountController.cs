using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using TeamManagement.Services;
using TeamManagement.Models;
using System.Security.Claims;
namespace TeamManagement.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Action method for login.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(model: new LoginViewModel());
        }
        /// <summary>
        /// Action method for login.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                DataAccessService _dataService = new DataAccessService();
                try
                {
                    User _user = await _dataService.FindUserByUserNameAsync(model.UserName);
                    if (_user != null && _user.Password == model.Password && _user.UserName == model.UserName)
                    {
                        List<Role> _roles = await _dataService.GetUserRolesAsync(_user.UserId);
                        List<Claim> _claims = new List<Claim>();
                        foreach (Role role in _roles)
                        {
                            if (role.RoleName == "TeamAdministrator")
                            {
                                _claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
                            }
                        }
                        _claims.Add(new Claim("UserId", _user.UserId.ToString()));
                        _claims.Add(new Claim("MemberType", _user.MemberType.ToString()));
                        _claims.Add(new Claim(ClaimTypes.UserData, _user.UserName));
                        _claims.Add(new Claim(ClaimTypes.Name, $"{_user.FirstName} {_user.LastName}", "Bearer"));

                        await this.HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(_claims, "Cookies", "user", "role")));
                        return this.RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        this.ModelState.AddModelError(string.Empty, "Invalid Credentials");
                        return this.View(model);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    this.ModelState.AddModelError(string.Empty, "Invalid Credentials");
                    return this.View(model);
                }
            }
            else
            {
                return this.View(model);
            }
        }


        /// <summary>
        /// Action method for logout.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync("Bearer");
            return this.RedirectToAction("Login", "Account");
        }
  
  }





}






       
