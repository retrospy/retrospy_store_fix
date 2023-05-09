using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StoreFix
{
    public class InventoryManager
    {
        private dynamic? attributeStockItems;
        private dynamic? products;
        private HttpClient wcHttpClient;

        public InventoryManager() 
        {
            HttpMessageHandler handler = new HttpClientHandler();
            wcHttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://retro-spy.com"),
            };
            wcHttpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            var authBytes = System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("WCKey") + ":" + Environment.GetEnvironmentVariable("WCSecret"));
            string authHeaderString = System.Convert.ToBase64String(authBytes);
            wcHttpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + authHeaderString);
        }

        public void SyncInventory()
        {
            GetProducts();
            GetAttributeStockItems();

            if (products == null)
                return;

            foreach (var product in products)
            {
                int stock = -1;
                foreach (var attr in product.attributes)
                {
                    int tempStock = GetStock(attr.name.ToString());
                    if (attr.options is not null && attr.options.Count > 0)
                        tempStock /= GetQuantityUsed(attr.options[0].ToString());
                    if (tempStock < stock || stock == -1)
                        stock = tempStock;
                }

                if (stock != -1)
                {
                    UpdateStock(product.id.ToString(), stock > 10 ? 10 : stock);
                    Thread.Sleep(1000);
                }
            }
        }

        private int GetQuantityUsed(string quantityStr)
        {
            switch(quantityStr)
            {
                case "1x":
                    return 1;
                case "2x":
                    return 2;
                case "3x":
                    return 3;
                case "4x":
                    return 4;
                case "5x":
                    return 5;
                case "6x":
                    return 6;
                case "7x":
                    return 7;
                case "8x":
                    return 8;
                case "9x":
                    return 9;
            }

            return 1;
        }

        private void UpdateStock(string productId, int stock)
        {
            var s = new StringContent("{\n \"stock_quantity\": " + stock.ToString() + "\n}");
            s.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            wcHttpClient.PutAsync("/wp-json/wc/v3/products/" + productId, s);
        }

        private int GetStock(string name)
        {
            if (attributeStockItems == null)
                return -1;

            foreach (var obj in attributeStockItems)
            {
                if (obj.title == name)
                {
                    return obj.quantity;
                }
            }

            return -1;
        }

        private void GetProducts()
        {
            HttpResponseMessage response = wcHttpClient.GetAsync("/wp-json/wc/v3/products?per_page=100").Result;
            string responseStr = string.Empty;

            using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            {
                responseStr = stream.ReadToEnd();
            }

            products = JsonConvert.DeserializeObject(responseStr);
        }

        private void GetAttributeStockItems()
        {
            HttpResponseMessage response = wcHttpClient.GetAsync("/wp-json/wc/v3/attribute-stock?per_page=100").Result;
            string responseStr = string.Empty;

            using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            {
                responseStr = stream.ReadToEnd();
            }

            attributeStockItems = JsonConvert.DeserializeObject(responseStr);
        }
    }
}
