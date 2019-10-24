using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.WebTail.Stripe.Services
{
    public class EventConsumer : IConsumer<PageRenderingEvent>
    {
        public void HandleEvent(PageRenderingEvent eventMessage)
        {

            ////add js script to one page checkout
            //if (eventMessage.GetRouteNames().Any(r => r.Equals("CheckoutOnePage")))
            //{
            //    eventMessage.Helper.AddScriptParts(ResourceLocation.Footer, StripePaymentDefaults.PaymentFormScriptPath, excludeFromBundle: true);
            //}
        }
        
    }
}
