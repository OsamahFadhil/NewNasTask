using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Insight.Invoicing.API.Authorization;


public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (HasPermission(roleClaim, requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }

    private static bool HasPermission(string role, string permission)
    {
        var rolePermissions = new Dictionary<string, string[]>
        {
            ["Administrator"] = new[]
            {
                Permissions.ContractsView,
                Permissions.ContractsCreate,
                Permissions.ContractsEdit,
                Permissions.ContractsApprove,
                Permissions.ContractsReject,
                Permissions.ContractsDelete,
                
                Permissions.InstallmentsView,
                Permissions.InstallmentsEdit,
                
                Permissions.PaymentReceiptsView,
                Permissions.PaymentReceiptsValidate,
                
                Permissions.UsersView,
                Permissions.UsersCreate,
                Permissions.UsersEdit,
                Permissions.UsersDelete,
                
                Permissions.ReportsView,
                Permissions.ReportsGenerate,
                
                Permissions.AdminDashboard,
                Permissions.AdminSettings
            },

            ["Tenant"] = new[]
            {
                Permissions.ContractsView,
                Permissions.ContractsCreate,
                
                Permissions.InstallmentsView,
                
                Permissions.PaymentReceiptsView,
                Permissions.PaymentReceiptsUpload
            }
        };

        if (rolePermissions.TryGetValue(role, out var permissions))
        {
            return permissions.Contains(permission);
        }

        return false;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

