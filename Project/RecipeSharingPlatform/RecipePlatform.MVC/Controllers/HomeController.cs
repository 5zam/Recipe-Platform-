using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RecipePlatform.BLL.DTOs;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.MVC.Models;
using RecipePlatform.MVC.Models.ViewModels;

namespace RecipePlatform.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRecipeService _recipeService;

        public HomeController(ILogger<HomeController> logger, IRecipeService recipeService)
        {
            _logger = logger;
            _recipeService = recipeService;
        }

        public async Task<IActionResult> Index()
        {
            var latestRecipes = await _recipeService.GetLatestRecipes(3);
            var topRatedRecipes = await _recipeService.GetTopRatedRecipes(3);

            var viewModel = new HomeViewModel
            {
                LatestRecipes = latestRecipes,
                TopRatedRecipes = topRatedRecipes
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string q, int page = 1, int pageSize = 12)
        {
            SearchResultsDto searchDto;

            if (string.IsNullOrWhiteSpace(q))
            {
                searchDto = await _recipeService.GetAllRecipesWithDetails(page, pageSize);
                ViewBag.SearchQuery = "";
            }
            else
            {
                searchDto = await _recipeService.SearchRecipes(q.Trim(), page, pageSize);
                ViewBag.SearchQuery = q;
            }

            // تحويل DTO إلى ViewModel
            var viewModel = new SearchResultsViewModel
            {
                Recipes = searchDto.Recipes,
                SearchQuery = searchDto.SearchQuery,
                CurrentPage = searchDto.CurrentPage,
                TotalPages = searchDto.TotalPages,
                TotalRecipes = searchDto.TotalRecipes
            };

            return View("SearchResults", viewModel);
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
    }
}