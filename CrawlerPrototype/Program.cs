using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Threading;

namespace CrawlerPrototype
{
    class Program
    {
        static List<string> VisitedLinks = new List<string>();
        static List<string> FoundLinks = new List<string>();
        static List<string> FoundLinksTemp = new List<string>();

        static List<string> ImageLinks = new List<string>();
        static List<string> CSSLinks = new List<string>();
        static List<string> JSLinks = new List<string>();
        static List<string> Analyzed = new List<string>();

        static List<string> AnalyzedTemp = new List<string>();
        static List<string> ImageLinksTemp = new List<string>();
        static List<string> CSSLinksTemp = new List<string>();
        static List<string> JSLinksTemp = new List<string>();

        static List<string> Errors = new List<string>();

        static string source = "http://www.google.com";
        static string mainSource = "";
        static string dlpath = "";
        static Uri mainUri;

        static int visited = 0;
        static int downloaded = 0;

        static string ParseUrl(string value,Uri url)
        {
            string EndVal = "";
            try
            {
                if (value.Contains("?") || value.Contains("<") || value.Contains(">") || value.Contains(":") || value.Contains(";"))
                {
                    EndVal = "";
                }
                else if (value.Substring(0, 1) == "/" && value.Substring(0, 2) != "//")
                {
                    EndVal = url.Scheme + "://" + url.Host + Path.GetDirectoryName(url.AbsolutePath) + value;
                }
                else if (value.Substring(0, 2) == "//")
                {
                    EndVal = url.Scheme + ":" + value + Path.GetDirectoryName(url.AbsolutePath);
                }
                else if (value.Substring(0, 1) == "#" || value.Contains("#"))
                {
                    EndVal = "";
                }
                //else if (value.Substring(0, 11) == "javascript:")
                //{
                //    EndVal = "";
                //}
                //else if (value.Substring(0, 7) == "mailto:")
                //{
                //    EndVal = "";
                //}
                //else if (value.Substring(0, 4) == "tel:")
                //{
                //    EndVal = "";
                //}
                else if (value.Substring(0, 6) == "../../")
                {
                    value = value.Substring(6);
                    EndVal = url.Scheme + "://" + url.Host + Path.GetFullPath(Path.Combine(url.AbsolutePath, @"..\..\..\")).Substring(2) + value;
                }
                else if (value.Substring(0, 2) == "./")
                {
                    value = value.Substring(1);
                    EndVal = url.Scheme + "://" + url.Host + Path.GetDirectoryName(url.AbsolutePath) + value;
                }
                else if (value.Substring(0, 3) == "../")
                {
                    value = value.Substring(3);
                    EndVal = url.Scheme + "://" + url.Host + Path.GetFullPath(Path.Combine(url.AbsolutePath, @"..\..\")).Substring(2) + value;
                }
                else if (value.Substring(0, 5) != "https" && value.Substring(0, 4) != "http")
                {
                    EndVal = url.Scheme + "://" + url.Host + Path.GetDirectoryName(url.AbsolutePath) + "/" + value;
                }
                else
                {
                    EndVal = value;
                }
            }
            catch (ArgumentOutOfRangeException ex) { Errors.Add(ex.Message); }
            EndVal = EndVal.Replace("\\", "/");
            return EndVal;
        }

        static void LogErrors()
        {
            using (TextWriter tw = new StreamWriter(mainUri.Host + "/Errors.txt"))
            {
                foreach (string s in Errors) tw.WriteLine(s);
            }
        }

        public static void DownloadLinks()
        {
            foreach (string EndVal in FoundLinks)
            {
                try
                {
                    Uri url = new Uri(EndVal);
                    string sourcehtml = HttpMethods.Get(EndVal, EndVal, ref HttpMethods.cookies);

                    Uri uriResult;
                    bool result = Uri.TryCreate(EndVal, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                    string lastSegment = url.Segments[url.Segments.Length - 1];
                    string FileName;
                    if (lastSegment.Substring(lastSegment.Length - 1) == "/")
                    {
                        FileName = "default-down.html";
                    }
                    else
                    {
                        FileName = lastSegment;
                    }
                    try
                    {
                        if (EndVal.Trim() != string.Empty && result)
                        {
                            if (mainUri.Host == url.Host)
                            {
                                if (!Directory.Exists(dlpath + url.Host)) Directory.CreateDirectory(dlpath + url.Host);
                                if (!Directory.Exists(dlpath + url.Host + Path.GetDirectoryName(url.AbsolutePath))) Directory.CreateDirectory(dlpath + url.Host + Path.GetDirectoryName(url.AbsolutePath));
                                //File.WriteAllText(url.Host + Path.GetDirectoryName(url.AbsolutePath) + "\\" + FileName, sourcehtml);
                                if (!File.Exists(dlpath + url.Host + Path.GetDirectoryName(url.AbsolutePath) + "\\" + FileName))
                                {
                                    WebClient client = new WebClient();
                                    client.DownloadFile(url, dlpath + url.Host + Path.GetDirectoryName(url.AbsolutePath) + "\\" + FileName);
                                }
                            }
                        }
                    }
                    catch (Exception ex) { Errors.Add(ex.Message); }
                    downloaded++;
                    try
                    {
                        VisitedLinks.Add(url.ToString());
                    }
                    catch (Exception ex) { Errors.Add(ex.Message); }
                    int toplamdosya = FoundLinks.Count + ImageLinks.Count + CSSLinks.Count + JSLinks.Count + Analyzed.Count;
                    Console.Title = string.Format("İndiriliyor... ({0} dosyadan {1} tanesi indirildi)", toplamdosya, downloaded);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("İndirildi: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(url);
                }
                catch (NullReferenceException ex) { Errors.Add(ex.Message); }
                catch (ArgumentOutOfRangeException ex) { Errors.Add(ex.Message); }
                catch (Exception ex) { Errors.Add(ex.Message); }
            }
        }

        public static void DownloadFile(string link, Uri url)
        {
            try
            {
                Uri uriResult;
                bool result = Uri.TryCreate(link, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                string lastSegment = url.Segments[url.Segments.Length - 1];
                string FileName;
                if (lastSegment.Substring(lastSegment.Length - 1) == "/")
                {
                    FileName = "default-down.asset";
                }
                else
                {
                    FileName = lastSegment;
                }
                try
                {
                    if (link.Trim() != string.Empty && result)
                    {
                        if (uriResult.Host == url.Host)
                        {
                            if (!Directory.Exists(dlpath + url.Host)) Directory.CreateDirectory(dlpath + url.Host);
                            if (!Directory.Exists(dlpath + url.Host + Path.GetDirectoryName(url.AbsolutePath))) Directory.CreateDirectory(dlpath + url.Host + Path.GetDirectoryName(url.AbsolutePath));
                            if (!File.Exists(dlpath + url.Host + Path.GetDirectoryName(url.AbsolutePath) + "\\" + FileName))
                            {
                                WebClient client = new WebClient();
                                client.DownloadFile(url, dlpath + url.Host + Path.GetDirectoryName(url.AbsolutePath) + "\\" + FileName);
                            }
                        }
                    }
                }
                catch (Exception ex) { Errors.Add(ex.Message); }
                downloaded++;
                try
                {
                    VisitedLinks.Add(url.ToString());
                }
                catch (Exception ex) { Errors.Add(ex.Message); }
                int toplamdosya = FoundLinks.Count + ImageLinks.Count + CSSLinks.Count + JSLinks.Count + Analyzed.Count;
                Console.Title = string.Format("İndiriliyor... ({0} dosyadan {1} tanesi indirildi)", toplamdosya, downloaded);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("İndirildi: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(url);
            }
            catch (Exception ex) { Errors.Add(ex.Message); }
        }

        public static void Crawl(string source)
        {
            try
            {
                Uri URL;
                string html;
                HtmlAgilityPack.HtmlDocument DOC;
                HtmlNodeCollection Links;
                HtmlNodeCollection Images;
                HtmlNodeCollection CSS;
                HtmlNodeCollection JS;

                URL = new Uri(source);
                html = HttpMethods.Get(source, source, ref HttpMethods.cookies);
                DOC = new HtmlAgilityPack.HtmlDocument();
                DOC.LoadHtml(html);

                CSS = DOC.DocumentNode.SelectNodes("//link");
                Links = DOC.DocumentNode.SelectNodes("//a");
                Images = DOC.DocumentNode.SelectNodes("//img");
                JS = DOC.DocumentNode.SelectNodes("//script");

                Console.Title = source;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Araştırılıyor: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(source);
                foreach (HtmlNode item in Links)
                {
                    try
                    {
                        Uri h = new Uri(ParseUrl(item.Attributes["href"].Value, URL));
                        if (mainUri.Host == h.Host)
                        {
                            FoundLinksTemp.Add(ParseUrl(item.Attributes["href"].Value, URL));
                        }
                    }
                    catch (Exception ex) { Errors.Add(ex.Message); }
                }
                foreach (HtmlNode item in Images)
                {
                    try
                    {
                        ImageLinksTemp.Add(ParseUrl(item.Attributes["src"].Value, URL));
                    }
                    catch (Exception ex) { Errors.Add(ex.Message); }
                }
                foreach (HtmlNode item in CSS)
                {
                    try
                    {
                        CSSLinksTemp.Add(ParseUrl(item.Attributes["href"].Value, URL));
                    }
                    catch (Exception ex) { Errors.Add(ex.Message); }
                }
                foreach (HtmlNode item in JS)
                {
                    try
                    {
                        JSLinksTemp.Add(ParseUrl(item.Attributes["src"].Value, URL));
                    }
                    catch (Exception ex) { Errors.Add(ex.Message); }
                }


                string Title = DOC.DocumentNode.SelectSingleNode("/html/head/title").InnerHtml;
                if (!Directory.Exists(URL.Host)) Directory.CreateDirectory(URL.Host);
                if (!File.Exists(URL.Host + "/ekvator.json")) File.WriteAllText(URL.Host + "/ekvator.json", "{\n\t\"siteadi\": \"" + Title + "\",\n\t\"sitekategori\": \"\",\n\t\"siteimage\": \"\",\n\t\"siteaciklama\": \"\",\n\t\"url\": \"" + source + "\"\n}");

                FoundLinks = FoundLinksTemp.Distinct().ToList();
            }
            catch (Exception ex) { Errors.Add(ex.Message); }
        }

        public static void AnalyzeCSSFile(string src, Uri url)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Analiz ediliyor: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(src);
                string pagesrc = HttpMethods.Get(src, src, ref HttpMethods.cookies);
                List<string> temp = new List<string>();

                pagesrc = pagesrc.Replace("\"", "");
                pagesrc = pagesrc.Replace("'", "");

                while (pagesrc.Contains("url("))
                {
                    try
                    {
                        int startIndex = pagesrc.IndexOf("url(") + "url(".Length;
                        pagesrc = pagesrc.Substring(startIndex);
                        int endIndex = pagesrc.IndexOf(")");
                        temp.Add(pagesrc.Substring(0, endIndex));
                    }
                    catch (Exception ex)
                    {
                        Errors.Add(ex.Message);
                    }
                }
                foreach (string item in temp)
                {
                    try
                    {
                        if (!item.Contains("\"") || !item.Contains("'"))
                        {
                            AnalyzedTemp.Add(ParseUrl(item, url));
                        }
                    }
                    catch (Exception ex)
                    {
                        Errors.Add(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Errors.Add(ex.Message);
            }
        }

        public static void Threader()
        {
            for (int i = 0; i < FoundLinks.Count; i++)
            {
                visited = i;
                Crawl(FoundLinks[i]);
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "Fetchy Web 'by' Berke Alp Çalışkan";

            try
            {
                source = args[0]; // Hedef siteyi parametreden al
                dlpath = args[1]; // İndirme konumu
            }
            catch (Exception)
            {
                Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Fetchy Web"));
                Console.Write("\nHedef Site -> ");
                source = Console.ReadLine();
            }
            finally
            {
                mainSource = source;
                mainUri = new Uri(mainSource);
            }

            Console.Clear();
            DateTime start = DateTime.Now; // İşlemin başlama zamanını tut

            Crawl(source); // İlk adım olarak girilen sayfadaki linkleri al
            Threader(); // İlk adımdan alınan verilerle bulunan tüm linklere gir ve oradan da linkler topla

            // Listeleri düzenle yinelenen linkleri kaldır
            ImageLinks = ImageLinksTemp.Distinct().ToList();
            CSSLinks = CSSLinksTemp.Distinct().ToList();
            JSLinks = JSLinksTemp.Distinct().ToList();
            
            Console.Title = "CSS Dosyaları Analiz Ediliyor...";
            foreach (string item in CSSLinks)
            {
                try
                {
                    Uri urle = new Uri(item);
                    AnalyzeCSSFile(item, urle);
                }
                catch (Exception ex) { Errors.Add(ex.Message); }
            } // CSS linklerini gezer ve içindeki linkleri de listeye ekler
            Analyzed = AnalyzedTemp.Distinct().ToList();

            // URL hatasından kaçınmak için '\' işaretini '/' ile değiştir
            for (int i = 0; i < ImageLinks.Count; i++) ImageLinks[i] = ImageLinks[i].Replace("\\", "/");
            for (int i = 0; i < CSSLinks.Count; i++) CSSLinks[i] = CSSLinks[i].Replace("\\", "/");
            for (int i = 0; i < JSLinks.Count; i++) JSLinks[i] = JSLinks[i].Replace("\\", "/");
            for (int i = 0; i < FoundLinks.Count; i++) FoundLinks[i] = FoundLinks[i].Replace("\\", "/");
            for (int i = 0; i < Analyzed.Count; i++) Analyzed[i] = Analyzed[i].Replace("\\", "/");

            Console.Clear();
            Console.Title = "İndiriliyor...";

            

            DownloadLinks(); // Bulunan tüm sayfaları indir


            // ** Geri kalan resim, stil ve script dosyalarını indir
            Uri url;
            foreach (string item in ImageLinks)
            {
                try
                {
                    url = new Uri(item);
                    DownloadFile(item, url);
                }
                catch (Exception ex) { Errors.Add(ex.Message); }
            }
            foreach (string item in Analyzed)
            {
                try
                {
                    url = new Uri(item);
                    DownloadFile(item, url);
                }
                catch (Exception ex) { Errors.Add(ex.Message); }
            }
            foreach (string item in CSSLinks)
            {
                try
                {
                    url = new Uri(item);
                    DownloadFile(item, url);
                }
                catch (Exception ex) { Errors.Add(ex.Message); }
            }
            foreach (string item in JSLinks)
            {
                try
                {
                    url = new Uri(item);
                    DownloadFile(item, url);
                }
                catch (Exception ex) { Errors.Add(ex.Message); }
            }

            TimeSpan gecenSure = DateTime.Now - start; // İşlemin aldığı zamanı hesapla
            using (TextWriter tw = new StreamWriter(mainUri.Host + "/FoundLinks.txt"))
            {
                foreach (String s in FoundLinks) tw.WriteLine(s);
            }
            using (TextWriter tw = new StreamWriter(mainUri.Host + "/DownloadedFiles.txt"))
            {
                foreach (string s in VisitedLinks) tw.WriteLine(s);
            }

            LogErrors(); // Son olarak bulunan hatalar dosyaya yazılır

            Console.WriteLine("\n========================================\n");
            Console.Title = "İşlem Bitti! " + mainUri.Host;
            Console.WriteLine("İndirme işlemi tamamlandı!");
            Console.WriteLine("{0} sayfa gezildi ve {1} dosya indirildi.", visited, downloaded);
            Console.WriteLine("İşlem {0} gün, {1} saat, {2} dakika, {3} saniyede tamamlandı.", gecenSure.Days, gecenSure.Hours, gecenSure.Minutes, gecenSure.Seconds);
            Console.ReadKey();
        }
    }
}