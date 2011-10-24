using System.Linq;
using Bottles;
using FubuCore;
using FubuCore.Binding;
using FubuMVC.Core;
using FubuMVC.Core.Assets;
using FubuMVC.Core.Assets.Combination;
using FubuMVC.Core.Assets.Content;
using FubuMVC.Core.Assets.Files;
using FubuMVC.Core.Assets.Http;
using FubuMVC.Core.Assets.Tags;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Diagnostics;
using FubuMVC.Core.Http;
using FubuMVC.Core.Http.AspNet;
using FubuMVC.Core.Packaging;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Querying;
using FubuMVC.Core.Resources.Media;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Security;
using FubuMVC.Core.SessionState;
using FubuMVC.Core.UI;
using FubuMVC.Core.Urls;
using FubuMVC.Core.View.Activation;
using FubuMVC.Core.Web.Security;
using FubuTestingSupport;
using NUnit.Framework;
using IPackageFiles = FubuMVC.Core.Packaging.IPackageFiles;

namespace FubuMVC.Tests.Registration
{
    [TestFixture]
    public class default_fubu_registry_service_registrations
    {
        private void registeredTypeIs<TService, TImplementation>()
        {
            new FubuRegistry().BuildGraph().Services.DefaultServiceFor<TService>().Type.ShouldEqual(
                typeof (TImplementation));
        }

        [Test]
        public void default_binding_logger_is_nullo()
        {
            registeredTypeIs<IBindingLogger, NulloBindingLogger>();
        }

        [Test]
        public void BindingContext_is_registered()
        {
            registeredTypeIs<IBindingContext, BindingContext>();
        }

        [Test]
        public void ContentPipeline_is_registered()
        {
            registeredTypeIs<IContentPipeline, ContentPipeline>();
        }

        [Test]
        public void IAuthenticationContext_is_registered()
        {
            registeredTypeIs<IAuthenticationContext, WebAuthenticationContext>();
        }

        [Test]
        public void IFlash_is_registered()
        {
            registeredTypeIs<IFlash, FlashProvider>();
        }

        [Test]
        public void IObjectResolver_is_registered()
        {
            registeredTypeIs<IObjectResolver, ObjectResolver>();
        }

        [Test]
        public void IOutputWriter_is_registered()
        {
            registeredTypeIs<IOutputWriter, OutputWriter>();
        }

        [Test]
        public void IPartialFactory_is_registered()
        {
            registeredTypeIs<IPartialFactory, PartialFactory>();
        }

        [Test]
        public void IRequestDataProvider_is_registered()
        {
            registeredTypeIs<IRequestDataProvider, RequestDataProvider>();
        }

        [Test]
        public void IRequestData_is_registered()
        {
            registeredTypeIs<IRequestData, RequestData>();
        }

        [Test]
        public void IResponseCaching_is_registered()
        {
            registeredTypeIs<IResponseCaching, ResponseCaching>();
        }

        [Test]
        public void ISecurityContext_is_registered()
        {
            registeredTypeIs<ISecurityContext, WebSecurityContext>();
        }

        [Test]
        public void ImageWriter_is_configured()
        {
            registeredTypeIs<IImageWriter, ImageWriter>();
        }


        [Test]
        public void ValueConverterRegistry_is_registered()
        {
            registeredTypeIs<IValueConverterRegistry, ValueConverterRegistry>();
        }

        [Test]
        public void an_activator_for_PackageFileActivator_is_registered()
        {
            new FubuRegistry().BuildGraph().Services.ServicesFor<IActivator>()
                .Any(x => x.Type == typeof (PackageFileActivator)).ShouldBeTrue();
        }

        [Test]
        public void asset_combination_cache_is_registered_as_a_singleton()
        {
            registeredTypeIs<IAssetCombinationCache, AssetCombinationCache>();
            ServiceRegistry.ShouldBeSingleton(typeof (AssetCombinationCache)).ShouldBeTrue();
        }

        [Test]
        public void asset_dependency_finder_should_be_registered_as_a_singleton()
        {
            registeredTypeIs<IAssetDependencyFinder, AssetDependencyFinderCache>();
            ServiceRegistry.ShouldBeSingleton(typeof (AssetDependencyFinderCache))
                .ShouldBeTrue();
        }

        [Test]
        public void asset_graph_and_pipeline_activators_are_registered_in_the_correct_order()
        {
            var activators = new FubuRegistry().BuildGraph().Services.ServicesFor<IActivator>().ToList();

            activators.Any(x => x.Type == typeof (AssetGraphConfigurationActivator)).ShouldBeTrue();
            activators.Any(x => x.Type == typeof (AssetPipelineBuilderActivator)).ShouldBeTrue();
            activators.Any(x => x.Type == typeof (AssetDeclarationVerificationActivator)).ShouldBeTrue();
            activators.Any(x => x.Type == typeof (AssetPolicyActivator)).ShouldBeTrue();

            activators.RemoveAll(x => !x.Type.Namespace.Contains(typeof (AssetGraph).Namespace));

            activators[0].Type.ShouldEqual(typeof (AssetGraphConfigurationActivator));
            activators[1].Type.ShouldEqual(typeof (AssetPipelineBuilderActivator));
            activators[2].Type.ShouldEqual(typeof (AssetDeclarationVerificationActivator));
            activators[3].Type.ShouldEqual(typeof (MimetypeRegistrationActivator));
            activators[4].Type.ShouldEqual(typeof(AssetCombinationBuildingActivator));
            activators[5].Type.ShouldEqual(typeof (AssetPolicyActivator));
            
        }

        [Test]
        public void asset_pipeline_is_registered_as_both_IAssetPipeline_and_IAssetFileRegistration_as_the_same_instance()
        {
            var services = new FubuRegistry().BuildGraph().Services;
            var pipeline1 = services.DefaultServiceFor<IAssetPipeline>().Value.ShouldBeOfType<AssetPipeline>();
            var pipeline2 = services.DefaultServiceFor<IAssetFileRegistration>().Value.ShouldBeOfType<AssetPipeline>();

            pipeline1.ShouldNotBeNull();
            pipeline2.ShouldNotBeNull();

            pipeline1.ShouldBeTheSameAs(pipeline2);
        }

        [Test]
        public void asset_requirements_are_registered()
        {
            registeredTypeIs<IAssetRequirements, AssetRequirements>();
        }

        [Test]
        public void asset_tag_builder_is_registered()
        {
            registeredTypeIs<IAssetTagBuilder, AssetTagBuilder>();
        }

        [Test]
        public void asset_tag_plan_cache_is_registered_as_a_singleton()
        {
            registeredTypeIs<IAssetTagPlanCache, AssetTagPlanCache>();
            ServiceRegistry.ShouldBeSingleton(typeof (AssetTagPlanCache)).ShouldBeTrue();
        }

        [Test]
        public void asset_tag_planner_is_registered()
        {
            registeredTypeIs<IAssetTagPlanner, AssetTagPlanner>();
        }

        [Test]
        public void asset_tag_write_is_registered()
        {
            registeredTypeIs<IAssetTagWriter, AssetTagWriter>();
        }

        [Test]
        public void authorization_preview_service_is_registered()
        {
            registeredTypeIs<IAuthorizationPreviewService, AuthorizationPreviewService>();
        }

        [Test]
        public void by_default_the_missing_asset_handler_is_traceonle()
        {
            registeredTypeIs<IMissingAssetHandler, TraceOnlyMissingAssetHandler>();
        }

        [Test]
        public void chain_authorizor_is_registered()
        {
            registeredTypeIs<IChainAuthorizor, ChainAuthorizor>();
        }

        [Test]
        public void combination_determination_service_is_registered()
        {
            registeredTypeIs<ICombinationDeterminationService, CombinationDeterminationService>();
        }

        [Test]
        public void content_plan_cache_is_registered_as_a_singleton()
        {
            registeredTypeIs<IContentPlanCache, ContentPlanCache>();
            ServiceRegistry.ShouldBeSingleton(typeof (ContentPlanCache));
        }

        [Test]
        public void content_plan_executor_is_registered()
        {
            registeredTypeIs<IContentPlanExecutor, ContentPlanExecutor>();
        }

        [Test]
        public void content_planner_is_registered()
        {
            registeredTypeIs<IContentPlanner, ContentPlanner>();
        }

        [Test]
        public void default_authorization_failure_handler_is_registered()
        {
            registeredTypeIs<IAuthorizationFailureHandler, DefaultAuthorizationFailureHandler>();
        }

        [Test]
        public void default_chain_resolver_is_registered()
        {
            registeredTypeIs<IChainResolver, ChainResolver>();
        }

        [Test]
        public void default_endpoint_factory_is_registered()
        {
            registeredTypeIs<IEndPointAuthorizorFactory, EndPointAuthorizorFactory>();
        }

        [Test]
        public void default_json_reader_is_JavascriptDeserializer_flavor()
        {
            registeredTypeIs<IJsonReader, JavaScriptJsonReader>();
        }

        [Test]
        public void display_formatter_is_registered()
        {
            registeredTypeIs<IDisplayFormatter, DisplayFormatter>();
        }

        [Test]
        public void endpoint_service_is_registered()
        {
            registeredTypeIs<IEndpointService, EndpointService>();
        }

        [Test]
        public void file_system_is_registered()
        {
            registeredTypeIs<IFileSystem, FileSystem>();
        }

        [Test]
        public void icontent_writer_is_registered()
        {
            registeredTypeIs<IContentWriter, ContentWriter>();
        }

        [Test]
        public void iscriptwriter_is_registered_to_the_basic_writer()
        {
            registeredTypeIs<IAssetTagWriter, AssetTagWriter>();
        }

        [Test]
        public void model_binder_cache_is_registered()
        {
            registeredTypeIs<IModelBinderCache, ModelBinderCache>();
        }

        [Test]
        public void object_converter_is_registered()
        {
            registeredTypeIs<IObjectConverter, ObjectConverter>();
        }

        [Test]
        public void package_files_are_registered()
        {
            registeredTypeIs<IPackageFiles, PackageFilesCache>();
        }

        [Test]
        public void page_activation_rule_cache_is_registered()
        {
            registeredTypeIs<IPageActivationRules, PageActivationRuleCache>();
        }

        [Test]
        public void page_activator_is_registered()
        {
            registeredTypeIs<IPageActivator, PageActivator>();
        }

        [Test]
        public void partial_invoker_is_registered()
        {
            registeredTypeIs<IPartialInvoker, PartialInvoker>();
        }

        [Test]
        public void property_cache_is_registered()
        {
            registeredTypeIs<IPropertyBinderCache, PropertyBinderCache>();
        }

        [Test]
        public void request_history_cache_is_registered()
        {
            registeredTypeIs<IRequestHistoryCache, RequestHistoryCache>();
        }

        [Test]
        public void script_graph_is_registered()
        {
            new FubuRegistry().BuildGraph().Services.DefaultServiceFor<AssetGraph>().Value.ShouldNotBeNull();
        }

        [Test]
        public void should_be_a_script_configuration_activator_registered_as_a_service()
        {
            new FubuRegistry().BuildGraph().Services.ServicesFor<IActivator>()
                .Any(x => x.Type == typeof (AssetGraphConfigurationActivator)).ShouldBeTrue();
        }

        [Test]
        public void smart_request_is_registered_as_the_fubu_smart_request()
        {
            registeredTypeIs<ISmartRequest, FubuSmartRequest>();
        }

        [Test]
        public void transformer_library_is_registered()
        {
            registeredTypeIs<ITransformerPolicyLibrary, TransformerPolicyLibrary>();
        }

        [Test]
        public void url_registry_is_registered()
        {
            registeredTypeIs<IUrlRegistry, UrlRegistry>();
        }

        [Test]
        public void values_is_registered()
        {
            var services = new FubuRegistry().BuildGraph().Services;
            services.DefaultServiceFor(typeof (IValues<>)).Type.ShouldEqual(typeof (SimpleValues<>));
        }

        [Test]
        public void value_source_is_registered()
        {
            var services = new FubuRegistry().BuildGraph().Services;
            services.DefaultServiceFor(typeof(IValueSource<>)).Type.ShouldEqual(typeof(ValueSource<>));
        }

        [Test]
        public void setter_binder_is_registered()
        {
            registeredTypeIs<ISetterBinder, SetterBinder>();
        }

        [Test]
        public void IRequestHeader_is_registered()
        {
            registeredTypeIs<IRequestHeaders, RequestHeaders>();
        }
    }
}