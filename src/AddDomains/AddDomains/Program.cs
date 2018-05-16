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
                    var xDomains = await Domains(domainsArr);
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

        public static Task<List<string>> Domains(IEnumerable<string> domains)
        {
            return Task.Factory.StartNew(() =>
            {
                var list = new List<string>();
                foreach (var item in domains)
                {
                    var str = item;
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
                    index = str.IndexOf(",");
                    if (index > -1)
                    {
                        str = str.Remove(index);
                    }
                    

                  
                    str = str
                        .Replace('|', ' ')
                        .Replace('^', ' ')
                        .Replace('\t', ' ')
                        .Trim();
                    ;
                    index = str.LastIndexOf(".jpg");
                    if (index > -1)
                    {
                        str = str.Remove(index);
                    }
                    index = str.LastIndexOf(".gif");
                    if (index > -1)
                    {
                        str = str.Remove(index);
                    }
                    index = str.LastIndexOf(".png");
                    if (index > -1)
                    {
                        str = str.Remove(index);
                    }
                      index = str.IndexOf("www.");
                    if (index == 0)
                    {
                        str = str.Remove(0, 4);
                    }

                    str = Regex.Replace(str, @"\s+", " ");






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
                    else
                    {
                        Console.WriteLine($"Error in domain str: {str}");
                    }


                    if (domain != null && domain.Length > 0
                        && domain != "localhost"
                        && domain != "localhost.localdomain"
                        && domain != "broadcasthost"
                        && domain != "local"
                        && domain != "ip6-localnet"
                        && domain != "ip6-mcastprefix"
                        && domain != "ip6-allnodes"
                        && domain != "ip6-allrouters"
                        && domain != "ip6-allhosts"
                        && domain != "0.0.0.0"
                        && domain != "127.0.0.1"
                        && Regex.Match(domain[0].ToString(), "[a-zA-Z0-9]").Success




                        )
                    {
                        list.Add(domain);

                    }
                }
                return list;

            });
            
        }
    }
}
