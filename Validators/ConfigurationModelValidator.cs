using FluentValidation;
using Nop.Web.Framework.Validators;
using Nop.WebTail.Stripe.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.WebTail.Stripe.Validators
{
    public class ConfigurationModelValidator : BaseNopValidator<ConfigurationModel>
    {
        public ConfigurationModelValidator()
        {
            
            this.RuleFor(model => model.LivePublishableKey).Must((model, context) =>
            {
                return !string.IsNullOrEmpty(model.LivePublishableKey);
            }).WithMessage("LivePublishableKey must be provided.");

            this.RuleFor(model => model.LiveSecretKey).Must((model, context) =>
            {
                return !string.IsNullOrEmpty(model.LiveSecretKey);
            }).WithMessage("LiveSecretKey must be provided.");

            this.RuleFor(model => model.TestPublishableKey).Must((model, context) =>
            {
                return !string.IsNullOrEmpty(model.TestPublishableKey);
            }).WithMessage("TestPublishableKey must be provided.");


            this.RuleFor(model => model.TestSecretKey).Must((model, context) =>
            {
                return !string.IsNullOrEmpty(model.TestSecretKey);
            }).WithMessage("TestSecretKey must be provided.");


        }
    }
}
