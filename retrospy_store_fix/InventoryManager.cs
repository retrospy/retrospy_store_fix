using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            GetAttributeStockItems();

            if (!LoadAndCheckAttributeStockState(attributeStockItems))
            {
                return;
            }

            GetProducts();

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
                            int variationStock = hasBaseStock ? stock : -100;
                            foreach (var attr in variation.attributes)
                            {
                                int tempVariationStock = GetStock(attr.option.ToString(), attr.name.ToString(), variationStock) / GetQuantityUsed(attr.option.ToString());
                                if (tempVariationStock < variationStock || variationStock == -100)
                                    variationStock = tempVariationStock;
                            }
                            if (variationStock != -100)
                            {
                                int newStock = Math.Min(hasBaseStock ? stock : 999999, variationStock);
                                int currentStock = 0;
                                if (variation.stock_quantity != null)
                                    variation.stock_quantity.ToObject<int>();

                                if (newStock != currentStock)
                                {
                                    UpdateVariationStock(product.id.ToString(), variation.id.ToString(), newStock);
                                    Thread.Sleep(1000);
                                }
                            }
                        }
                    }
                }
                else if (stock != -100 && stock != product.stock_quantity.ToObject<int>())
                {
                    UpdateStock(product.id.ToString(), stock);
                    Thread.Sleep(1000);
                }
            }

            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var stockstate_path = Path.Combine(Path.GetDirectoryName(location)!, "stockstate.json");
            string json = JsonConvert.SerializeObject(attributeStockItems);
            File.WriteAllText(stockstate_path, json);
        }

        private bool LoadAndCheckAttributeStockState(dynamic? attributeStockItems)
        {
            bool needToUpdateStock = false;

            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var stockstate_path = Path.Combine(Path.GetDirectoryName(location)!, "stockstate.json");

            try
            {
                if (File.Exists(stockstate_path))
                {
                    var stockstate = File.ReadAllText(stockstate_path);
                    dynamic? currentStockState = JsonConvert.DeserializeObject(stockstate);

                    int count = 0;
                    Dictionary<string, int> stockState = new Dictionary<string, int>();

                    if (currentStockState != null && attributeStockItems != null)
                    {
                        foreach (var stockattribute in currentStockState)
                        {
                            stockState.Add(stockattribute.title.ToString(), int.Parse(stockattribute.quantity.ToString()));
                            count++;
                        }

                        foreach (var serverStockAttribute in attributeStockItems)
                        {
                            int quantity = 0;
                            if (!stockState.TryGetValue(serverStockAttribute.title.ToString(), out quantity) || quantity != int.Parse(serverStockAttribute.quantity.ToString()))
                            {
                                needToUpdateStock = true;
                                break;
                            }
                            count--;
                        }

                        if (count != 0)
                            needToUpdateStock = true;
                    }
                    else
                    {
                        needToUpdateStock = true;
                    }
                }
                else
                {
                    needToUpdateStock = true;
                }

            }
            catch (Exception)
            {
            }

            return needToUpdateStock;
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

        private void GetProducts()
        {
            bool morePages = true;
            int page = 1;

            while (morePages)
            {
                HttpResponseMessage response = wcHttpClient.GetAsync(string.Format("/wp-json/wc/v3/products?per_page=100&page={0}", page)).Result;
                string responseStr = string.Empty;

                using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                {
                    responseStr = stream.ReadToEnd();
                }

                if (page == 1)
                {
                    products = JsonConvert.DeserializeObject(responseStr);
                    if (products?.Count < 100)
                        morePages = false;
                }
                else
                {
                    dynamic? moreProducts = JsonConvert.DeserializeObject(responseStr);
                    if (moreProducts != null)
                        foreach (var product in moreProducts)
                            products?.Add(product);

                    if (moreProducts?.Count < 100)
                        morePages = false;
                }
                page++;
                
            }
        }

        private void GetAttributeStockItems()
        {
            bool morePages = true;
            int page = 1;

            while (morePages)
            {
                HttpResponseMessage response = wcHttpClient.GetAsync(string.Format("/wp-json/wc/v3/attribute-stock?per_page=100&page={0}", page)).Result;
                string responseStr = string.Empty;

                using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                {
                    responseStr = stream.ReadToEnd();
                }

                if (page == 1)
                {
                    attributeStockItems = JsonConvert.DeserializeObject(responseStr);
                    if (attributeStockItems?.Count < 100)
                        morePages = false;
                }
                else
                {
                    dynamic? moreAttributeStockItems = JsonConvert.DeserializeObject(responseStr);
                    if (moreAttributeStockItems != null)
                        foreach (var item in moreAttributeStockItems)
                            attributeStockItems?.Add(item);

                    if (moreAttributeStockItems?.Count < 100)
                        morePages = false;
                }
                page++;
            }
        }

        private void GetProductVariations(int productId)
        {
            bool morePages = true;
            int page = 1;
            while (morePages)
            {
                HttpResponseMessage response = wcHttpClient.GetAsync(" /wp-json/wc/v3/products/" + productId + "/variations?per_page=100").Result;
                string responseStr = string.Empty;

                using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                {
                    responseStr = stream.ReadToEnd();
                }

                if (page == 1)
                {
                    productVariationsItems = JsonConvert.DeserializeObject(responseStr);
                    if (productVariationsItems?.Count < 100)
                        morePages = false;
                }
                else
                {
                    dynamic? moreProductVariationsItems = JsonConvert.DeserializeObject(responseStr);
                    if (moreProductVariationsItems != null)
                        foreach(var variation in moreProductVariationsItems)
                            productVariationsItems?.Add(variation);

                    if (moreProductVariationsItems?.Count < 100)
                        morePages = false;
                }
                page++;
            }
        }

    }
}
