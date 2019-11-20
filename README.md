# Web Tail Pty Ltd - Stripe Plugin
 NopCommerce plugin to enable Stripe as a payment method
 
 Supported Versions: 4.10, 4.20, 4.30
 
This is a simple plugin example demonstrating how to implement Stripe as a payment method for NopCommerce. The project is written in C# and has the following features/functionality:
 
 - We are using the current latest version of [Stripe.net](https://github.com/stripe/stripe-dotnet). An extremely high quality C# wrapper for the Stripe API's.
 - We enable a sandbox/production mode.
 - We enable multiple capture modes.
	 1. Authorize - Used to pre-authorize a purchase and place a holder on the customers credit card for the order amount.
	 2. Capture - Used to immediately authorize the full transaction amount and begin the funds transfer process.
 - We enable additional fees to be charged for the use of this payment method.
	1. Additional fees can also be a percentage of the total order.

We use this plugin in production and intend to actively maintain it, but also encourage other contributors to help improve the code and users to provide feedback and suggestions.
 

## Installation

1. Download the plugin from the NopCommerce [marketplace](https://www.nopcommerce.com/marketplace.aspx).
2.  Connect to your NopCommerce site and extract the archive to /site/wwwroot/Plugins/.
3.  Navigate to https://yoursite.com/Admin/Plugin/List.
4. Find **'Credit Card (Stripe)'** and press install.
5. Restart site to apply changes.
6. Configure your API keys and press save.



![Configuration](https://github.com/MaximG1234/Nop.WebTail.Stripe/raw/master/Readme/configuration.PNG)

![enter image description here](https://github.com/MaximG1234/Nop.WebTail.Stripe/raw/master/Readme/credit%20card%20screen.PNG)
![enter image description here](https://github.com/MaximG1234/Nop.WebTail.Stripe/raw/master/Readme/payment%20method.PNG)
