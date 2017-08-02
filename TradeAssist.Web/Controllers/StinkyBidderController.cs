using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeAssist.Web.Models;
using TradeAssist.Web.Models.StinkyBidderViewModels;
using TradeAssist.Web.Services;

namespace TradeAssist.Web.Controllers
{
    [Authorize]
    public class StinkyBidderController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly IEmailSender _emailSender;
        readonly ISmsSender _smsSender;
        readonly ILogger _logger;

        public StinkyBidderController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<StinkyBidderController>();
        }

        //GET: /StinkyBidder
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var listOfMarkets = await TARClient.Current.Markets();
            var vm = new IndexViewModel();
            vm.Markets.AddRange(listOfMarkets);

            return View(vm);
        }

        //POST: /StinkyBidder
        [HttpPost]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("OrderEntry", "StinkyBidder", routeValues: new { currencyPair = vm.SelectedMarket });
        }

        //GET: /StinkyBidder/OrderEntry
        [HttpGet]
        public async Task<IActionResult> OrderEntry(string currencyPair)
        {
            var user = await GetCurrentUserAsync();

            // need some intelligence here around current price
            var model = new OrderEntryViewModel
            {
                CurrencyPair = currencyPair,
                AmountInBtc = 0.0m,
                IsSilent = false,
                Price = 0.00000000m,
                AveragePriceAcrossExchanges = 0.03450284m,
                RecommendedBuyAmountBasedOnUserCurrentTotalBalance = new List<decimal> { 0.5m, 1m, 2m, 3m }
            };

            return View(model);
        }

        //GET: /StinkyBidder/OrderEntry
        [HttpPost]
        public async Task<IActionResult> OrderEntry(OrderEntryViewModel vm)
        {
            var user = await GetCurrentUserAsync();

            var orderId = Guid.NewGuid();

            var order = new
            {
                OrderId = orderId,
                UserId = user.Id,
                CurrencyPair = vm.CurrencyPair,
                Price = vm.Price,
                Amount = vm.AmountInBtc,
                IsSilent = vm.IsSilent,
            };

            await EventHubRouter.Current.SendAsJson(order);

            return RedirectToAction("Index");
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}