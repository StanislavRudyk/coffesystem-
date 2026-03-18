using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Cafe.Client.Manager.Services
{
    public class OrderRecord
    {
        public DateTime Timestamp { get; set; }
        public decimal Total { get; set; }
        public List<string> Items { get; set; }
        public string Method { get; set; }
    }

    public static class DataService
    {
        private static readonly string Path = @"C:\Users\Game-X\Desktop\курсовая\shared_ledger.json";

        public static void SaveOrder(decimal total, List<string> items, string method)
        {
            try
            {
                var records = LoadAll();
                records.Add(new OrderRecord
                {
                    Timestamp = DateTime.Now,
                    Total = total,
                    Items = items,
                    Method = method
                });

                string json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Path, json);
            }
            catch { }
        }

        public static List<OrderRecord> LoadAll()
        {
            if (!File.Exists(Path)) return new List<OrderRecord>();
            try
            {
                string json = File.ReadAllText(Path);
                return JsonSerializer.Deserialize<List<OrderRecord>>(json) ?? new List<OrderRecord>();
            }
            catch { return new List<OrderRecord>(); }
        }
    }
}
