using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FubuMVC.Core.Assets.Http;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Urls;
using OpenQA.Selenium;
using Serenity.Endpoints;

namespace Serenity
{
    public class ApplicationDriver
    {
        private readonly IApplicationUnderTest _application;

        public ApplicationDriver(IApplicationUnderTest application)
        {
            _application = application;
        }

        public void NavigateTo(object target)
        {
            var url = _application.Urls.UrlFor(target);
            _application.Driver.Navigate().GoToUrl(url);
        }

        public void NavigateTo<T>(Expression<Action<T>> expression)
        {
            var url = _application.Urls.UrlFor(expression);
            _application.Driver.Navigate().GoToUrl(url);
        }

        public ScreenDriver GetCurrentScreen()
        {
            return new ScreenDriver(_application.Driver);
        }

        public EndpointDriver GetEndpointDriver()
        {
            return new EndpointDriver(_application.Urls);
        }

        public string AssetUrlFor(string file)
        {
            return _application.RootUrl + ("/_content/" + file).Replace("//", "/");
        }

        public void NavigateToHome()
        {
            _application.Driver.Navigate().GoToUrl(_application.RootUrl);
        }
    }

    public class AssetTagsState
    {
        public IList<IWebElement> Scripts { get; set; }
        public IList<IWebElement> Styles { get; set; }
    }

    public class ScreenDriver
    {
        private readonly IWebDriver _browser;
        private readonly Lazy<IWebElement> _head;

        public ScreenDriver(IWebDriver browser)
        {
            _browser = browser;
            _head = new Lazy<IWebElement>(() => _browser.FindElement(By.TagName("head")));
        }

        private IWebElement head
        {
            get { return _head.Value; }
        }

        private IWebElement elementFor(string name)
        {
            return _browser.FindElement(By.Id(name)) ?? _browser.FindElement(By.Name(name));
        }

        public AssetTagsState GetAssetDeclarationsFromTheHead()
        {
            return new AssetTagsState
            {
                Scripts = head.FindElements(By.TagName("script")).ToList(),
                Styles = head.FindElements(By.TagName("link")).Where(x => x.IsCssLink()).ToList()
            };
        }

        public AssetTagsState GetAssetDeclarations()
        {
            return new AssetTagsState
            {
                Scripts = _browser.FindElements(By.TagName("script")).ToList(),
                Styles = _browser.FindElements(By.TagName("link")).Where(x => x.IsCssLink()).ToList()
            };
        }
    }

    public static class WebElementExtensions
    {
        public static bool IsCssLink(this IWebElement element)
        {
            return element.TagName == "link" &&
                   element.GetMimeType() == MimeType.Css;
        }

        public static string Href(this IWebElement element)
        {
            return element.GetAttribute("href");
        }

        public static string AssetName(this IWebElement element)
        {
            var parts = (element.Href() ?? element.GetAttribute("src")).Split('/').ToList();
            var index = parts.IndexOf(UrlRegistry.AssetsUrlFolder);

            return parts.Skip(index).Join("/");
        }

        public static MimeType GetMimeType(this IWebElement element)
        {
            return MimeType.MimeTypeByFileName(element.Href());
        }
    }
}