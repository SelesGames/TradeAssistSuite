using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeAssist.Web.Models.ManageViewModels
{
    public class GoogleAuthenticatorViewModel
    {
        public string SecretKey { get; set; }
        public string BarcodeUrl { get; set; }
        public string Code { get; set; }
    }
}