using System.Collections.Generic;
using FubuMVC.Core.Http;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuMVC.Tests.Http
{
    [TestFixture]
    public class CurrentChainTester
    {
        private BehaviorChain theChain;
        private IDictionary<string, object> theRouteData;
        private Dictionary<string, object> someDifferentRouteData;
        private BehaviorChain theSecondChain;
        private BehaviorChain theThirdChain;

        [SetUp]
        public void SetUp()
        {
            theChain = new BehaviorChain(){
                Route = new RouteDefinition("some/pattern/url")
            };

            theSecondChain = new BehaviorChain()
            {
                Route = new RouteDefinition("some/other/pattern")
            };

            theThirdChain = new BehaviorChain()
            {
                Route = new RouteDefinition("yet/another/pattern")
            };

            theRouteData = new Dictionary<string, object>{
                {"A", "1"},
                {"B", "2"},
                {"C", "3"}
            };

            someDifferentRouteData = new Dictionary<string, object>{
                {"A", "1"},
                {"B", "2"},
                {"C", "33"}
            };


        }

        [Test]
        public void the_initial_state_points_to_the_top_chain()
        {
            new CurrentChain(theChain, theRouteData).Current.ShouldBeTheSameAs(theChain);
        }

        [Test]
        public void the_top_chain_is_always_the_originating_chain()
        {
            var currentChain = new CurrentChain(theChain, theRouteData);
            currentChain.OriginatingChain.ShouldBeTheSameAs(theChain);

            currentChain.Push(theSecondChain);
            currentChain.OriginatingChain.ShouldBeTheSameAs(theChain);

            currentChain.Pop();

            currentChain.OriginatingChain.ShouldBeTheSameAs(theChain);
        }

        [Test]
        public void push_and_pop_track_the_current_chain()
        {
            var currentChain = new CurrentChain(theChain, theRouteData);
            currentChain.Push(theSecondChain);

            currentChain.Current.ShouldBeTheSameAs(theSecondChain);

            currentChain.Push(theThirdChain);
            currentChain.Current.ShouldBeTheSameAs(theThirdChain);

            currentChain.Pop();
            currentChain.Current.ShouldBeTheSameAs(theSecondChain);

            currentChain.Pop();
            currentChain.Current.ShouldBeTheSameAs(theChain);
        }

        [Test]
        public void the_resource_hash_is_deterministic_by_route_parameters()
        {
            var hash1 = new CurrentChain(theChain, theRouteData).ResourceHash();
            var hash2 = new CurrentChain(theChain, theRouteData).ResourceHash();

            hash1.ShouldEqual(hash2);

            var hash3 = new CurrentChain(theChain, someDifferentRouteData).ResourceHash();

            hash1.ShouldNotEqual(hash3);
        }

        [Test]
        public void the_resource_hash_is_deterministic_by_pattern()
        {
            var hash1 = new CurrentChain(theChain, theRouteData).ResourceHash();
            var hash2 = new CurrentChain(theSecondChain, theRouteData).ResourceHash();

            hash1.ShouldNotEqual(hash2);
        }
    }
}