using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Castle.Core.Internal;
using System.Web.Helpers;
using System.Web.WebPages;
using BigLab.Entities;
using BigLab.Models;
using Newtonsoft.Json;

using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.Internal;


namespace BigLab.Controllers
{
    public class HomeController : Controller
    {
        public Context context = new Context();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        [HttpPost]
        public String GetProducts()
        {
            UserInfo user = context.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                user = new UserInfo();
                user.isAdmin = false;
            }
            IndexModel model = new IndexModel()
            {
                User = user,
                Games = context.Games.ToList()
            };
            
            
            var json = JsonConvert.SerializeObject(model, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return json;
        }
        
        [HttpPost]
        public async Task Buy(string Id)
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo user = context.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                var gameIndex = 0;
                if (int.TryParse(Id, out gameIndex))
                {
                    IQueryable<Game> findGames = from contextGame in context.Games
                        where (gameIndex == contextGame.Id)
                        select contextGame;
                    Game game = context.Games.SingleOrDefault(g => g.Id == gameIndex);

                    Order order = new Order()
                    {
                        UserGame = game,
                        UserInfo = user
                    };
                    await context.Orders.AddAsync(order);
                    await context.SaveChangesAsync();
                }
            }
        }
        
        [HttpPost]
        public async Task Reject(string Id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var gameIndex = 0;
                if (int.TryParse(Id, out gameIndex))
                {
                    IQueryable<Order> findOrders = from contextGame in context.Orders
                        where (gameIndex == contextGame.Id)
                        select contextGame;

                    if (findOrders.Any())
                    {
                        try
                        {
                            foreach (var line in findOrders)
                            {
                                context.Remove(line);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Ошибка при удалении");
                            throw;
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }
        
        [HttpPost]
        public async Task<string> GetProfileProducts()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return "bad";
            }
            List<UserInfo> usersList = new List<UserInfo>();
            List<Order> ordersList = new List<Order>();
            UserInfo user = context.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            IQueryable<Order> orders = from order in context.Orders
                where (order.UserInfo.Id == user.Id)
                select order;
            if (orders.Any())
            {
                ordersList = orders.ToList();
            }
            if (user == null)
            {
                user = new UserInfo();
                user.isAdmin = false;
            }
            else
            {
                if (user.isAdmin)
                {
                    IQueryable<UserInfo> users = from contextUsers in context.Users
                        where (user.Username != contextUsers.Username && user.Id != contextUsers.Id)
                        select contextUsers;
                    usersList = users.ToList();
                }
            }
            await context.SaveChangesAsync();
            UserPageModel model = new UserPageModel()
            {
                User = user,
                Users = usersList,
                Orders = ordersList
            };
            
            var json = JsonConvert.SerializeObject(model, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return json;
        }
        
        public IActionResult Index()
        {
            UserInfo user = context.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                user = new UserInfo();
                user.isAdmin = false;
            }
            IndexModel model = new IndexModel()
            {
                User = user,
                Games = context.Games.ToList()
            };
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Index(string gameName, 
                                                string gameGenre, 
                                                string gameDescription, 
                                                string gamePrice,
                                                int deleteGameID,
                                                string buttonType)
        {
            UserInfo user = context.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            IndexModel model = new IndexModel();
            if (User.Identity.IsAuthenticated && user.isAdmin)
            {
                var price = 0;
                int.TryParse(gamePrice, out price);
                if (buttonType == "Добавить")
                {
                    Game game = new Game()
                    {
                        Name = gameName,
                        Genre = gameGenre,
                        Description = gameDescription,
                        Price = price
                    };
                    await context.Games.AddAsync(game);
                    await context.SaveChangesAsync();
                    model = new IndexModel()
                    {
                        User = user,
                        Games = context.Games.ToList()
                    };
                }

                if (buttonType == "Удалить")
                {
                    IQueryable<Game> findGames = from contextGame in context.Games
                        where (deleteGameID == contextGame.Id)
                        select contextGame;
                    
                    IQueryable<Order> findOrderGames = from contextGame in context.Orders
                        where (deleteGameID == contextGame.UserGame.Id)
                        select contextGame;
                    
                    if (findGames.Any())
                    {
                        try
                        {
                            foreach (var line in findGames)
                            {
                                context.Remove(line);
                            }
                        }
                        catch (Exception e)
                        {
                            ViewBag.message = "<script>alert('Ошибка при удалении')</script>";
                            Console.WriteLine("Ошибка при удалении");
                            throw;
                        }
                    }
                    
                    if (findOrderGames.Any())
                    {
                        try
                        {
                            foreach (var line in findOrderGames)
                            {
                                context.Remove(line);
                            }
                        }
                        catch (Exception e)
                        {
                            ViewBag.message = "<script>alert('Ошибка при удалении')</script>";
                            Console.WriteLine("Ошибка при удалении");
                            throw;
                        }
                    }

                    await context.SaveChangesAsync();
                    model = new IndexModel()
                    {
                        User = user,
                        Games = context.Games.ToList()
                    };
                }
            }

            if (buttonType.Contains("Купить"))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Redirect("/Home/Login");
                }
                var index = buttonType.Split(" ").Last();
                var gameIndex = 0;
                if (int.TryParse(index, out gameIndex))
                {
                    IQueryable<Game> findGames = from contextGame in context.Games
                        where (gameIndex == contextGame.Id)
                        select contextGame;
                    Game game = context.Games.SingleOrDefault(g => g.Id == gameIndex);

                    Order order = new Order()
                    {
                        UserGame = game,
                        UserInfo = user
                    };
                    await context.Orders.AddAsync(order);
                    await context.SaveChangesAsync();
                    return Redirect("/Home/UserPage");
                }
            }
            return View(model);
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            if (!User.Identity.IsAuthenticated)
            {
                LoginModel empty = new LoginModel();
                return View(empty);
            }
            else
            {
                return Redirect("/Home/UserPage");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Register(string login, string pswd, bool isJson = false)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                LoginModel model = new LoginModel()
                {
                    Password = pswd,
                    ErrorMessage = "Имя пользователя не указано!"
                };
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(pswd))
            {
                LoginModel model = new LoginModel()
                {
                    Login = login,
                    ErrorMessage = "Пароль не корректный!"
                };
                return View(model);
            }
            UserInfo usr = context.Users.FirstOrDefault(u => u.Username == login);
            if (usr != null)
            {
                LoginModel model = new LoginModel()
                {
                    Password = pswd,
                    ErrorMessage = "Данное имя пользователя уже используется"
                };
                return View(model);
            }
            else
            {
                UserInfo user = new UserInfo()
                {
                    Username = login,
                    PasswordHash = Crypto.HashPassword(pswd),
                    isAdmin = false
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                return Redirect("/Home/Index");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (!User.Identity.IsAuthenticated)
            {
                LoginModel empty = new LoginModel();
                return View(empty);
            }
            else
            {
                return Redirect("/Home/UserPage");
            }
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
                };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public JsonResult LoginModelToJson(LoginModel model)
        {
            var json = JsonConvert.SerializeObject(model, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return Json(json);
        }
        
        public JsonResult StringToJson(String model)
        {
            var json = JsonConvert.SerializeObject(model, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return Json(json);
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string pswd, string typeJson = "false")
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                LoginModel model = new LoginModel()
                {
                    Password = pswd,
                    ErrorMessage = "Имя пользователя не указано!"
                };
                if (typeJson == "true")
                {
                    return StringToJson("bad");
                }
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(pswd))
            {
                LoginModel model = new LoginModel()
                {
                    Login = login,
                    ErrorMessage = "Пароль не корректный!"
                };
                if (typeJson == "true")
                {
                    return StringToJson("bad");
                }
                return View(model);
            }
            UserInfo usr = context.Users.FirstOrDefault(u => u.Username == login);
            if (usr == null)
            {
                LoginModel model = new LoginModel()
                {
                    ErrorMessage = "Пользователя не существует!"
                };
                if (typeJson == "true")
                {
                    return StringToJson("bad");
                }
                return View(model);
            }
            else if (!Crypto.VerifyHashedPassword(usr.PasswordHash,pswd))
            {
                LoginModel model = new LoginModel()
                {
                    ErrorMessage = "Неверный пароль!"
                };
                if (typeJson == "true")
                {
                    return StringToJson("bad");
                }
                return View(model);
            }
            else
            {
                await Authenticate(login);
                if (typeJson == "true")
                {
                    return StringToJson("good");
                }
                return Redirect("/Home/UserPage");
            }
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/Home/Login");
        }
        
        [Authorize]
        public async Task<IActionResult> UserPage()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/Login");
            }
            List<UserInfo> usersList = new List<UserInfo>();
            List<Order> ordersList = new List<Order>();
            UserInfo user = context.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            IQueryable<Order> orders = from order in context.Orders
                where (order.UserInfo.Id == user.Id)
                select order;
            if (orders.Any())
            {
                ordersList = orders.ToList();
            }
            if (user == null)
            {
                user = new UserInfo();
                user.isAdmin = false;
            }
            else
            {
                if (user.isAdmin)
                {
                    IQueryable<UserInfo> users = from contextUsers in context.Users
                        where (user.Username != contextUsers.Username && user.Id != contextUsers.Id)
                        select contextUsers;
                    usersList = users.ToList();
                }
            }
            await context.SaveChangesAsync();
            UserPageModel model = new UserPageModel()
            {
                User = user,
                Users = usersList,
                Orders = ordersList
            };
            return View(model);
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UserPage(int userID, string userIsAdmin, string buttonType)
        {
            List<UserInfo> usersList = new List<UserInfo>();
            List<Order> ordersList = new List<Order>();
            UserInfo user = context.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            IQueryable<Order> orders = from order in context.Orders
                where (order.UserInfo.Id == user.Id)
                select order;
            if (orders.Any())
            {
                ordersList = orders.ToList();
            }
            if (user == null)
            {
                user = new UserInfo();
                user.isAdmin = false;
            }
            else
            {
                if (user.isAdmin)
                {
                    IQueryable<UserInfo> users = from contextUsers in context.Users
                        where (user.Username != contextUsers.Username && user.Id != contextUsers.Id)
                        select contextUsers;
                    usersList = users.ToList();
                }
            }

            if (buttonType == "Установить")
            {
                IQueryable<UserInfo> findUsers = from contextUsers in context.Users
                    where (userID == contextUsers.Id && userID != user.Id) 
                    select contextUsers;
                var isAdmin = false;
                bool.TryParse(userIsAdmin, out isAdmin);
                if (findUsers.Any())
                {
                    try
                    {
                        foreach (var line in findUsers)
                        {
                            line.isAdmin = isAdmin;
                        }
                    }
                    catch (Exception e)
                    {
                        ViewBag.message = "<script>alert('Ошибка при изменении')</script>";
                        Console.WriteLine("Ошибка при изменении");
                        throw;
                    }
                }
            }
            if (buttonType.Contains("Убрать"))
            {
                var index = buttonType.Split(" ").Last();
                var gameIndex = 0;
                if (int.TryParse(index, out gameIndex))
                {
                    IQueryable<Order> findOrders = from contextGame in context.Orders
                        where (gameIndex == contextGame.Id)
                        select contextGame;
                    
                    if (findOrders.Any())
                    {
                        try
                        {
                            foreach (var line in findOrders)
                            {
                                context.Remove(line);
                            }
                        }
                        catch (Exception e)
                        {
                            ViewBag.message = "<script>alert('Ошибка при удалении')</script>";
                            Console.WriteLine("Ошибка при удалении");
                            throw;
                        }
                        
                    }
                }
            }
            await context.SaveChangesAsync();
            IQueryable<Order> newOrders = from order in context.Orders
                where (order.UserInfo.Id == user.Id)
                select order;
            if (newOrders.Any())
            {
                ordersList = newOrders.ToList();
            }
            UserPageModel model = new UserPageModel()
            {
                User = user,
                Users = usersList,
                Orders = ordersList
            };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}