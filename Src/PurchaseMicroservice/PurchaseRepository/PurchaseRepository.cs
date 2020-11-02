﻿namespace PurchaseRepository
{
    using Common.Core;
    using MongoDB.Driver;
    using Purchase.Domain.Model;
    using Purchase.Repository;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class PurchaseRepository : IPurchaseRepostiory
    {
        private readonly IMongoDbService mongoDbService;

        public PurchaseRepository(IMongoDbService mongoDbService)
        {
            this.mongoDbService = mongoDbService;
        }

        public async Task<Purchase> GetById(string id)
        {
            IMongoCollection<Purchase> purchaseCollection = mongoDbService.GetCollection<Purchase>();

            IAsyncCursor<Purchase> purchaseCursor = await purchaseCollection.FindAsync(item => item.Id.Equals(id));

            return await purchaseCursor.FirstOrDefaultAsync();
        }

        public async Task<Purchase> Save(Purchase purchase)
        {
            IMongoCollection<Purchase> purchaseCollection = mongoDbService.GetCollection<Purchase>();

            await purchaseCollection.InsertOneAsync(purchase);

            return purchase;
        }

        public Task<IEnumerable<Purchase>> FindPurchases(DateTime from, DateTime to, int pageNumber, int pageSize, string sortField, int sortDirection)
        {
            IMongoCollection<Purchase> purchaseCollection = mongoDbService.GetCollection<Purchase>();

            int skip = (pageNumber - 1) * pageSize;

            IQueryable<Purchase> query = purchaseCollection.AsQueryable().Where(item => item.PurchaseDate >= from && item.PurchaseDate <= to);

            if (sortDirection == 0)
            {
                query.OrderBy(item => sortField);
            }
            else if (sortDirection == 1)
            {
                query.OrderByDescending(item => sortField);
            }

            IEnumerable<Purchase> purchases = query.Skip(skip).Take(pageSize);

            return Task.FromResult(purchases);
        }
    }
}