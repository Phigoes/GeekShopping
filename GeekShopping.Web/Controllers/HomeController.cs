using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GeekShopping.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IProductService productService, ICartService cartService, ILogger<HomeController> logger)
        {
            _productService = productService;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.FindAllProducts("");
            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> Details(long id)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var products = await _productService.FindProductById(id, token);
            return View(products);
        }

        [Authorize]
        [ActionName("Details")]
        [HttpPost]
        public async Task<IActionResult> DetailsPost(ProductViewModel productViewModel)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var cart = new CartViewModel()
            {
                CartHeader = new CartHeaderViewModel()
                {
                    UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value
                }
            };

            var cartDetailViewModel = new CartDetailViewModel()
            {
                Count = productViewModel.Count,
                ProductId = productViewModel.Id,
                Product = await _productService.FindProductById(productViewModel.Id, token)
            };

            var cartDetailViewModelList = new List<CartDetailViewModel>
            {
                cartDetailViewModel
            };

            cart.CartDetails = cartDetailViewModelList;

            var response = await _cartService.AddItemToCart(cart, token);
            if (response != null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(productViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Login()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}