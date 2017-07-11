using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TradeAssist.Web.Models;
using TradeAssist.Web.Models.ManageViewModels;
using TradeAssist.Web.Services;
using TradeAssist.Web.TwoFactor;
using static TradeAssist.Web.TwoFactor.GoogleAuthenticatorHelper;
using TradeAssist.Web.Models.StinkyBidderViewModels;

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

        public Task<IActionResult> Index()
        {
            return OrderEntry("BTC_ETH");
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
                AmountInBtc = 0.00000000m,
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

            //Rfc6238AuthenticationService
            var secretKey = GenerateStandardSecret();// _googleAuthenticatorService.GenerateSecret();
            string issuer = System.Net.WebUtility.UrlEncode("TradeAssist");
            var label = $"{issuer}:{user.UserName}";
            var totpUrl = CreateSimpleTotpUri(label, secretKey, issuer);

            var model = new GoogleAuthenticatorViewModel
            {
                SecretKey = secretKey,
                BarcodeUrl = totpUrl.ToString(),
            };

            return View(model);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}