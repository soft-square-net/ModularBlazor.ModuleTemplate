namespace soft.square.net.WebApi.Modules.ModuleName.UseCases;

/// <summary>
/// The module's permissions, named <c>&lt;module&gt;.&lt;action&gt;</c>. Declare every permission the module uses (the
/// manifest lists <see cref="All"/>); activation fails on an undeclared one. Admins grant them per tenant.
/// </summary>
public static class ModuleNamePermissions
{
    public const string Read = "modulename.read";
    public const string Write = "modulename.write";

    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new(Read, "Read and search notes."),
        new(Write, "Create, edit and delete notes.")
    ];
}
