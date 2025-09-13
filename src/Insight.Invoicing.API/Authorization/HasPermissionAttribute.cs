using Microsoft.AspNetCore.Authorization;

namespace Insight.Invoicing.API.Authorization;


public class HasPermissionAttribute : AuthorizeAttribute
{
 
    public HasPermissionAttribute(string permission)
    {
        Policy = permission;
    }
}

public static class Permissions
{
    public const string ContractsView = "Contracts:View";
    public const string ContractsCreate = "Contracts:Create";
    public const string ContractsEdit = "Contracts:Edit";
    public const string ContractsApprove = "Contracts:Approve";
    public const string ContractsReject = "Contracts:Reject";
    public const string ContractsDelete = "Contracts:Delete";

    public const string InstallmentsView = "Installments:View";
    public const string InstallmentsEdit = "Installments:Edit";

    public const string PaymentReceiptsView = "PaymentReceipts:View";
    public const string PaymentReceiptsUpload = "PaymentReceipts:Upload";
    public const string PaymentReceiptsValidate = "PaymentReceipts:Validate";

    public const string UsersView = "Users:View";
    public const string UsersCreate = "Users:Create";
    public const string UsersEdit = "Users:Edit";
    public const string UsersDelete = "Users:Delete";

    public const string ReportsView = "Reports:View";
    public const string ReportsGenerate = "Reports:Generate";

    public const string AdminDashboard = "Admin:Dashboard";
    public const string AdminSettings = "Admin:Settings";
}

