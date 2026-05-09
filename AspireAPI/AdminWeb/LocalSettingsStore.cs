using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AspireAPI.AdminWeb
{
    /// <summary>
    /// Reads and writes the editable subset of Aspire MCP settings to a
    /// gitignored appsettings.Local.json. Designed for the admin web UI:
    /// flat shape exposed to the form, nested JSON shape on disk that matches
    /// what AspireApiOptions binds to.
    ///
    /// On read, falls back to appsettings.Development.json values for fields
    /// not yet in Local — so the UI starts with the operator's existing
    /// configuration rather than an empty form.
    ///
    /// On write, only fields the form explicitly set are persisted; other
    /// keys in Local.json are preserved verbatim. We never touch
    /// appsettings.json or appsettings.Development.json — those are
    /// considered source-controlled and out of bounds for the admin UI.
    /// </summary>
    public sealed class LocalSettingsStore
    {
        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true,
            // Write strings as-is; the values may include credentials so we don't want
            // any encoder being clever with non-ASCII characters.
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public string LocalPath { get; }
        public string DevelopmentPath { get; }

        public LocalSettingsStore(string contentRootPath)
        {
            LocalPath = Path.Combine(contentRootPath, "appsettings.Local.json");
            DevelopmentPath = Path.Combine(contentRootPath, "appsettings.Development.json");
        }

        public AdminSettings Load()
        {
            // Read Development.json first so the UI surfaces existing values; Local
            // overrides field-by-field. Both files are tolerated as missing.
            var dev = ReadAspireApi(DevelopmentPath);
            var local = ReadAspireApi(LocalPath);
            return new AdminSettings
            {
                BaseUrl = local.BaseUrl ?? dev.BaseUrl,
                ClientId = local.ClientId ?? dev.ClientId,
                ClientSecret = local.ClientSecret ?? dev.ClientSecret,
                ApiKey = local.ApiKey ?? dev.ApiKey,
                OAuthServerUrl = local.OAuthServerUrl ?? dev.OAuthServerUrl,
                Username = local.Username ?? dev.Username,
                Password = local.Password ?? dev.Password,
                CompanyKey = local.CompanyKey ?? dev.CompanyKey,
                LocalFileExists = File.Exists(LocalPath),
            };
        }

        public void Save(AdminSettings s)
        {
            // Read existing Local.json if any so we preserve unknown keys.
            JsonObject root;
            if (File.Exists(LocalPath))
            {
                var text = File.ReadAllText(LocalPath);
                root = (JsonNode.Parse(text) as JsonObject) ?? new JsonObject();
            }
            else
            {
                root = new JsonObject();
            }

            var aspireNode = root["AspireApi"] as JsonObject;
            if (aspireNode is null)
            {
                aspireNode = new JsonObject();
                root["AspireApi"] = aspireNode;
            }

            SetIfProvided(aspireNode, "BaseUrl", s.BaseUrl);
            SetIfProvided(aspireNode, "ClientId", s.ClientId);
            SetIfProvided(aspireNode, "ClientSecret", s.ClientSecret);
            SetIfProvided(aspireNode, "ApiKey", s.ApiKey);
            SetIfProvided(aspireNode, "OAuthServerUrl", s.OAuthServerUrl);

            var authNode = aspireNode["Auth"] as JsonObject;
            if (authNode is null)
            {
                authNode = new JsonObject();
                aspireNode["Auth"] = authNode;
            }
            SetIfProvided(authNode, "Username", s.Username);
            SetIfProvided(authNode, "Password", s.Password);
            SetIfProvided(authNode, "CompanyKey", s.CompanyKey);

            var serialized = root.ToJsonString(WriteOptions);
            File.WriteAllText(LocalPath, serialized);
        }

        private static void SetIfProvided(JsonObject parent, string key, string? value)
        {
            // The form posts every field; an empty string means "clear it" rather
            // than "leave alone". A null means "the form omitted this entirely",
            // which we honor by leaving the existing value alone.
            if (value is null) return;
            if (string.IsNullOrEmpty(value))
            {
                parent.Remove(key);
                return;
            }
            parent[key] = value;
        }

        private static RawAspireApi ReadAspireApi(string path)
        {
            if (!File.Exists(path)) return new RawAspireApi();
            try
            {
                var text = File.ReadAllText(path);
                var root = JsonNode.Parse(text) as JsonObject;
                var aspire = root?["AspireApi"] as JsonObject;
                if (aspire is null) return new RawAspireApi();
                var auth = aspire["Auth"] as JsonObject;
                return new RawAspireApi
                {
                    BaseUrl = aspire["BaseUrl"]?.GetValue<string>(),
                    ClientId = aspire["ClientId"]?.GetValue<string>(),
                    ClientSecret = aspire["ClientSecret"]?.GetValue<string>(),
                    ApiKey = aspire["ApiKey"]?.GetValue<string>(),
                    OAuthServerUrl = aspire["OAuthServerUrl"]?.GetValue<string>(),
                    Username = auth?["Username"]?.GetValue<string>(),
                    Password = auth?["Password"]?.GetValue<string>(),
                    CompanyKey = auth?["CompanyKey"]?.GetValue<string>(),
                };
            }
            catch
            {
                // Malformed file — surface as "no values" rather than crash. The
                // operator can fix it by saving fresh values via the UI.
                return new RawAspireApi();
            }
        }

        private sealed class RawAspireApi
        {
            public string? BaseUrl;
            public string? ClientId;
            public string? ClientSecret;
            public string? ApiKey;
            public string? OAuthServerUrl;
            public string? Username;
            public string? Password;
            public string? CompanyKey;
        }
    }

    public sealed class AdminSettings
    {
        public string? BaseUrl { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? ApiKey { get; set; }
        public string? OAuthServerUrl { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? CompanyKey { get; set; }
        public bool LocalFileExists { get; set; }
    }
}
