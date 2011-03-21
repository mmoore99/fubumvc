using System.Linq;
using FubuValidation.Fields;
using NUnit.Framework;

namespace FubuValidation.Tests.Fields
{


    [TestFixture]
    public class when_evaluating_the_collection_length_rule
    {
        [Test]
        public void do_not_log_any_messages_when_the_collection_is_valid()
        {
            var target = new CollectionLengthTarget(){
                Names = new[]{"a"}
            };

            var rule = new CollectionLengthRule(1);
            rule.Validate(target, x => x.Names).MessagesFor<CollectionLengthTarget>(x => x.Names).Any().ShouldBeFalse();
        }

        [Test]
        public void log_message_when_collection_is_wrong_length()
        {
            var target = new CollectionLengthTarget(){
                Names = new string[0]
            };

            var rule = new CollectionLengthRule(1);
            rule.Validate(target, x => x.Names).MessagesFor<CollectionLengthTarget>(x => x.Names).Single().GetMessage()
                .ShouldEqual("Must be exactly 1 element(s)"); 
        }

        [Test]
        public void log_message_when_collection_is_null()
        {
            var target = new CollectionLengthTarget()
            {
                Names = null
            };

            var rule = new CollectionLengthRule(1);
            rule.Validate(target, x => x.Names).MessagesFor<CollectionLengthTarget>(x => x.Names).Single().GetMessage()
                .ShouldEqual("Must be exactly 1 element(s)");
        }


        [Test]
        public void log_message_when_collection_is_wrong_length_w()
        {
            var target = new CollectionLengthTarget()
            {
                Names = new string[] { "a"}
            };

            var rule = new CollectionLengthRule(2);
            rule.Validate(target, x => x.Names).MessagesFor<CollectionLengthTarget>(x => x.Names).Single().GetMessage()
                .ShouldEqual("Must be exactly 2 element(s)");
        }
    }
    
    public class CollectionLengthTarget
    {
        public string[] Names { get; set; }
    }

}