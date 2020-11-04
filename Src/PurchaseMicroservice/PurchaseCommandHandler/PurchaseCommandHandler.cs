﻿namespace Purchase.CommandHandler
{
    using Common.Core;
    using Common.Core.Events;
    using Polly;
    using Polly.Retry;
    using Purchase.Command;
    using Purchase.Core;
    using Purchase.Domain.Model;
    using Purchase.Repository;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class PurchaseCommandHandler : ICommandHandler<PurchaseCommand, CommandResult>
    {
        private readonly IPurchaseRepostiory purchaseRepository;
        private readonly IEventBus serviceBus;
        private readonly IEmailService emailService;
        private IHttpClientFactory httpClientFactory;

        public PurchaseCommandHandler(IPurchaseRepostiory purchaseRepository, IEventBus serviceBus, IEmailService emailService,
            IHttpClientFactory httpClientFactory)
        {
            this.purchaseRepository = purchaseRepository;
            this.serviceBus = serviceBus;
            this.emailService = emailService;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<CommandResult> Handle(PurchaseCommand purchaseCommand)
        {
            CommandResult commandResponse = ValidateCommand(purchaseCommand);
            if (!commandResponse.Succeed)
            {
                return commandResponse;
            }
            if (!await IsStoreServiceOn())
            {
                commandResponse.Error = "Sorry! Store Service is not On. You have to wait until it is open";

                return commandResponse;
            }

            Purchase purchase = Map(purchaseCommand);
            await purchaseRepository.Save(purchase);

            ProductPurchasedEvent productPurchasedEvent = Map(purchase);
            await serviceBus.Publish(Constants.MessageQueues.PurchasedQueue, productPurchasedEvent);

            EmailParams emailParams = BuildEmailParameters(productPurchasedEvent);
            //await emailService.SendEmail(emailParams);

            return new CommandResult();
        }

        private CommandResult ValidateCommand(PurchaseCommand productPurchaseCommand)
        {
            var commandResponse = new CommandResult();

            if (null == productPurchaseCommand)
            {
                commandResponse.Error = "Sorry! Product Purchase Command should not be null.";
            }
            else if (productPurchaseCommand.LineItems == null || productPurchaseCommand.LineItems.Count == 0)
            {
                commandResponse.Error = "Sorry! Should have at least one Purchase Item.";
            }

            return commandResponse;
        }

        private Purchase Map(PurchaseCommand purchaseCommand)
        {
            var purchase = new Purchase
            {
                Id = purchaseCommand.PurchaseId,
                PurchaseDate = DateTime.UtcNow
            };

            foreach (LineItemCommand lineItemCommand in purchaseCommand.LineItems)
            {
                var productLineItem = new ProductLineItem
                {
                    Product = new Product
                    {
                        Id = lineItemCommand.ProductId,
                        ProductName = lineItemCommand.ProductName
                    }
                };
                productLineItem.PurchaseUnitPrice = lineItemCommand.PurchaseUnitPrice;
                productLineItem.PurchaseQuantity = lineItemCommand.PurchaseQuantity;

                purchase.LineItems.Add(productLineItem);
            }

            return purchase;
        }

        private ProductPurchasedEvent Map(Purchase purchase)
        {
            var productPurchasedEvent = new ProductPurchasedEvent
            {
                PurchaseDate = purchase.PurchaseDate
            };

            foreach (ProductLineItem productLineItem in purchase.LineItems)
            {
                var lineItem = new PurchasedLineItem
                {
                    ProductId = productLineItem.Product.Id,
                    PurchasedUnitPrice = productLineItem.PurchaseUnitPrice,
                    PurchasedQuantity = productLineItem.PurchaseQuantity
                };
                productPurchasedEvent.LineItems.Add(lineItem);
            }

            return productPurchasedEvent;
        }

        private EmailParams BuildEmailParameters(ProductPurchasedEvent @event)
        {
            long purchasedTotalPrice = 0;

            @event.LineItems.ToList().ForEach(item => purchasedTotalPrice += item.PurchaedTotalPrice);

            var emailParams = new EmailParams
            {
                Subject = "Product Purchased",
                Body = $"The Product Purchased Amount={purchasedTotalPrice}"
            };

            emailParams.ToList.Add(Constants.EmailAddressess.AdminPurchaseEmailAddress);

            return emailParams;
        }

        /// <summary>
        /// Implement Retry policy with REST call.
        /// Considering network is unreliable. so retry is needed.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> IsStoreServiceOn()
        {
            bool serviceOn = false;
            const string url = "http://localhost:4000/api/storequery/is-service-on";
            HttpClient httpClient = httpClientFactory.CreateClient();

            try
            {
                AsyncRetryPolicy retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3, item => TimeSpan.FromSeconds(2));

                await retryPolicy.ExecuteAsync(async () =>
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    serviceOn = await response.Content.ReadAsAsync<bool>();
                });
            }
            catch (HttpRequestException) { }

            return serviceOn;
        }
    }
}
