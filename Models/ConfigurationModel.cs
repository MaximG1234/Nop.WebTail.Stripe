using FluentValidation.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.WebTail.Stripe.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.WebTail.Stripe.Models
{
    [Validator(typeof(ConfigurationModelValidator))]
    public class ConfigurationModel
    {
        [NopResourceDisplayName("WebTail.Payments.Stripe.Fields.LiveSecretKey")]
        public string LiveSecretKey { get; set; }

        [NopResourceDisplayName("WebTail.Payments.Stripe.Fields.LivePublishableKey")]
        public string LivePublishableKey { get; set; }

        [NopResourceDisplayName("WebTail.Payments.Stripe.Fields.TestSecretKey")]
        public string TestSecretKey { get; set; }

        [NopResourceDisplayName("WebTail.Payments.Stripe.Fields.TestPublishableKey")]
        public string TestPublishableKey { get; set; }

        [NopResourceDisplayName("WebTail.Payments.Stripe.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        [NopResourceDisplayName("WebTail.Payments.Stripe.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [NopResourceDisplayName("WebTail.Payments.Stripe.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }


        [NopResourceDisplayName("WebTail.Payments.Stripe.Fields.TransactionMode")]
        public int TransactionModeId { get; set; }
        public SelectList TransactionModes { get; set; }

    }
}
