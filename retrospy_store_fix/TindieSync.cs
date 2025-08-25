using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StoreFix
{
    struct WooProduct
    {
        public int ProductId;
        public int Quantity;
        public int? VariationId;
        public float Price;
    }
    public class TindieSync
    {
        private HttpClient wcHttpClient;
        private HttpClient tHttpClient;

        public TindieSync()
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

            HttpMessageHandler handler1 = new HttpClientHandler();
            tHttpClient = new HttpClient(handler1)
            {
                BaseAddress = new Uri("https://www.tindie.com"),
            };
            tHttpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            tHttpClient.DefaultRequestHeaders.Add("User-Agent", "RetroSpy-Store-Fix-Service");
            tHttpClient.DefaultRequestHeaders.Add("Authorization", "ApiKey "+ Environment.GetEnvironmentVariable("TKey") + ":" + Environment.GetEnvironmentVariable("TSecret"));
        }

        private List<WooProduct>? GenerateWooProductsFromTindieOrder(dynamic? tindieOrder)
        {

            if (tindieOrder?.items == null) return null;

            var items = new List<WooProduct>();

            foreach (var item in tindieOrder.items)
            {
                switch(item.product.ToString())
                {
                    case "S-100 Newtech Model 6 Music Card Reproduction":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = item.options.ToString().Contains("Bare PCB or Fully Assembled: Fully Assembled") ? 995 : 1151;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = item.options.ToString().Contains("Bare PCB or Fully Assembled: Fully Assembled") ? 60.50f : 5.50f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            if (item.options.ToString().Contains("Add Speaker?: Yes"))
                            {
                                var wooItem1 = new WooProduct();
                                wooItem1.ProductId = 1148;
                                wooItem1.Quantity = item.quantity;
                                wooItem1.Price = 5.50f;
                                wooItem1.VariationId = 0;
                                items.Add(wooItem1);
                            }
                            if (item.options.ToString().Contains("Add TS78L05CT?: Yes"))
                            {
                                var wooItem1 = new WooProduct();
                                wooItem1.ProductId = 1146;
                                wooItem1.Quantity = item.quantity;
                                wooItem1.Price = 0.55f;
                                wooItem1.VariationId = 0;
                                items.Add(wooItem1);
                            }
                            break;
                        }
                    case "RetroSpy Pixel GameBoy Printer Emulator":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = 822;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = 29.70f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            bool addCable = item.options == " (Add Mini-USB Cable?: No)" ? false : true;
                            if (addCable)
                            {
                                var wooItemAdd = new WooProduct();
                                wooItemAdd.ProductId = 971;
                                wooItemAdd.Quantity = item.quantity;
                                wooItemAdd.Price = 5.50f;
                                wooItemAdd.VariationId = 0;
                                items.Add(wooItemAdd);
                            }
                            break;
                        }
                    case "Custom Arduino Mega Shield for ADAM Drive Emulator":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = 998;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = 12.10f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            bool addPartsKit = item.options == " (Bare PCB or Fully Assembled: Bare PCB)" ? false : true;
                            if (addPartsKit)
                            {
                                var wooItemAdd = new WooProduct();
                                wooItemAdd.ProductId = 997;
                                wooItemAdd.Quantity = item.quantity;
                                wooItemAdd.Price = item.options == " (Bare PCB or Fully Assembled: Fully Assembled)" ? 22.00f : 15.40f;
                                wooItemAdd.VariationId = 0;
                                items.Add(wooItemAdd);
                            }
                            break;
                        }
                    case "8pin Mini-DIN Breakout Board for a TMS-RGB":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = 1907;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = 1.00f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            break;
                        }
                    case "AIM 65 I/O Board":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = 1007;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = 68.20f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            bool addROM = item.options == " (Relocated Assembler to B000 on 2532 EPROM: Yes)" ? true : false;
                            if (addROM)
                            {
                                var wooItemAdd = new WooProduct();
                                wooItemAdd.ProductId = 1006;
                                wooItemAdd.Quantity = item.quantity;
                                wooItemAdd.Price = 5.50f;
                                wooItemAdd.VariationId = 0;
                                items.Add(wooItemAdd);
                            }
                            break;
                        }
                    case "Active Buzzer for Gotek Floppy Drive Emulators":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = 1844;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = 17.60f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            break;
                        }
                    case "Power supply board for SYM-1 SBC":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = item.options == " (Assembly: Fully Assembled)" ? 1000 : 1150;  ;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = item.options == " (Assembly: Fully Assembled)" ? 23.10f : 5.50f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            break;
                        }
                    case "I/O board for SYM-1 SBC":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = item.options == " (Assembly: Fully Assembled)" ? 1001 : 1152; ;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = item.options == " (Assembly: Fully Assembled)" ? 24.20f : 5.50f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            break;
                        }
                    case "RetroSpy Tristimulus XYZ Color Sensor":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = 2124; ;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = 27.50f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            break;
                        }
                    case "2532 to 2764 EPROM Adapter":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = 1954; ;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = 36.30f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            break;
                        }
                    case "HR12 8pin Breakout Board":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = 1908; ;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = 1.00f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            break;
                        }
                    case "Simple 1d6 Demo Board":
                        {
                            var wooItem = new WooProduct();
                            wooItem.ProductId = 991;
                            wooItem.Quantity = item.quantity;
                            wooItem.Price = 8.8f;
                            wooItem.VariationId = 0;
                            items.Add(wooItem);
                            bool addPartsKit = item.options == " (Bare PCB or Fully Assembled: Bare PCB)" ? false : true;
                            if (addPartsKit)
                            {
                                var wooItemAdd = new WooProduct();
                                wooItemAdd.ProductId = 984;
                                wooItemAdd.Quantity = item.quantity;
                                wooItemAdd.Price = 13.20f;
                                wooItemAdd.VariationId = 0;
                                items.Add(wooItemAdd);
                            }
                            break;
                        }
                    case "SymDOS I/O board for SYM-1 SBC":
                        {
                            var wooItem = new WooProduct();
                            wooItem.Quantity = item.quantity;
                            if (item.options.ToString().Contains("Assembly: Fully Assembled"))
                            {
                                wooItem.ProductId = 1003;
                                if (item.options.ToString().Contains("Teensy 4.0 Installed?: Yes"))
                                {
                                    wooItem.VariationId = 1910;
                                    wooItem.Price = 80.30f;
                                }
                                else
                                {
                                    wooItem.VariationId = 1911;
                                    wooItem.Price = 46.20f;
                                }
                            }    
                            else if (item.options.ToString().Contains("Assembly: Bare PCB"))
                            {
                                wooItem.ProductId = 1149;
                                wooItem.VariationId = 0;
                                wooItem.Price = 5.50f;
                            }
                            else
                            {
                                wooItem.ProductId = 2018;
                                wooItem.VariationId = 0;
                                wooItem.Price = 63.00f;
                            }
                            items.Add(wooItem);
                            
                            if (item.options.ToString().Contains("Add SYM-1 Monitor v1.1 on 2532 EEPROM: Yes"))
                            {
                                var wooItem1 = new WooProduct();
                                wooItem1.ProductId = 1149;
                                wooItem1.Quantity = item.quantity;
                                wooItem1.Price = 5.50f;
                                wooItem1.VariationId = 0;
                                items.Add(wooItem1);
                            }
                            break;
                        }
                    case "Simple Speaker & Switches Demo Board":
                        {
                            var wooItem = new WooProduct();
                            wooItem.Quantity = item.quantity;
                            wooItem.ProductId = 986;
                            if (item.options.ToString().Contains("Board Version: Version 1"))
                            {     
                                wooItem.VariationId = 987;
                                wooItem.Price = 8.80f;
                            }
                            else if (item.options.ToString().Contains("Board Version: Version 2"))
                            {
                                wooItem.VariationId = 988;
                                wooItem.Price = 13.20f;
                            }
                            else
                            {
                                wooItem.VariationId = 0;
                                wooItem.Price = 19.80f;
                            }
                            items.Add(wooItem);

                            if (item.options.ToString().Contains("Parts Kit?: v1/v2 Parts Kit"))
                            {
                                var wooItem1 = new WooProduct();
                                wooItem1.ProductId = 982;
                                wooItem1.Quantity = item.quantity;
                                wooItem1.Price = 13.20f;
                                wooItem1.VariationId = 0;
                                items.Add(wooItem1);
                            }
                            else if (item.options.ToString().Contains("Parts Kit?: v3 Parts Kit"))
                            {
                                var wooItem1 = new WooProduct();
                                wooItem1.ProductId = 983;
                                wooItem1.Quantity = item.quantity;
                                wooItem1.Price = 13.20f;
                                wooItem1.VariationId = 0;
                                items.Add(wooItem1);
                            }
                            break;
                        }
                }
            }

            if (items.Count == 0) 
                return null;

            return items;
        }

        private dynamic? GetOrders()
        {
            HttpResponseMessage response = tHttpClient.GetAsync("/api/v1/order/?shipped=false").Result;
            string responseStr = string.Empty;

            using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            {
                responseStr = stream.ReadToEnd();
            }

           return JsonConvert.DeserializeObject(responseStr);
        }

        private dynamic? GetProductOptions(int sku)
        {
            HttpResponseMessage response = tHttpClient.GetAsync("/api/v2/products/" + sku + "/options/").Result;
            string responseStr = string.Empty;

            using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            {
                responseStr = stream.ReadToEnd();
            }

            return JsonConvert.DeserializeObject(responseStr);
        }

        public async Task CreateOrders()
        {

            dynamic? tindieOrders = GetOrders();
            if (tindieOrders == null)
                return;

            int lastOrder;
            try
            {
                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var lastorder_path = Path.Combine(Path.GetDirectoryName(location)!, "lastorder.txt");
                StreamReader sr = new StreamReader(lastorder_path);
                var line = sr.ReadLine();
                lastOrder = int.Parse(line ?? "0");
                sr.Close();
            }
            catch (Exception)
            {
                lastOrder = 0;
            }

            if (lastOrder == 0)
                return;

            foreach (var order in tindieOrders.orders)
            {
                if (order.number > lastOrder)
                {
                    var productsBought = GenerateWooProductsFromTindieOrder(order);

                    if (productsBought.Count == 0)
                        continue;

                    string createJson = string.Format(jsonOrderTemplate, order.shipping_name.ToString().Split(" ")[0],
                                                                         order.shipping_name.ToString().Split(" ")[1],
                                                                         order.shipping_street,
                                                                         "",
                                                                         order.shipping_city,
                                                                         order.shipping_state,
                                                                         order.shipping_postcode,
                                                                         order.shipping_country_code,
                                                                         order.email,
                                                                         order.phone,
                                                                         order.shipping_name.ToString().Split(" ")[0],
                                                                         order.shipping_name.ToString().Split(" ")[1],
                                                                         order.shipping_street,
                                                                         "",
                                                                         order.shipping_city,
                                                                         order.shipping_state,
                                                                         order.shipping_postcode,
                                                                         order.shipping_country_code,
                                                                         GenerateItemJson(productsBought),
                                                                         order.total_shipping
                    ).Replace("\n", "").Replace("\r", "");

                    var s = new StringContent(createJson);
                    s.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    var response = await wcHttpClient.PostAsync("/wp-json/wc/v3/orders", s);
                    string responseStr;
                    using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                    {
                        responseStr = stream.ReadToEnd();
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        var lastorder_path = Path.Combine(Path.GetDirectoryName(location)!, "lastorder.txt");
                        StreamWriter sw = new StreamWriter(lastorder_path);
                        sw.WriteLine(order.number);
                        sw.Close();
                    }
                    else
                    {
                        return;
                    }
                }
            }
            
        }

        private string GenerateItemJson(List<WooProduct> products)
        {
            string output = string.Empty;
            for(int i = 0; i < products.Count; i++)
            {
                if (i != 0)
                    output += ",";

                if (products[i].VariationId == 0)
                    output += string.Format("{{\"product_id\": {0}, \"quantity\": {1}, \"total\": \"{2}\"}}", products[i].ProductId, products[i].Quantity, products[i].Price);
                else
                    output += string.Format("{{\"product_id\": {0}, \"quantity\": {1}, \"total\": \"{2}\", \"variation_id\": {3}}}", products[i].ProductId, products[i].Quantity, products[i].Price, products[i].VariationId);
            }

            return output;
        }

        private string jsonOrderTemplate = @"{{
  ""set_paid"": true,
  ""billing"": {{
    ""first_name"": ""{0}"",
    ""last_name"": ""{1}"",
    ""address_1"": ""{2}"",
    ""address_2"": ""{3}"",
    ""city"": ""{4}"",
    ""state"": ""{5}"",
    ""postcode"": ""{6}"",
    ""country"": ""{7}"",
    ""email"": ""{8}"",
    ""phone"": ""{9}""
  }},
  ""shipping"": {{
    ""first_name"": ""{10}"",
    ""last_name"": ""{11}"",
    ""address_1"": ""{12}"",
    ""address_2"": ""{13}"",
    ""city"": ""{14}"",
    ""state"": ""{15}"",
    ""postcode"": ""{16}"",
    ""country"": ""{17}""
  }},
  ""line_items"": [
    {18}
  ],
  ""shipping_lines"": [
    {{
      ""total"": ""{19}"",
      ""method_id"": 3
    }}
  ]
}}";
    }
}
