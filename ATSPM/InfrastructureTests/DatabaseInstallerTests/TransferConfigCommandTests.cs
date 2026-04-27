using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Xunit;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.InfrastructureTests.DatabaseInstallerTests;

public class TransferConfigCommandTests
{
    private static readonly string DatabaseInstallerAssemblyPath =
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "DatabaseInstaller",
            "bin",
            "Debug",
            "net8.0",
            "DatabaseInstaller.dll"));

    [Fact]
    public void RootCommand_IncludesTransferConfigCommand()
    {
        var assembly = LoadDatabaseInstallerAssembly();
        var rootCommand = Activator.CreateInstance(
            assembly.GetType("DatabaseInstaller.Commands.DatabaseInstallerCommands", throwOnError: true)!)!;

        var children = GetEnumerableProperty(rootCommand, "Children")
            .Select(child => (string)GetPropertyValue(child, "Name")!)
            .ToArray();

        Assert.Contains("transfer-config", children);
    }

    [Fact]
    public void TransferConfigCommand_ExposesExpectedOptions()
    {
        var assembly = LoadDatabaseInstallerAssembly();
        var command = Activator.CreateInstance(
            assembly.GetType("DatabaseInstaller.Commands.TransferConfigCommand", throwOnError: true)!)!;

        var optionNames = GetEnumerableProperty(command, "Options")
            .Select(option => GetEnumerableProperty(option, "Aliases").Cast<string>().First())
            .ToArray();

        Assert.Equal(
            new[]
            {
                "--api-base-url",
                "--bearer-token",
                "--delete",
                "--update-locations",
                "--update-speed"
            },
            optionNames);
    }

    [Fact]
    public void BindCommandOptions_RegistersBinderAndHostedService()
    {
        var assembly = LoadDatabaseInstallerAssembly();
        var commandType = assembly.GetType("DatabaseInstaller.Commands.TransferConfigCommand", throwOnError: true)!;
        var command = Activator.CreateInstance(commandType)!;
        var services = new ServiceCollection();
        var bindMethod = commandType.GetMethod("BindCommandOptions", BindingFlags.Instance | BindingFlags.Public)!;

        bindMethod.Invoke(command, new object[] { new HostBuilderContext(new Dictionary<object, object>()), services });

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType.FullName?.Contains("System.CommandLine.NamingConventionBinder.ModelBinder") == true);

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IHostedService) &&
                          descriptor.ImplementationType?.Name == "TransferConfigCommandHostedService");
    }

    [Fact]
    public void JsonOptions_AcceptsStringEnumValues()
    {
        var assembly = LoadDatabaseInstallerAssembly();
        var hostedServiceType = assembly.GetType("TransferConfigCommandHostedService", throwOnError: true)!;
        var jsonOptionsField = hostedServiceType.GetField("JsonOptions", BindingFlags.NonPublic | BindingFlags.Static)!;
        var jsonOptions = Assert.IsType<JsonSerializerOptions>(jsonOptionsField.GetValue(null));

        var json = """
        {
          "value": [
            {
              "id": 1,
              "description": "Test config",
              "protocol": "Ftp",
              "port": 1234
            }
          ]
        }
        """;

        var responseType = hostedServiceType.GetNestedType("ODataResponse`1", BindingFlags.NonPublic)!;
        var closedResponseType = responseType.MakeGenericType(typeof(DeviceConfiguration));

        var response = JsonSerializer.Deserialize(json, closedResponseType, jsonOptions);
        Assert.NotNull(response);

        var valueProperty = closedResponseType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)!;
        var value = Assert.IsAssignableFrom<IEnumerable<DeviceConfiguration>>(valueProperty.GetValue(response));
        var configuration = Assert.Single(value);

        Assert.Equal(TransportProtocols.Ftp, configuration.Protocol);
    }

    private static Assembly LoadDatabaseInstallerAssembly()
    {
        Assert.True(File.Exists(DatabaseInstallerAssemblyPath), $"Missing assembly: {DatabaseInstallerAssemblyPath}");
        return Assembly.LoadFrom(DatabaseInstallerAssemblyPath);
    }

    private static IEnumerable<object> GetEnumerableProperty(object instance, string propertyName)
    {
        var value = GetPropertyValue(instance, propertyName);
        return Assert.IsAssignableFrom<IEnumerable<object>>(value);
    }

    private static object GetPropertyValue(object instance, string propertyName) =>
        instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!
            .GetValue(instance);
}
