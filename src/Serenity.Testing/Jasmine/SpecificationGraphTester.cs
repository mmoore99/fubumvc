using System.Diagnostics;
using Bottles.Diagnostics;
using FubuMVC.Core.Assets;
using FubuMVC.Core.Assets.Files;
using NUnit.Framework;
using Serenity.Jasmine;
using System.Linq;
using FubuTestingSupport;
using System.Collections.Generic;

namespace Serenity.Testing.Jasmine
{
    [TestFixture]
    public class SpecificationGraphTester
    {
        private AssetPipeline thePipeline;
        private AssetFileDataMother theFiles;

        [SetUp]
        public void SetUp()
        {
            thePipeline = new AssetPipeline();
            theFiles = new AssetFileDataMother(thePipeline.AddFile);
        }

        [Test]
        public void finds_all_the_specs_and_puts_in_folder_structure()
        {
            theFiles.LoadAssets(@"
scripts/lib1.js
scripts/lib2.js
scripts/lib3.js
scripts/specs/something.js
scripts/specs/lib1.spec.js
scripts/specs/lib2.spec.js
scripts/specs/lib3.spec.js
pak1:scripts/specs/lib4.spec.js
pak1:scripts/specs/lib5.spec.js
pak1:scripts/specs/lib6.spec.js
pak1:scripts/f1/specs/lib7.spec.js
pak2:scripts/specs/f1/lib8.spec.js
");

            var graph = new SpecificationGraph(thePipeline);
            graph.AllSpecifications.Select(x => x.File.Name).Each(x => Debug.WriteLine(x));
            graph.AllSpecifications.Select(x => x.File.Name)
                .ShouldHaveTheSameElementsAs(
"specs/something.js",
"specs/lib1.spec.js",
"specs/lib2.spec.js",
"specs/lib3.spec.js",
"f1/specs/lib7.spec.js",
"specs/lib4.spec.js",

"specs/lib5.spec.js",
"specs/lib6.spec.js",

"specs/f1/lib8.spec.js"
                );

        }

        [Test]
        public void makes_asset_graph_dependencies_between_files()
        {
            theFiles.LoadAssets(@"
scripts/lib1.js
scripts/lib2.js
scripts/lib3.js
scripts/specs/something.js
scripts/specs/lib1.spec.js
scripts/specs/lib2.spec.js
scripts/specs/lib3.spec.js
");


            var graph = new SpecificationGraph(thePipeline);

            graph.FindSpecByFullName("specs/lib1.spec.js").Libraries.Select(x => x.Name)
                .ShouldHaveTheSameElementsAs("lib1.js");


            graph.FindSpecByFullName("specs/lib2.spec.js").Libraries.Select(x => x.Name)
                .ShouldHaveTheSameElementsAs("lib2.js");


            graph.FindSpecByFullName("specs/lib3.spec.js").Libraries.Select(x => x.Name)
                .ShouldHaveTheSameElementsAs("lib3.js");

            graph.FindSpecByFullName("specs/something.js").Libraries.Any().ShouldBeFalse();

        }
    }
}