﻿using System;
using System.IO;
using Bottles.Exploding;
using Bottles.Zipping;
using Fubu;
using Fubu.Templating;
using Fubu.Templating.Steps;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;

namespace FubuMVC.Tests.Templating
{
    [TestFixture]
    public class NewCommandEndToEndTester
    {
        private NewCommand _command;
        private FileSystem _fileSystem;
        private ZipFileService _zipService;
        private NewCommandInput _commandInput;

        [SetUp]
        public void before_each()
        {
            _command = new NewCommand();
            _fileSystem = new FileSystem();
            _zipService = new ZipFileService(_fileSystem);
            _commandInput = new NewCommandInput();
        }

        [Test, Explicit]
        public void clone_git_repo_and_append_to_existing_solution()
        {
            var tmpDir = FileSystem.Combine("Templating", Guid.NewGuid().ToString());
            var repoZip = FileSystem.Combine("Templating", "repo.zip");
            _zipService.ExtractTo(repoZip, tmpDir, ExplodeOptions.DeleteDestination);

            var solutionFile = FileSystem.Combine("Templating", "sample", "myproject.txt");
            var oldContents = _fileSystem.ReadStringFromFile(solutionFile);
            var solutionDir = _fileSystem.GetDirectory(solutionFile);

            _commandInput.GitFlag = "file:///{0}".ToFormat(_fileSystem.GetFullPath(tmpDir).Replace("\\", "/"));
            _commandInput.ProjectName = "MyProject";
            _commandInput.SolutionFlag = solutionFile;
            _commandInput.OutputFlag = solutionDir;
            _commandInput.RakeFlag = "init.rb";

            _command
                .Execute(_commandInput)
                .ShouldBeTrue();

            _fileSystem
                .DirectoryExists("Templating", "sample", "MyProject")
                .ShouldBeTrue();

            _fileSystem
                .DirectoryExists("Templating", "sample", "MyProject.Tests")
                .ShouldBeTrue();

            _fileSystem
                .FileExists("Templating", "sample", "MyProject", "MyProject.csproj")
                .ShouldBeTrue();

            _fileSystem
                .FileExists("Templating", "sample", "MyProject.Tests", "MyProject.Tests.csproj")
                .ShouldBeTrue();

            // .fubuignore
            _fileSystem
                .FileExists("Templating", "sample", MoveContent.FubuIgnoreFile)
                .ShouldBeFalse();

            _fileSystem
                .FileExists("Templating", "sample", "ignored.txt")
                .ShouldBeFalse();

            _fileSystem
                .FileExists("Templating", "sample", "MyProject.xml")
                .ShouldBeTrue();

            var solutionContents = _fileSystem.ReadStringFromFile(solutionFile);

            // cleanup first
            var info = new DirectoryInfo(tmpDir);
            info.SafeDelete();
            _fileSystem.DeleteDirectory("Templating", "sample", "MyProject");
            _fileSystem.DeleteDirectory("Templating", "sample", "MyProject.Tests");
            _fileSystem.WriteStringToFile(solutionFile, oldContents);

            var lines = ((SolutionFileService) _command.SolutionFileService).SplitSolution(solutionContents);
            var guid = _command.KeywordReplacer.Replace("GUID1");
            lines[2].ShouldEqual("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"MyProject\", \"MyProject\\MyProject.csproj\", \"{" + guid +  "}\"");
            lines[3].ShouldEqual("EndProject");
        }
    }
}