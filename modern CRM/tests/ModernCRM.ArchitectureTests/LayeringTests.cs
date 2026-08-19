using Microsoft.AspNetCore.Mvc;
using ModernCRM.Auth.Application.Integration;
using ModernCRM.Crm.Application.Frontend;
using Xunit;

namespace ModernCRM.ArchitectureTests;

public sealed class LayeringTests
{
    [Fact]
    public void Api_controllers_do_not_depend_on_infrastructure_or_entity_framework_types()
    {
        var assemblies = new[]
        {
            typeof(ModernCRM.Auth.Api.Controllers.AuthController).Assembly,
            typeof(ModernCRM.Crm.Api.Controllers.AccountsController).Assembly
        };

        var forbidden = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(ControllerBase).IsAssignableFrom(x))
            .SelectMany(x => x.GetConstructors())
            .SelectMany(x => x.GetParameters())
            .Select(x => x.ParameterType.FullName ?? string.Empty)
            .Where(x => x.Contains(".Infrastructure.", StringComparison.Ordinal) || x.Contains("EntityFrameworkCore", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(forbidden);
    }

    [Fact]
    public void Application_repository_contracts_do_not_expose_iqueryable()
    {
        var contracts = new[] { typeof(ICrmFrontendRepository), typeof(IAuthPersistenceRepository) };
        var exposed = contracts.SelectMany(x => x.GetMethods())
            .SelectMany(x => new[] { x.ReturnType }.Concat(x.GetParameters().Select(p => p.ParameterType)))
            .Where(ContainsQueryable)
            .ToList();
        Assert.Empty(exposed);
    }

    private static bool ContainsQueryable(Type type)
        => type == typeof(IQueryable) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>));
}
