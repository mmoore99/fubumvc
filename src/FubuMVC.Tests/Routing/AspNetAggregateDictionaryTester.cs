using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Web;
using System.Web.Routing;
using FubuCore.Binding;
using FubuCore.Reflection;
using FubuMVC.Core.Http;
using FubuMVC.Core.Http.AspNet;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuMVC.Tests.Routing
{
    [TestFixture]
    public class AspNetAggregateDictionaryTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            callback = MockRepository.GenerateMock<IDictionaryCallback>();

            dict1 = new Dictionary<string, object>
            {
                {"a", 1},
                {"b", 2},
                {"c", 3}
            };

            dict2 = new Dictionary<string, object>
            {
                {"d", 4},
                {"e", 5},
                {"f", 6}
            };

            dict3 = new Dictionary<string, object>
            {
                {"g", 7},
                {"h", 8},
                {"i", 9}
            };

            aggregate = new AggregateDictionary();
            aggregate.AddLocator(RequestDataSource.Route.ToString(), key => dict1.ContainsKey(key) ? dict1[key] : null, () => dict1.Keys);
            aggregate.AddLocator(RequestDataSource.Request.ToString(), key => dict2.ContainsKey(key) ? dict2[key] : null, () => dict2.Keys);
            aggregate.AddLocator(RequestDataSource.Header.ToString(), key => dict3.ContainsKey(key) ? dict3[key] : null, () => dict3.Keys);
        }

        #endregion

        private IDictionaryCallback callback;
        private Dictionary<string, object> dict1;
        private Dictionary<string, object> dict2;
        private Dictionary<string, object> dict3;
        private AggregateDictionary aggregate;

        [Test]
        public void has_prefixed_value_positive()
        {
            dict3.Add("SiteName", "a");
            aggregate.HasAnyValuePrefixedWith("Site").ShouldBeTrue();

            dict2.Add("AddressName", "b");
            aggregate.HasAnyValuePrefixedWith("Address").ShouldBeTrue();

            dict1.Add("CaseName", "c");
            aggregate.HasAnyValuePrefixedWith("Case").ShouldBeTrue();
        }

        [Test]
        public void has_prefixed_value_negative()
        {
            aggregate.HasAnyValuePrefixedWith("Site").ShouldBeFalse();

            aggregate.HasAnyValuePrefixedWith("Address").ShouldBeFalse();

            aggregate.HasAnyValuePrefixedWith("Case").ShouldBeFalse();
        }

        private void forKey(string key)
        {
            aggregate.Value(key, callback.Callback);
        }

        private void assertFound(RequestDataSource source, object value)
        {
            callback.AssertWasCalled(x => x.Callback(source.ToString(), value));
        }

        [Test]
        public void find_value_from_second_dictionary()
        {
            forKey("d");
            assertFound(RequestDataSource.Request, 4);
        }

        [Test]
        public void find_value_from_third_dictionary()
        {
            forKey("i");
            assertFound(RequestDataSource.Header, 9);
        }

        [Test]
        public void find_value_from_top_dictionary()
        {
            forKey("b");
            assertFound(RequestDataSource.Route, 2);
        }

        [Test]
        public void find_value_when_it_is_not_there()
        {
            forKey("abc");

            callback.AssertWasNotCalled(x => x.Callback(RequestDataSource.Route.ToString(), null), x => x.IgnoreArguments());
        }

        [Test]
        public void find_value_from_request_property()
        {
            const string expectedValue = "STUBBED USERAGENT";
            var requestCtx = Do_the_Stupid_ASPNET_Mock_HokeyPokey();
            requestCtx.HttpContext.Request.Stub(r => r.UserAgent).Return(expectedValue);
            aggregate = new AspNetAggregateDictionary(requestCtx);

            forKey("UserAgent");

            assertFound(RequestDataSource.RequestProperty, expectedValue);
        }

        [Test]
        public void find_uri_value_from_request_property()
        {
            var expectedValue = new Uri("http://localhost/foo");
            var requestCtx = Do_the_Stupid_ASPNET_Mock_HokeyPokey();
            aggregate = new AspNetAggregateDictionary(requestCtx);

            forKey("Url");

            assertFound(RequestDataSource.RequestProperty, expectedValue);
        }

        [Test]
        public void find_value_from_request_property_of_added_aggregate()
        {
            const string expectedValue = "STUBBED USERAGENT";
            aggregate = new AggregateDictionary();

            aggregate.AddDictionary("Other", new Dictionary<string, object> { { "UserAgent", expectedValue } });
            forKey("UserAgent1");
            callback.AssertWasNotCalled(x => x.Callback(RequestDataSource.Other.ToString(), null), o => o.IgnoreArguments());
            
            forKey("UserAgent");
            assertFound(RequestDataSource.Other, expectedValue);
        }

        private RequestContext Do_the_Stupid_ASPNET_Mock_HokeyPokey()
        {
            var context = MockRepository.GenerateStub<HttpContextBase>();
            var request = MockRepository.GenerateStub<HttpRequestBase>();
            context.Stub(c => c.Request).Return(request);
            request.Stub(r => r.Files).Return(MockRepository.GenerateStub<HttpFileCollectionBase>());
            request.Stub(r => r.Headers).Return(new NameValueCollection());
            request.Stub(r => r.Url).Return(new Uri("http://localhost/foo"));
            request.Stub(r => r.UrlReferrer).Return(new Uri("http://localhost/login"));
            return new RequestContext(context, new RouteData());
        }

        private bool isSystemProperty<T>(Expression<Func<T, object>> expression)
        {
            var property = ReflectionHelper.GetProperty(expression);
            return AspNetAggregateDictionary.IsSystemProperty(property);
        }

        [Test]
        public void when_both_property_type_and_name_match()
        {
            isSystemProperty<SystemTarget>(x => x.AcceptTypes).ShouldBeTrue();
        }

        [Test]
        public void when_only_the_property_name_matches()
        {
            isSystemProperty<NonSystemTarget>(x => x.AcceptTypes).ShouldBeFalse();
        }
    }

    public class SystemTarget
    {
        public string[] AcceptTypes { get; set; }
        

    }

    public class NonSystemTarget
    {
        public bool AcceptTypes { get; set; }
    }

    public interface IDictionaryCallback
    {
        void Callback(string source, object value);
    }
}