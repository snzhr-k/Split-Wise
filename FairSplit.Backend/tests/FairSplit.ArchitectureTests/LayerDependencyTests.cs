using FairSplit.Api.Controllers;
using NetArchTest.Rules;

namespace FairSplit.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string ControllersNamespace = "FairSplit.Api.Controllers";
    private const string ServicesNamespace = "FairSplit.Api.Services";
    private const string RepositoriesNamespace = "FairSplit.Api.Repositories";
    private const string InfrastructureNamespace = "FairSplit.Api.Infrastructure";

    [Fact]
    public void Controllers_Should_Not_Depend_On_Repositories()
    {
        var result = Types
            .InAssembly(typeof(GroupsController).Assembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .ShouldNot()
            .HaveDependencyOn(RepositoriesNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, BuildFailureMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Controllers_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(GroupsController).Assembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, BuildFailureMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Services_Should_Not_Depend_On_Controllers()
    {
        var result = Types
            .InAssembly(typeof(GroupsController).Assembly)
            .That()
            .ResideInNamespaceStartingWith(ServicesNamespace)
            .ShouldNot()
            .HaveDependencyOn(ControllersNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, BuildFailureMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Services_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(GroupsController).Assembly)
            .That()
            .ResideInNamespaceStartingWith(ServicesNamespace)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, BuildFailureMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Repositories_Should_Not_Depend_On_Services()
    {
        var result = Types
            .InAssembly(typeof(GroupsController).Assembly)
            .That()
            .ResideInNamespaceStartingWith(RepositoriesNamespace)
            .ShouldNot()
            .HaveDependencyOn(ServicesNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, BuildFailureMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Repositories_Should_Not_Depend_On_Controllers()
    {
        var result = Types
            .InAssembly(typeof(GroupsController).Assembly)
            .That()
            .ResideInNamespaceStartingWith(RepositoriesNamespace)
            .ShouldNot()
            .HaveDependencyOn(ControllersNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, BuildFailureMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Controllers_Should_Depend_On_Services()
    {
        var result = Types
            .InAssembly(typeof(GroupsController).Assembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .Should()
            .HaveDependencyOn(ServicesNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, BuildFailureMessage(result.FailingTypeNames));
    }

    private static string BuildFailureMessage(IReadOnlyCollection<string>? failingTypes)
    {
        if (failingTypes is null || failingTypes.Count == 0)
        {
            return "Layer dependency rule failed.";
        }

        return $"Layer dependency rule failed for: {string.Join(", ", failingTypes)}";
    }
}
