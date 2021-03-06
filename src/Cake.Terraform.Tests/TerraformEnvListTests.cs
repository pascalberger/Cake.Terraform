﻿using Cake.Core;
using Cake.Terraform.EnvList;
using Cake.Testing;
using Cake.Testing.Fixtures;
using Xunit;

namespace Cake.Terraform.Tests
{
    using System.Collections.Generic;

    public class TerraformEnvListTests
    {
        class Fixture : TerraformFixture<TerraformEnvListSettings>
        {
            public Fixture(PlatformFamily platformFamily = PlatformFamily.Windows) : base(platformFamily) { }

            public IEnumerable<string> Environments { get; private set; } = new List<string>();

            protected override void RunTool()
            {
                ProcessRunner.Process.SetStandardOutput(new List<string>{ "default" });

                var tool = new TerraformEnvListRunner(FileSystem, Environment, ProcessRunner, Tools);

                Environments = tool.Run(Settings);
            }
        }

        public class TheExecutable
        {
            [Fact]
            public void Should_throw_if_terraform_runner_was_not_found()
            {
                var fixture = new Fixture();
                fixture.GivenDefaultToolDoNotExist();

                var result = Record.Exception(() => fixture.Run());

                Assert.IsType<CakeException>(result);
                Assert.Equal("Terraform: Could not locate executable.", result.Message);
            }

            [Theory]
            [InlineData("/bin/tools/terraform/terraform.exe", "/bin/tools/terraform/terraform.exe")]
            [InlineData("/bin/tools/terraform/terraform", "/bin/tools/terraform/terraform")]
            public void Should_use_terraform_from_tool_path_if_provided(string toolPath, string expected)
            {
                var fixture = new Fixture() {Settings = {ToolPath = toolPath}};
                fixture.GivenSettingsToolPathExist();

                var result = fixture.Run();

                Assert.Equal(expected, result.Path.FullPath);
            }

            [Fact]
            public void Should_find_terraform_if_tool_path_not_provided()
            {
                var fixture = new Fixture();

                var result = fixture.Run();

                Assert.Equal("/Working/tools/terraform.exe", result.Path.FullPath);
            }

            [Fact]
            public void Should_throw_if_process_has_a_non_zero_exit_code()
            {
                var fixture = new Fixture();
                fixture.GivenProcessExitsWithCode(1);

                var result = Record.Exception(() => fixture.Run());

                Assert.IsType<CakeException>(result);
                Assert.Equal("Terraform: Process returned an error (exit code 1).", result.Message);
            }

            [Fact]
            public void Should_find_linux_executable()
            {
                var fixture = new Fixture(PlatformFamily.Linux);
                fixture.Environment.Platform.Family = PlatformFamily.Linux;


                var result = fixture.Run();

                Assert.Equal("/Working/tools/terraform", result.Path.FullPath);
            }

            [Fact]
            public void Should_set_workspace_and_list_parameter()
            {
                var fixture = new Fixture();

                var result = fixture.Run();

                Assert.Contains("workspace list", result.Args);
            }

            [Fact]
            public void Should_set_env_and_list_parameter()
            {
                var fixture = new Fixture {Settings = {EnvCommand = TerraformEnvSettings.EnvCommandType.Env}};

                var result = fixture.Run();

                Assert.Contains("env list", result.Args);
            }

            [Fact]
            public void Should_return_existing_environments()
            {
                var fixture = new Fixture();
            
                var result = fixture.Run();
                
                Assert.Contains("default", fixture.Environments);
            }
        }
    }
}