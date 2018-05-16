using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AddDomains
{
    class Program
    {
       
        static void Main(string[] args)
        {
            func().Wait();
            Console.WriteLine("Hello World!");
        }
        public static async Task func() {

            var path = "../../../../../../";
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

            var domains = await DomainsFromUrls(urls);
            File.WriteAllLines($"{path}domains.txt", domains);
        }
        public static async Task<List<string>> DomainsFromUrls(IEnumerable<string> urls) {

            var domains = new List<string>();
            foreach (var url in urls)
            {
                try
                {
                    Console.WriteLine($"Getting domain names from url: {url}");
                    var client = new HttpClient();
                    var domainsStr = await client.GetStringAsync(url);
                    var domainsArr = domainsStr.Split(new[] { '\r', '\n' });
                    var xDomains = Domains(domainsArr);
                    domains.AddRange(xDomains);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            var arr = domains.Distinct().OrderBy(x=>x);



            return domains;
        }

        public static List<string> Domains(IEnumerable<string> domains)
        {
            var list = new List<string>();
            foreach (var item in domains)
            {
                var str = item;
                var index = str.IndexOf("#");
                if (index>-1)
                {
                    str = item.Remove(index);
                }
                str = str
                    .Replace('|', ' ')
                    .Replace('^', ' ')
                    .Replace('\t', ' ')
                    .Trim();
                    ;
                str =Regex.Replace(str, @"\s+", " ");

                var aa = str.Split(' ');

                string domain = null;

                if (aa.Length == 1)
                {
                    domain = str;
                }
                else if (aa.Length == 2)
                {
                    domain = aa[1];
                }
                else if (aa.Length == 0) { }
                else {
                    Console.WriteLine($"Error in domain str: {str}");
                }


                if (domain!=null&& domain.Length>0
                    && domain != "localhost"
                    && domain != "localhost.localdomain"
                    && domain != "broadcasthost"
                    && domain != "local"
                    && domain != "ip6-localnet"
                    && domain != "ip6-mcastprefix"
                    && domain != "ip6-allnodes"
                    && domain != "ip6-allrouters"
                    && domain != "ip6-allhosts"
                    )
                {
                    list.Add(domain);

                }
            }
            return list;
        }
    }
}
