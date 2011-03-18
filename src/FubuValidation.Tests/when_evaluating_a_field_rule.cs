using System.Collections.Specialized;
using FubuCore;
using FubuCore.Reflection;
using FubuLocalization;
using FubuValidation.Strategies;
using FubuValidation.Tests.Models;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuValidation.Tests
{
    [TestFixture]
    public class when_evaluating_a_field_rule : InteractionContext<FieldRule>
    {
        private AddressModel _model;
        private Notification _notification;
        private string _rawValue;
        private StringToken _token;
        private ValidationContext _context;
        private ValidationStrategyContext _strategyContext;

        protected override void beforeEach()
        {
            _model = new AddressModel();
            _notification = new Notification();
            _rawValue = "Test";
            _token = StringToken.FromKeyString("test-key", "Test message");
            _context = new ValidationContext(null, null, null);
            _strategyContext = new ValidationStrategyContext(_model, _rawValue, typeof(AddressModel), null, _notification);

            MockFor<ITypeResolver>()
                .Expect(resolver => resolver.ResolveType(_model))
                .Return(typeof(AddressModel));

            MockFor<Accessor>()
                .Expect(accessor => accessor.GetValue(_model))
                .Return(_rawValue);

            Container.Configure(x => x.For<StringToken>().Use(() => _token));
        }

        [Test]
        public void should_delegate_to_field_strategy()
        {
            MockFor<IFieldValidationStrategy>()
                .Expect(strategy => strategy.Validate(_strategyContext))
                .Return(ValidationStrategyResult.Valid());

            ClassUnderTest.Validate(_model, _context, _notification);

            VerifyCallsFor<IFieldValidationStrategy>();
        }

        [Test]
        public void should_not_set_substitution_variables_if_validation_passes()
        {
            MockFor<IFieldValidationStrategy>()
                .Expect(strategy => strategy.Validate(_strategyContext))
                .Return(ValidationStrategyResult.Valid());

            ClassUnderTest.Validate(_model, _context, _notification);

            MockFor<IFieldValidationStrategy>()
                .AssertWasNotCalled(s => s.GetMessageSubstitutions(null), options => options.IgnoreArguments());
        }

        [Test]
        public void should_set_substitution_variables_if_validation_fails()
        {
            var message = new NotificationMessage(_token);

            MockFor<IFieldValidationStrategy>()
                .Expect(strategy => strategy.Validate(_strategyContext))
                .Return(ValidationStrategyResult.Invalid(message));

            MockFor<IFieldValidationStrategy>()
                .Expect(strategy => strategy.GetMessageSubstitutions(MockFor<Accessor>()))
                .Return(new NameValueCollection { {RequiredFieldStrategy.FIELD, "test"} });

            ClassUnderTest
                .Validate(_model, _context, _notification);

            message
                .MessageSubstitutions
                .ShouldContain(k => k.Key == RequiredFieldStrategy.FIELD && k.Value == "test");
        }

        [Test]
        public void should_register_message_if_validation_fails()
        {
            MockFor<IFieldValidationStrategy>()
                .Expect(strategy => strategy.Validate(_strategyContext))
                .Return(ValidationStrategyResult.Invalid(new NotificationMessage(StringToken.FromKeyString("key"))));

            MockFor<IFieldValidationStrategy>()
                .Expect(strategy => strategy.GetMessageSubstitutions(MockFor<Accessor>()))
                .Return(new NameValueCollection());

            ClassUnderTest
                .Validate(_model, _context, _notification);

            _notification
                .MessagesFor(MockFor<Accessor>())
                .Messages
                .ShouldHaveCount(1);
        }
    }
}