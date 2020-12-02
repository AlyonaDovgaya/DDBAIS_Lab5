using Lab5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.Data
{
    public class DbInitializer
    {
        int _refferenceTableSize;
        int _operationalTableSize;
        public DbInitializer(int refferenceTableSize = 100, int operationalTableSize = 10000)
        {
            _refferenceTableSize = refferenceTableSize;
            _operationalTableSize = operationalTableSize;
        }
        public void Initialize(Lab5Context dbContext)
        {
            Random rand = new Random();
            if (!dbContext.Customers.Any())
            {
                for (int i = 0; i < _refferenceTableSize; i++)
                {
                    dbContext.Customers.Add(new Models.Customer
                    {
                        CustomerName = GetRandomString(15),

                    });
                }
            }
            dbContext.SaveChanges();

            if (!dbContext.Products.Any())
            {
                for (int i = 0; i < _refferenceTableSize; i++)
                {
                    dbContext.Products.Add(new Models.Product
                    {
                        ProductName = GetRandomString(15),
                    });
                }
            }
            dbContext.SaveChanges();

            if (!dbContext.Orders.Any())
            {
                var products = dbContext.Products.ToList();
                var customers = dbContext.Customers.ToList();
                for (int i = 0; i < _operationalTableSize; i++)
                {
                    var product = products.ElementAt(rand.Next(dbContext.Products.Count() - 1));
                    var customer = customers.ElementAt(rand.Next(dbContext.Customers.Count() - 1));
                    dbContext.Orders.Add(new Models.Order
                    {
                        OrderDate = GetRandomDate(new DateTime(2000, 1, 1), DateTime.Now),
                        Delivery = GetRandomString(30),
                        Volume = rand.Next(10, 90),
                        Product = product,
                        ProductId = product.ProductId,
                        Customer = customer,
                        CustomerId = customer.CustomerId
                    });
                }
            }
            dbContext.SaveChanges();
        }
        public string GetRandomString(int maxLength)
        {
            Random rand = new Random();
            int length = rand.Next(maxLength / 3, maxLength);
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var str = new char[length];
            int spaceRange = rand.Next(4, 7);
            for (int i = 0; i < length; i++)
            {
                if ((i + 1) % spaceRange == 0)
                {
                    str[i] = ' ';
                    spaceRange = rand.Next(4, 7);
                    continue;
                }
                str[i] = chars[rand.Next(chars.Length)];
            }
            return new string(str);
        }
        public DateTime GetRandomDate(DateTime minDate, DateTime maxDate)
        {
            Random rand = new Random();
            int range = (maxDate - minDate).Days;
            return minDate.AddDays(rand.Next(range));
        }
    }
}
