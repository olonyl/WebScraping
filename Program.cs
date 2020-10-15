using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ScrapySharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using WebScraping.Models;

namespace WebScraping
{
    public class Program
    {
        static HtmlWeb _web = new HtmlWeb();
        static List<String> invokedURLs = new List<string>();
        static List<String> _imageList = new List<string>();

        static String imageContainerPath = @"C:\Lab\Smart-Apartment-Data\02 - WebScraping\WebScraping\Images\";
        static void Main(string[] args)
        {
            GetImagesRecursively();
        }
        private static void GetImagesRecursively()
        {
            String appURL = @"https://www.pandasecurity.com/spain/mediacenter/malware/hackers-mas-famosos-historia/";
            GetImages(appURL);
            Console.ReadLine();
        }
        private static void GetImagesBasedOnUrl()
        {
            String appURL = @"https://www.pandasecurity.com/spain/mediacenter/malware/hackers-mas-famosos-historia/";


            HtmlDocument _document = _web.Load(appURL);

            foreach (var image in _document.DocumentNode.CssSelect("img"))
            {
                var imageURL = image.GetAttributeValue("src");
                using (WebClient oClient = new WebClient())
                {
                    var fullPathImage = $"{imageContainerPath}{Guid.NewGuid()}.jpg";
                    try
                    {
                        oClient.DownloadFile(new Uri(imageURL), fullPathImage);
                        Console.WriteLine($"Imaged Downloaded: {imageURL}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"this image can't be gotten: {imageURL}");
                    }
                }
            }
            Console.ReadLine();
        }
        private static void GetImagesBasedOnSameDomain()
        {
            // String appURL = @"https://www.rottentomatoes.com/";
            String appURL = @"http://app.blognetcore.olonyl/";

            HtmlDocument _document = _web.Load(appURL);

            foreach (var image in _document.DocumentNode.CssSelect("img"))
            {
                var imageURL = image.GetAttributeValue("src");
                var fullURLImage = $"{appURL}{imageURL}";
                using (WebClient oClient = new WebClient())
                {
                    var fullPathImage = $"{imageContainerPath}{Guid.NewGuid()}.jpg";
                    try
                    {
                        oClient.DownloadFile(new Uri(fullURLImage), fullPathImage);
                        Console.WriteLine($"Imaged Downloaded: {fullURLImage}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"this image can't be gotten: {fullURLImage}");
                    }
                }
            }
            Console.ReadLine();
        }
        private static void SaveContentiIntoDB()
        {
            ApplicationContext context = new ApplicationContext();
            HtmlDocument _document = _web.Load("http://app.blognetcore.olonyl");

            var _body = _document.DocumentNode.CssSelect(".card").ToList();

            foreach (var _card in _body)
            {
                var title = _card.CssSelect(".text-center").First().InnerHtml;
                var time = _card.CssSelect("p").First().InnerHtml;
                var article = new Article
                {
                    Date = time,
                    Title = title
                };
                context.Article.Add(article);
            }
            context.SaveChanges();

        }
        #region Helpers
        private static void GetImages(string url, string tab = "")
        {
            try
            {
                HtmlDocument _document = _web.Load(url);
                foreach (var anchors in _document.DocumentNode.CssSelect("a"))
                {
                    var anchorURL = anchors.GetAttributeValue("href");

                    var exists = invokedURLs.Find(f => f == anchorURL) != null;

                    if (!exists && anchorURL.StartsWith(@"https://www.pandasecurity.com"))
                    {
                        foreach (var node in _document.DocumentNode.CssSelect("img"))
                        {
                            SaveImagesLocaly(node.GetAttributeValue("src"));
                        }
                        invokedURLs.Add(anchorURL);
                        Console.WriteLine(tab + anchorURL);
                        GetImages(anchorURL, tab + "|");


                    }
                }
            }
            catch (Exception ex) { }

        }
        static private void SaveImagesLocaly(string imageURL)
        {
            try
            {

                var splitImageUrl = imageURL.Split("/");
                var imageName = splitImageUrl[splitImageUrl.Length - 1];
                var fullPathImage = $"{imageContainerPath}{imageName}";

                if (_imageList.Find(f => f == imageName) != null)
                    return;

                using (WebClient oClient = new WebClient())
                {

                    oClient.DownloadFile(new Uri(imageURL), fullPathImage);

                    _imageList.Add(imageName);

                    Console.WriteLine($"Image saved loacaly: {imageURL}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"this image can't be gotten: {imageURL}");
            }
        }
        #endregion
    }
}
