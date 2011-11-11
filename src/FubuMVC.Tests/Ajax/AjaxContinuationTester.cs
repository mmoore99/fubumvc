using FubuCore;
using FubuLocalization;
using FubuMVC.Core.Ajax;
using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;

namespace FubuMVC.Tests.Ajax
{
    [TestFixture]
    public class AjaxContinuationTester
    {
        private AjaxContinuation theContinuation;

        [SetUp]
        public void SetUp()
        {
            theContinuation = new AjaxContinuation();
        }

        [Test]
        public void success_is_placed_into_the_dictionary()
        {
            theContinuation.Success = false;
            theContinuation.ToDictionary()["success"].As<bool>().ShouldBeFalse();

            theContinuation.Success = true;
            theContinuation.ToDictionary()["success"].As<bool>().ShouldBeTrue();
        }

        [Test]
        public void refresh_is_placed_int_the_dictionary()
        {
            theContinuation.ShouldRefresh = false;
            theContinuation.ToDictionary()["refresh"].As<bool>().ShouldBeFalse();

            theContinuation.ShouldRefresh = true;
            theContinuation.ToDictionary()["refresh"].As<bool>().ShouldBeTrue();
        }

        [Test]
        public void message_is_placed_into_the_dictionary_if_it_exists()
        {
            theContinuation.ToDictionary().ContainsKey("message").ShouldBeFalse();

            theContinuation.Message = "something";

            theContinuation.ToDictionary()["message"].ShouldEqual("something");
        }

        [Test]
        public void errors_are_only_written_to_the_dictionary_if_they_exist()
        {
            theContinuation.ToDictionary().ContainsKey("errors").ShouldBeFalse();

            theContinuation.Errors.Add(new AjaxError(){message = "bad!"});

            theContinuation.ToDictionary()["errors"].ShouldBeOfType<AjaxError[]>()
                .Single().message.ShouldEqual("bad!");
        }

        [Test]
        public void Successful_builder_method()
        {
            var success = AjaxContinuation.Successful();
        
            success.Errors.Any().ShouldBeFalse();
            success.Success.ShouldBeTrue();
        }

        [Test]
        public void ForMessage_builder_method()
        {
            var continuation = AjaxContinuation.ForMessage("some message");

            continuation.Success.ShouldBeFalse();
            continuation.Message.ShouldEqual("some message");
        }

        [Test]
        public void ForMessage_via_StringToken()
        {
            var token = StringToken.FromKeyString("key", "some message");

            var continuation = AjaxContinuation.ForMessage(token);

            continuation.Success.ShouldBeFalse();
            continuation.Message.ShouldEqual(token.ToString());
        }
    }
}