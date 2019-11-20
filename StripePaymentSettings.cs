using Nop.Core.Configuration;
using Stripe;
using System;

namespace Nop.WebTail.Stripe
{
    public class StripePaymentSettings : ISettings
    {
        public string LiveSecretKey { get; set; }
        public string LivePublishableKey { get; set; }
        public string TestSecretKey { get; set; }
        public string TestPublishableKey { get; set; }
        public bool UseSandbox { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }
        public TransactionMode TransactionMode { get; set; }

        public string GetApiKey()
        {
            if (this.UseSandbox)
            {
                return this.TestSecretKey;
            }
            else
            {
                return this.LiveSecretKey;
            }
        }

        public string GetCustomerIdKey()
        {
            if (this.UseSandbox)
            {
                return StripePaymentDefaults.CustomerIdAttributeSandbox;
            }
            else
            {
                return StripePaymentDefaults.CustomerIdAttributeProduction;
            }
        }

        public StripeClient GetStripeClient()
        {
            try
            {
                var stripeClient = new StripeClient(this.GetApiKey());
                return stripeClient;
            } 
            catch (Exception ex)
            {
                throw;
            }
            
        }
    }
}
