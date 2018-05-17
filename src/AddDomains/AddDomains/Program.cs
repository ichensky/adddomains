using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AddDomains
{
    public class Info {
        public string Url { get; set; }
        public string Hash { get; set; }
    }
    class Program
    {
       
        static void Main(string[] args)
        {
            func().Wait();
            Console.WriteLine("Hello World!");
        }
        public static async Task func() {

            var path = "../../../../../../";
            var info = new List<Info>();
            var dirPath = Path.Combine(path, "result");
            var domainsPath = Path.Combine(dirPath,"domains.txt");
            var infoPath = Path.Combine(dirPath, "info.txt");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var list = File.ReadAllLines($"{path}urls.txt");

            var urls = new List<string>();
            foreach (var item in list)
            {
                var url = item.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    urls.Add(url);
                }
            }

            await Task.WhenAll(urls.Select(x=> {
                var hash = GetHashString(x);
                var filename = $"{hash}.txt";
                info.Add(new Info { Hash=hash, Url=x });
                Console.WriteLine($"Downloading: {hash}={x}");

                return Download(x, Path.Combine(dirPath, filename)); }));

            File.WriteAllText(infoPath, JsonConvert.SerializeObject(info));


            var domains = new List<string>();
            var files = Directory.GetFiles(dirPath);

            foreach (var item in files)
            {
                var fileName = new FileInfo(item).Name;
                var hash = fileName.Replace(".txt","");
                var infoItem = info.FirstOrDefault(x => x.Hash == hash);

                if (infoItem != null)
                {
                    Console.WriteLine($"Processing: {hash}={infoItem.Url}");

                    var lines = File.ReadAllLines(item);
                    var fileDomains = Domains(lines);
                    domains.AddRange(fileDomains);
                }
            }

            Console.WriteLine($"Saving domains ...");

            File.WriteAllLines(domainsPath, domains.OrderBy(x=>x).Distinct());
        }
        public static async Task Download(string url, string filePath) {

            var client = new HttpClient();
            var domainsStr = await client.GetStringAsync(url);

            await File.WriteAllTextAsync(filePath, domainsStr);
        }




        public static List<string> Domains(IEnumerable<string> domains)
        {

            var list = new List<string>();
            foreach (var item in domains)
            {
                var str = item.Trim();
                var index = str.IndexOf("#");
                if (index > -1)
                {
                    str = str.Remove(index);
                }

                index = str.IndexOf(";");
                if (index > -1)
                {
                    str = str.Remove(index);
                }

                if (str.IndexOf("||") == 0 && str.IndexOf("^") == str.Length - 1)
                {
                    str = str.Remove(str.Length - 1).Remove(0, 2);
                }
                if (str.Length == 0 || !Regex.Match(str[0].ToString(), "[0-9a-zA-Z]").Success)
                {
                    continue;
                }
                str = Regex.Replace(str, @"\s+", " ");





                var arr = str.Split(' ');

                string domain = null;

                if (arr.Length == 1)
                {
                    domain = str;
                }
                else if (arr.Length == 2)
                {
                    domain = arr[1];
                }
                else
                {
                    continue;
                }
                if (domain.Length == 0)
                {
                    continue;
                }
                UriBuilder uri = null;
                try
                {
                    uri = new UriBuilder(domain);

                }
                catch (Exception)
                {
                    continue;
                }


                var host = uri.Host;

                if (System.Net.IPAddress.TryParse(host, out var ip))
                {
                    continue;
                }





                index = host.IndexOf("www.");
                if (index == 0)
                {
                    host = host.Remove(0, 4);
                }





                if (host.Length > 0
                    && host.IndexOf(".") > 0
                    && host.IndexOf(".") != host.Length - 1
                    && host.IndexOf(":") == -1
                    && host != "localhost.localdomain"
                    )
                {
                    list.Add(host);

                }
            }
            return list;


        }


        public static byte[] GetHash(string inputString)
        {
            var algorithm = SHA256.Create();  //or use SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
