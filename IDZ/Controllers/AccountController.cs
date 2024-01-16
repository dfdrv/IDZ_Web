using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IDZ.Models.Entities;
using System.Net;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using IDZ.Models.ViewModels;
using System.Security.Cryptography;
using System.Web.Security;
using System.Diagnostics;
using System.Text;

namespace IDZ.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(Users webUser)
        {
            if (ModelState.IsValid)
            {
                using (Entities context = new Entities())
                {
                    Users user = null;
                    user = context.Users.Where(u => u.username == webUser.username).FirstOrDefault();
                    if (user != null)
                    {
                        string passwordHash = HashPassword(webUser.password + user.Salt.ToString().ToUpper());

                        Debug.WriteLine($"Input Password: {webUser.password}");
                        Debug.WriteLine($"Salt: {user.Salt}");
                        Debug.WriteLine($"Calculated Hash: {passwordHash}");
                        Debug.WriteLine($"Database Hash: {user.passwordhash}");

                        if (webUser.password== user.password)
                        {
                            string userRole = "";
                            switch (user.role)
                            {
                                case "user":
                                    userRole = "user";
                                    break;
                                case "moderator":
                                    userRole = "moderator";
                                    break;
                                    // Добавьте другие роли, если необходимо
                            }

                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                                1,
                                user.username,
                                DateTime.Now,
                                DateTime.Now.AddDays(1),
                                false,
                                userRole);

                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket));
                            return RedirectToAction("WordsList", "Lab");
                        }
                    }
                }
            }
            ViewBag.Error = "Пользователь не найден. Попробуйте еще раз";
            return View(webUser);
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("WordsList", "Lab");
        }

        string HashPassword(string loginAndSalt)
        {
            string hash = "";
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] data = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(loginAndSalt));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                hash = sBuilder.ToString().ToUpper();
            }
            return hash;
        }
    }
}
