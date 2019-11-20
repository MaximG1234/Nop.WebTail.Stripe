using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.WebTail.Stripe
{
    public static class StripePaymentDefaults
    {
        public const string ViewComponentName = "PaymentStripe";
        public const string ControllerName = "PaymentStripe";
        public const string SystemName = "WebTail.Stripe";
        public const string PaymentFormScriptPath = "https://js.stripe.com/v2/";
        public const string AccessTokenRoute = "https://dashboard.stripe.com/account/apikeys";

        //public const string CustomerIdStripeKey = "CustomerId";
        public const string SaveCardKey = "SaveCard";

        public const string CustomerIdAttributeSandbox = "StripeCustomerIdSandbox";
        public const string CustomerIdAttributeProduction = "StripeCustomerIdProduction";

    }
}
