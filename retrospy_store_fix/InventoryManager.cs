using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StoreFix
{
    public class InventoryManager
    {
        private dynamic? attributeStockItems;
        private dynamic? products;
        private dynamic? productVariationsItems;
        private HttpClient wcHttpClient;

        public InventoryManager() 
        {
            HttpMessageHandler handler = new HttpClientHandler();
            wcHttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://retro-spy.com"),
            };
            wcHttpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            wcHttpClient.DefaultRequestHeaders.Add("User-Agent", "RetroSpy-Store-Fix-Service");
            var authBytes = System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("WCKey") + ":" + Environment.GetEnvironmentVariable("WCSecret"));
            string authHeaderString = System.Convert.ToBase64String(authBytes);
            wcHttpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + authHeaderString);
        }

        public void SyncInventory()
        {
            int productPage = 1;
            while (productPage < 3)
            {
                GetProducts(productPage);
                GetAttributeStockItems();

                if (products == null)
                    return;

                foreach (var product in products)
                {
                    bool hasVariations = false;
                    int id = product.id;
                    int stock = -100;
                    int tempStock = -100;
                    bool hasBaseStock = false;
                    foreach (var attr in product.attributes)
                    {
                        if (attr.variation == true && product.type == "variable")
                        {
                            GetProductVariations(int.Parse((string)product.id));
                            if (productVariationsItems != null)
                            {
                                foreach (var variation in productVariationsItems)
                                {
                                    if (variation.manage_stock == true)
                                    {
                                        hasVariations = true;
                                    }
                                }

                                if (hasVariations)
                                    continue;
                            }
                        }
                        tempStock = GetStock(attr.options[0].ToString(), attr.name.ToString());
                        if (attr.options is not null && attr.options.Count > 0)
                            tempStock /= GetQuantityUsed(attr.options[0].ToString());
                        if (tempStock < stock || stock == -100)
                            stock = tempStock;
                        hasBaseStock = true;
                    }

                    if (hasVariations && productVariationsItems != null)
                    {
                        foreach (var variation in productVariationsItems)
                        {
                            if (variation.manage_stock == true)
                            {
                                int variationStock = -100;
                                foreach (var attr in variation.attributes)
                                {
                                    int tempVariationStock = GetStock(attr.option.ToString(), attr.name.ToString(), variationStock) / GetQuantityUsed(attr.option.ToString());
                                    if (tempVariationStock < variationStock || variationStock == -100)
                                        variationStock = tempVariationStock;
                                }
                                if (variationStock != -100)
                                    UpdateVariationStock(product.id.ToString(), variation.id.ToString(), Math.Min(hasBaseStock ? stock : 999999, variationStock));
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    else if (stock != -100)
                    {
                        UpdateStock(product.id.ToString(), stock);
                        Thread.Sleep(1000);
                    }
                }
                productPage++;
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

        private void UpdateVariationStock(string productId, string variationId, int stock)
        {
            var s = new StringContent("{\n \"stock_quantity\": " + stock.ToString() + "\n}");
            s.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            wcHttpClient.PutAsync("/wp-json/wc/v3/products/" + productId + "/variations/" + variationId, s);
        }

        private int GetStock(string option, string name, int currentStock)
        {
            if (attributeStockItems == null)
                return -100;

            foreach (var obj in attributeStockItems)
            {
                if (obj.title == name)
                {
                    if (option != "No")
                        return obj.quantity;
                    else
                        return currentStock;
                }
                else if (obj.title == name + " (" + option + ")")
                {
                    return obj.quantity;
                }
            }

            return -100;
        }

        private int GetStock(string option, string name)
        {
            if (attributeStockItems == null)
                return -100;

            foreach (var obj in attributeStockItems)
            {
                if (obj.title == name || obj.title == name + " (" + option + ")")
                {
                    return obj.quantity;
                }
            }

            return -100;
        }

        private void GetProducts(int page)
        {
            HttpResponseMessage response = wcHttpClient.GetAsync(string.Format("/wp-json/wc/v3/products?per_page=100&page={0}", page)).Result;
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

        private void GetProductVariations(int productId)
        {
            HttpResponseMessage response = wcHttpClient.GetAsync(" /wp-json/wc/v3/products/" + productId + "/variations?per_page=100").Result;
            string responseStr = string.Empty;

            using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            {
                responseStr = stream.ReadToEnd();
            }

           productVariationsItems = JsonConvert.DeserializeObject(responseStr);
        }

    }
}
