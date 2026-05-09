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
