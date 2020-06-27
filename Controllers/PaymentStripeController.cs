using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.WebTail.Stripe.Extensions;
using Nop.WebTail.Stripe.Models;

namespace Nop.WebTail.Stripe.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class PaymentStripeController : BasePaymentController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;

        private readonly StripePaymentSettings _stripePaymentSettings;

        public PaymentStripeController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            INotificationService notificationService,
            StripePaymentSettings stripePaymentSettings)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._notificationService = notificationService;
            this._stripePaymentSettings = stripePaymentSettings;
        }
        
        public IActionResult Configure()
        {
            //whether user has the authority
            if (!this._permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return this.AccessDeniedView();

            //prepare model
            var model = new ConfigurationModel
            {
                LivePublishableKey = this._stripePaymentSettings.LivePublishableKey,
                LiveSecretKey = this._stripePaymentSettings.LiveSecretKey,
                TestPublishableKey = this._stripePaymentSettings.TestPublishableKey,
                TestSecretKey = this._stripePaymentSettings.TestSecretKey,
                UseSandbox = this._stripePaymentSettings.UseSandbox,
                AdditionalFee = this._stripePaymentSettings.AdditionalFee,
                AdditionalFeePercentage = this._stripePaymentSettings.AdditionalFeePercentage,
                TransactionModeId = (int)this._stripePaymentSettings.TransactionMode,
                TransactionModes = this._stripePaymentSettings.TransactionMode.ToSelectList(),
            };

            return this.View("~/Plugins/WebTail.Stripe/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!this._permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return this.AccessDeniedView();

            if (!this.ModelState.IsValid)
                return this.Configure();

            this._stripePaymentSettings.TransactionMode = (TransactionMode)model.TransactionModeId;
            this._stripePaymentSettings.AdditionalFee = model.AdditionalFee;
            this._stripePaymentSettings.UseSandbox = model.UseSandbox;
            this._stripePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            this._stripePaymentSettings.LivePublishableKey = model.LivePublishableKey;
            this._stripePaymentSettings.LiveSecretKey = model.LiveSecretKey;
            this._stripePaymentSettings.TestPublishableKey = model.TestPublishableKey;
            this._stripePaymentSettings.TestSecretKey = model.TestSecretKey;

            if (this._stripePaymentSettings.TryConnect())
            {
                this._settingService.SaveSetting(this._stripePaymentSettings);
                this._notificationService.SuccessNotification(this._localizationService.GetResource("Admin.Plugins.Saved"));
            }
            else
            {
                this._notificationService.ErrorNotification("Cannot connect to stripe using provided credentials.");
            }

            return this.Configure();

        }
    }
}
