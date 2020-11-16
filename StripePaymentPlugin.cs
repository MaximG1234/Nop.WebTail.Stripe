using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Stores;
using Nop.WebTail.Stripe.Extensions;
using Nop.WebTail.Stripe.Models;
using Nop.WebTail.Stripe.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using stripe = Stripe;

namespace Nop.WebTail.Stripe
{
    public class StripePaymentPlugin : BasePlugin, IPaymentMethod
    {
        private readonly StripePaymentSettings _stripePaymentSettings;
        private readonly CurrencySettings _currencySettings;

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;        
        private readonly ICountryService _countryService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;

        public StripePaymentPlugin(ILocalizationService localizationService, 
                                   IGenericAttributeService genericAttributeService,
                                   ICurrencyService currencyService,
                                   ICustomerService customerService,
                                   IStateProvinceService stateProvinceService,
                                   ICountryService countryService,
                                   IStoreService storeService,
                                   ISettingService settingService, 
                                   IPaymentService paymentService,
                                   IWebHelper webHelper, 
                                   ILogger logger,
                                   StripePaymentSettings stripePaymentSettings,
                                   CurrencySettings currencySettings)
        {
            this._localizationService = localizationService;
            this._genericAttributeService = genericAttributeService;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._stateProvinceService = stateProvinceService;
            this._countryService = countryService;
            this._webHelper = webHelper;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._stripePaymentSettings = stripePaymentSettings;
            this._currencySettings = currencySettings;
            this._logger = logger;
        }

        public bool SupportCapture => true;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Manual;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        public string PaymentMethodDescription => this._localizationService.GetResource("WebTail.Payments.Stripe.PaymentMethodDescription");

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{this._webHelper.GetStoreLocation()}Admin/{StripePaymentDefaults.ControllerName}/Configure";
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            if (cancelPaymentRequest == null)
                throw new ArgumentException(nameof(cancelPaymentRequest));

            //always success
            return new CancelRecurringPaymentResult();
        }

        public bool CanRePostProcessPayment(Core.Domain.Orders.Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return false;
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            if (capturePaymentRequest == null)
                throw new ArgumentNullException(nameof(capturePaymentRequest));

            var currentStore = EngineContext.Current.Resolve<IStoreContext>().CurrentStore;

            stripe.Charge charge = capturePaymentRequest.CreateCapture(this._stripePaymentSettings, currentStore);
            
            if (charge.GetStatus() == StripeChargeStatus.Succeeded)
            {
                //successfully captured
                return new CapturePaymentResult
                {
                    NewPaymentStatus = PaymentStatus.Paid,
                    CaptureTransactionId = charge.Id
                };
            }
            else
            {
                //successfully captured
                return new CapturePaymentResult
                {
                    Errors = new List<string>(new [] { $"An error occured attempting to capture charge {charge.Id}." }),
                    NewPaymentStatus = PaymentStatus.Authorized,
                    CaptureTransactionId = charge.Id
                };
            }

            
            

        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this._paymentService.CalculateAdditionalFee(cart, this._stripePaymentSettings.AdditionalFee, this._stripePaymentSettings.AdditionalFeePercentage);

            return result;
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest()
            {
                CreditCardType = form["CreditCardType"],
                CreditCardName = form["CardholderName"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = int.Parse(form["ExpireMonth"]),
                CreditCardExpireYear = int.Parse(form["ExpireYear"]),
                CreditCardCvv2 = form["CardCode"]
            };
        }

        public string GetPublicViewComponentName()
        {
            return StripePaymentDefaults.ViewComponentName;
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {

            bool dataMissing = string.IsNullOrEmpty(this._stripePaymentSettings.LivePublishableKey) ||
                               string.IsNullOrEmpty(this._stripePaymentSettings.LiveSecretKey) ||
                               string.IsNullOrEmpty(this._stripePaymentSettings.TestPublishableKey) ||
                               string.IsNullOrEmpty(this._stripePaymentSettings.TestSecretKey);
            
            return dataMissing;
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest, bool isRecurringPayment)
        {
            
            var currentStore = EngineContext.Current.Resolve<IStoreContext>().CurrentStore;
            var chargeResponse = processPaymentRequest.CreateCharge(this._stripePaymentSettings, 
                                                                    this._currencySettings,
                                                                    currentStore, 
                                                                    this._customerService, 
                                                                    this._stateProvinceService,
                                                                    this._countryService,
                                                                    this._currencyService, 
                                                                    this._genericAttributeService);
                
            if (chargeResponse.GetStatus() == StripeChargeStatus.Failed)
                throw new NopException(chargeResponse.FailureMessage);
                
            string transactionResult = $"Transaction was processed by using Stripe. Status is {chargeResponse.GetStatus()}";
            var result = new ProcessPaymentResult()
            {
                NewPaymentStatus = chargeResponse.GetPaymentStatus(this._stripePaymentSettings.TransactionMode)
            };

            if (this._stripePaymentSettings.TransactionMode == TransactionMode.Authorize)
            {
                result.AuthorizationTransactionId = chargeResponse.Id;
                result.AuthorizationTransactionResult = transactionResult;
            }
                
            if (this._stripePaymentSettings.TransactionMode == TransactionMode.Charge)
            {
                result.CaptureTransactionId = chargeResponse.Id;
                result.CaptureTransactionResult = transactionResult;
            }
                
            return result;
            
            
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            return this.ProcessPayment(processPaymentRequest, false);
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            return this.ProcessPayment(processPaymentRequest, true);
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var refund = refundPaymentRequest.CreateRefund(this._stripePaymentSettings, this._currencySettings, this._currencyService);

            if (refund.GetStatus() != StripeRefundStatus.Succeeded)
            {
                return new RefundPaymentResult { Errors = new[] { $"Refund is {refund.Status}" }.ToList() };
            }
            else
            {
                return new RefundPaymentResult
                {
                    NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
                };
            }
            
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(this._localizationService);
            var model = new PaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));

            return warnings;
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public override void Install()
        {
            //settings
            this._settingService.SaveSetting(new StripePaymentSettings
            {
                UseSandbox = true,
                AdditionalFee = 0,
                AdditionalFeePercentage = false,
                LivePublishableKey = string.Empty,
                LiveSecretKey = string.Empty,
                TestPublishableKey = string.Empty,
                TestSecretKey = string.Empty,
            });

            this._localizationService.AddOrUpdatePluginLocaleResource("Webtail.Payments.Stripe.Instructions", @"
                <p>
                    For plugin configuration follow these steps:<br />
                    <br />
                    1. You will need a Stripe Merchant account. If you don't already have one, you can sign up here: <a href=""https://dashboard.stripe.com/register"" target=""_blank"">https://dashboard.stripe.com/register</a><br />
                    <em>Important: Your merchant account must be approved by Stripe prior to you be able to cash out payments.</em><br />
                    2. Sign in to your Stripe Developer Portal at <a href=""https://dashboard.stripe.com/login"" target=""_blank"">https://dashboard.stripe.com/login</a>; use the same sign in credentials as your merchant account.<br />
                    3. Use the API keys provided at <a href=""https://dashboard.stripe.com/account/apikeys"" target=""_blank"">https://dashboard.stripe.com/account/apikeys</a> to configure the account.
                    <br />
                </p>");

            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.UseSandbox", "Use sandbox");
            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.UseSandbox.Hint", "Determine whether to use sandbox credentials.");
            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.TransactionMode", "Transaction mode");
            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.TransactionMode.Hint", "Choose the transaction mode.");

            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.LiveSecretKey", "Live Secret Key");
            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.LivePublishableKey", "Live Publishable Key");
            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.TestSecretKey", "Test Secret Key");
            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.TestPublishableKey", "Test Publishable Key");

            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.AdditionalFee", "Additional Fee");
            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.Fields.AdditionalFeePercentage", "Is Fee Percentage");
            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Stripe.PaymentMethodDescription", "Pay By Credit Card");

            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Labels.ExpirationMonth", "Expiry Month");
            this._localizationService.AddOrUpdatePluginLocaleResource("WebTail.Payments.Labels.ExpirationYear", "Expiry Year");

            base.Install();
        }

        public override void Uninstall()
        {
            this._settingService.DeleteSetting<StripePaymentSettings>();

            this._localizationService.DeletePluginLocaleResource("Webtail.Payments.Stripe.Instructions");

            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.UseSandbox");
            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.UseSandbox.Hint");
            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.TransactionMode");
            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.TransactionMode.Hint");

            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.LiveSecretKey");
            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.LivePublishableKey");
            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.TestSecretKey");
            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.TestPublishableKey");

            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.AdditionalFee");
            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.Fields.AdditionalFeePercentage");
            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Stripe.PaymentMethodDescription");


            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Labels.ExpirationMonth");
            this._localizationService.DeletePluginLocaleResource("WebTail.Payments.Labels.ExpirationYear");

            base.Uninstall();
        }

    }
}
