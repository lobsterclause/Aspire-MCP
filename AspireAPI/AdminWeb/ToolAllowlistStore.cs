using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AspireAPI.AdminWeb
{
    /// <summary>
    /// Persists per-tool enable/disable state to appsettings.Local.json under
    /// AspireApi.Allowlist. Default = all tools enabled (no allowlist key).
    ///
    /// The MCP server's ListTools and CallTool both consult IsAllowed() so that
    /// disabling a tool both removes it from the catalog (clients can't
    /// discover it) and refuses any explicit invocation.
    ///
    /// Storage shape:
    ///   "AspireApi": {
    ///     "Allowlist": {
    ///       "Mode": "blocklist" | "allowlist",
    ///       "Tools": ["ToolNameA", "ToolNameB"]
    ///     }
    ///   }
    /// In "blocklist" mode, listed tools are blocked and everything else is
    /// allowed (default mode if unspecified).
    /// In "allowlist" mode, only listed tools are allowed.
    /// </summary>
    public sealed class ToolAllowlistStore
    {
        public string LocalPath { get; }

        // Loaded snapshot — re-loaded on every Load() call from disk to pick up
        // out-of-band edits.
        private AllowlistConfig _current = AllowlistConfig.Default();
        private readonly object _lock = new();

        public ToolAllowlistStore(string contentRootPath)
        {
            LocalPath = Path.Combine(contentRootPath, "appsettings.Local.json");
            _current = LoadFromDisk();
        }

        public AllowlistConfig Load()
        {
            // Always re-read so the admin UI sees on-disk truth even after
            // out-of-band edits to Local.json.
            lock (_lock)
            {
                _current = LoadFromDisk();
                return _current;
            }
        }

        public void Save(AllowlistConfig cfg)
        {
            lock (_lock)
            {
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

                var aspire = root["AspireApi"] as JsonObject ?? new JsonObject();
                root["AspireApi"] = aspire;

                var allowlist = new JsonObject
                {
                    ["Mode"] = cfg.Mode == AllowlistMode.Allowlist ? "allowlist" : "blocklist",
                    ["Tools"] = new JsonArray(cfg.Tools.Select(t => (JsonNode)t).ToArray()),
                };
                aspire["Allowlist"] = allowlist;

                File.WriteAllText(LocalPath, root.ToJsonString(new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }));

                _current = cfg;
            }
        }

        public bool IsAllowed(string toolName)
        {
            var cfg = _current;
            if (cfg.Mode == AllowlistMode.Blocklist)
            {
                return !cfg.Tools.Contains(toolName, StringComparer.OrdinalIgnoreCase);
            }
            return cfg.Tools.Contains(toolName, StringComparer.OrdinalIgnoreCase);
        }

        private AllowlistConfig LoadFromDisk()
        {
            if (!File.Exists(LocalPath)) return AllowlistConfig.Default();
            try
            {
                var text = File.ReadAllText(LocalPath);
                var root = JsonNode.Parse(text) as JsonObject;
                var aspire = root?["AspireApi"] as JsonObject;
                var node = aspire?["Allowlist"] as JsonObject;
                if (node is null) return AllowlistConfig.Default();
                var modeStr = node["Mode"]?.GetValue<string>() ?? "blocklist";
                var mode = string.Equals(modeStr, "allowlist", StringComparison.OrdinalIgnoreCase)
                    ? AllowlistMode.Allowlist
                    : AllowlistMode.Blocklist;
                var tools = (node["Tools"] as JsonArray)?
                    .Where(n => n is not null)
                    .Select(n => n!.GetValue<string>())
                    .ToList() ?? new List<string>();
                return new AllowlistConfig(mode, tools);
            }
            catch
            {
                return AllowlistConfig.Default();
            }
        }
    }

    public enum AllowlistMode
    {
        /// <summary>Listed tools blocked; everything else allowed (default).</summary>
        Blocklist,
        /// <summary>Only listed tools allowed.</summary>
        Allowlist,
    }

    public sealed record AllowlistConfig(AllowlistMode Mode, IReadOnlyList<string> Tools)
    {
        /// <summary>
        /// Discovery-first default: only SearchAspire + the four compositions +
        /// the version probe are enabled. Operators opt-in the rest via the
        /// admin UI's "Auto-detect from tenant" probe (or by editing the
        /// allowlist manually). Stops MCP clients from seeing 167 tools on
        /// first connect — they see the 6-tool discovery surface and can
        /// route everything else through SearchAspire until the operator
        /// has tailored the allowlist.
        /// </summary>
        public static AllowlistConfig Default() => new(AllowlistMode.Allowlist, BootstrapTools);

        /// <summary>Open the floodgates — only used by operators who explicitly want it.</summary>
        public static AllowlistConfig AllowAll() => new(AllowlistMode.Blocklist, Array.Empty<string>());

        public static IReadOnlyList<string> BootstrapTools { get; } = new[]
        {
            "SearchAspire",
            "GetJobLifecycle",
            "GetCustomer360",
            "RenderScheduleBoard",
            "ListChangedSince",
            "GetVersionGetApiVersion",
        };
    }
}
