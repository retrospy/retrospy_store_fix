using System.Text;
using System;
using Newtonsoft.Json;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using Newtonsoft.Json.Linq;

namespace StoreFix
{
    public class Program
    {
        static void Main(string[] args)
        {
            var inventoryManager = new InventoryManager();
            inventoryManager.SyncInventory();

            var tindieSync = new TindieSync();
            tindieSync.CreateOrders().GetAwaiter().GetResult(); ;
        }
    }
}