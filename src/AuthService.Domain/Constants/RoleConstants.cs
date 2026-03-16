namespace AuthService.Domain.Constants;

public static class RoleConstants
{
    public const string ADMIN_ROLE = "ADMIN";
    public const string USER_ROLE = "USER";
    public static readonly string[] AllowedRoles = { ADMIN_ROLE, USER_ROLE };
}