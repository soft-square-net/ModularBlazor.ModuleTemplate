using soft.square.net.ModularBlazor.Abstractions;

// Default settings of this client module: read by the WebApi's client-modules core module when the file is uploaded.
// Open to every signed-in user: what each one may do is decided by the API (modulename.* permissions in the user's
// tenant), and the page shows its answer. The API is tenant-bound, so sign in to a tenant to use it.
[assembly: ClientModule(Name = "ModuleName", Layout = "default", RoutePrefixes = new[] { "modulename" },
    Dependencies = new[] { "WebApi.Modules.ModuleName.Client" }, Styles = new[] { "modulename.css" },
    NavTitle = "ModuleName", NavHref = "modulename", NavOrder = 50)]
