namespace soft.square.net.WebApi.Modules.ModuleName.UseCases;

/// <summary>Module settings (section <c>ModuleName</c>; defaults in the manifest, changed on the Host Modules page).</summary>
public sealed class ModuleNameOptions
{
    /// <summary>The largest page a list or search returns.</summary>
    public int MaxPageSize { get; set; } = 100;
}
