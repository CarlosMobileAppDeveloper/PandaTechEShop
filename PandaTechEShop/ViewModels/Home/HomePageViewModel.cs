﻿using System;
using System.Threading.Tasks;
using PandaTechEShop.Models.Category;
using PandaTechEShop.Models.Product;
using PandaTechEShop.Services.Category;
using PandaTechEShop.Services.Preferences;
using PandaTechEShop.Services.Product;
using PandaTechEShop.Services.ShoppingCart;
using PandaTechEShop.ViewModels.Base;
using Prism.Navigation;
using Rg.Plugins.Popup.Contracts;
using Xamarin.CommunityToolkit.ObjectModel;

namespace PandaTechEShop.ViewModels.Home
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly IPreferences _preferences;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IShoppingCartService _shoppingCartService;

        public HomePageViewModel(
            INavigationService navigationService,
            IPopupNavigation popupNavigation,
            IPreferences preferences,
            IProductService productService,
            ICategoryService categoryService,
            IShoppingCartService shoppingCartService)
            : base(navigationService, popupNavigation)
        {
            Title = "Panda eShop";

            _preferences = preferences;
            _productService = productService;
            _categoryService = categoryService;
            _shoppingCartService = shoppingCartService;

            TrendingProducts = new ObservableRangeCollection<TrendingProduct>();
            Categories = new ObservableRangeCollection<CategoryInfo>();

            ViewProductForCategoryCommand = new AsyncCommand(ExecuteViewProductForCategoryCommandAsync, allowsMultipleExecutions: false);
        }

        public string Username { get; set; }

        public int CartItemsCount { get; set; } = 0;

        public CategoryInfo SelectedProductCategory { get; set; }

        public ObservableRangeCollection<TrendingProduct> TrendingProducts { get; set; }

        public ObservableRangeCollection<CategoryInfo> Categories { get; set; }

        public IAsyncCommand ViewProductForCategoryCommand { get; }

        public override Task InitializeAsync(INavigationParameters parameters)
        {
            Username = _preferences.Get("userName", string.Empty);
            return Task.WhenAll(LoadTrendingProducts(), LoadCategories());
        }

        public override Task OnAppearingAsync()
        {
            return LoadCartItemsCount();
        }

        private async Task LoadTrendingProducts()
        {
            var products = await _productService.GetTrendingProductsAsync();
            foreach (var product in products)
            {
                TrendingProducts.Add(product);
            }
        }

        private async Task LoadCategories()
        {
            var categories = await _categoryService.GetCategoriesAsync();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private async Task LoadCartItemsCount()
        {
            var userId = _preferences.Get("userId", string.Empty);
            var totalCartItem = await _shoppingCartService.GetTotalCartItemsAsync(Convert.ToInt32(userId));
            CartItemsCount = totalCartItem.TotalItems;
        }

        private Task ExecuteViewProductForCategoryCommandAsync()
        {
            if (SelectedProductCategory == null)
            {
                return Task.CompletedTask;
            }

            var navigationParameters = new NavigationParameters
            {
                { nameof(CategoryInfo), SelectedProductCategory },
            };

            // Clear selection before navigating
            SelectedProductCategory = null;

            return NavigationService.NavigateAsync("ProductListPage", navigationParameters, useModalNavigation: true);
        }
    }
}
