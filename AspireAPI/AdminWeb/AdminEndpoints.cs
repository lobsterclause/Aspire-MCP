using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AspireAPI.AdminWeb
{
    public static class AdminEndpoints
    {
        public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
        {
            // Single-page settings UI served as an embedded resource.
            app.MapGet("/", () => Results.Redirect("/admin"));
            app.MapGet("/admin", () =>
            {
                var html = LoadEmbeddedResource("AspireAPI.AdminWeb.admin.html");
                return Results.Text(html, "text/html; charset=utf-8");
            });

            app.MapGet("/api/config", (LocalSettingsStore store) =>
            {
                var s = store.Load();
                return Results.Json(new
                {
                    settings = s,
                    paths = new { local = store.LocalPath, development = store.DevelopmentPath },
                });
            });

            app.MapPost("/api/config", async (HttpRequest req, LocalSettingsStore store) =>
            {
                var settings = await req.ReadFromJsonAsync<AdminSettings>();
                if (settings is null) return Results.BadRequest(new { error = "missing body" });
                store.Save(settings);
                return Results.Json(new
                {
                    saved = true,
                    path = store.LocalPath,
                    note = "Settings written. Restart the MCP server (close stdio client, relaunch) for the changes to take effect.",
                });
            });

            app.MapGet("/api/tools", async (ToolCatalogService catalog, System.Threading.CancellationToken ct) =>
            {
                var entries = await catalog.ListAsync(ct);
                return Results.Json(new { count = entries.Count, tools = entries });
            });

            app.MapPost("/api/tools/{name}/invoke", async (
                string name,
                HttpRequest req,
                ToolCatalogService catalog,
                System.Threading.CancellationToken ct) =>
            {
                var dryRun = string.Equals(req.Query["dryRun"].ToString(), "true",
                    StringComparison.OrdinalIgnoreCase);
                System.Text.Json.JsonElement? args = null;
                if (req.ContentLength is > 0)
                {
                    using var doc = await System.Text.Json.JsonDocument.ParseAsync(req.Body, cancellationToken: ct);
                    args = doc.RootElement.Clone();
                }
                var result = await catalog.InvokeAsync(name, args, dryRun, ct);
                return Results.Json(result);
            });

            app.MapGet("/api/status", (TokenService tokens) =>
            {
                return Results.Json(tokens.GetStatus());
            });

            app.MapGet("/api/tail", (CallTailBuffer tail, HttpRequest req) =>
            {
                var max = int.TryParse(req.Query["max"], out var m) && m > 0 && m <= CallTailBuffer.Capacity
                    ? m : CallTailBuffer.Capacity;
                return Results.Json(tail.Recent(max));
            });

            app.MapPost("/api/tail/clear", (CallTailBuffer tail) =>
            {
                tail.Clear();
                return Results.Json(new { cleared = true });
            });

            app.MapGet("/api/allowlist", (ToolAllowlistStore store) =>
            {
                return Results.Json(store.Load());
            });

            app.MapPost("/api/allowlist", async (HttpRequest req, ToolAllowlistStore store) =>
            {
                var cfg = await req.ReadFromJsonAsync<AllowlistConfig>();
                if (cfg is null) return Results.BadRequest(new { error = "missing body" });
                store.Save(cfg);
                return Results.Json(new { saved = true, mode = cfg.Mode.ToString(), count = cfg.Tools.Count });
            });

            app.MapPost("/api/probe", async (TenantProbeService probe, System.Threading.CancellationToken ct) =>
            {
                var result = await probe.ProbeAsync(ct);
                return Results.Json(result);
            });

            app.MapGet("/api/bootstrap", () =>
            {
                // The discovery-first default: tools that are always on regardless
                // of the allowlist UI's state. UI uses this to disable their
                // "uncheck to disable" controls.
                return Results.Json(new { tools = AllowlistConfig.BootstrapTools });
            });
        }

        private static string LoadEmbeddedResource(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Embedded resource '{name}' not found.");
            using var reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
