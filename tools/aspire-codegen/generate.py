#!/usr/bin/env python3
"""
Aspire-MCP code generator.

Reads the Aspire External REST API OpenAPI 3.0 spec (swagger.json) and emits one
C# handler + one C# tool definition per (path, method) pair, plus a single
generated DI registration partial and a single generated tool-router partial.

Output:
    AspireAPI/Generated/Handlers/<ToolName>Handler.cs
    AspireAPI/Generated/ToolDefinitions/<ToolName>ToolDefinition.cs
    AspireAPI/Generated/AspireMcpServer.GeneratedRouter.cs   (partial of AspireMcpServer)
    AspireAPI/Generated/GeneratedDIRegistration.cs           (extension method)

Idempotent: rerun safely; existing files are overwritten.
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path
from typing import Any

REPO_ROOT = Path(__file__).resolve().parents[2]
SPEC_PATH = Path(__file__).resolve().parent / "swagger.json"
OUT_HANDLERS = REPO_ROOT / "AspireAPI" / "Generated" / "Handlers"
OUT_TOOLDEFS = REPO_ROOT / "AspireAPI" / "Generated" / "ToolDefinitions"
OUT_ROUTER_PARTIAL = REPO_ROOT / "AspireAPI" / "Generated" / "AspireMcpServer.GeneratedRouter.cs"
OUT_DI_PARTIAL = REPO_ROOT / "AspireAPI" / "Generated" / "GeneratedDIRegistration.cs"
OUT_MANIFEST = REPO_ROOT / "AspireAPI" / "Generated" / "tool-manifest.json"

# Tool names we should NOT generate — auth is handled internally by TokenService,
# and the existing 4 active hand-written tools should keep their own implementation.
EXCLUDED_PATHS: set[str] = {
    "/Authorization",
    "/Authorization/RefreshToken",
}
EXCLUDED_HANDWRITTEN_TOOLS: set[str] = {
    "ListPayments", "ListProperties", "ListContacts", "ListJobs",
}

VERB_FOR_METHOD = {
    "get-collection": "List",
    "get-single": "Get",
    "post": "Create",
    "put": "Update",
    "patch": "Patch",
    "delete": "Delete",
}


def pascal_segment(seg: str) -> str:
    """Best-effort PascalCase a path segment, splitting on - and _."""
    parts = re.split(r"[-_]", seg)
    return "".join(p[:1].upper() + p[1:] for p in parts if p)


def camel(name: str) -> str:
    return name[:1].lower() + name[1:] if name else name


def derive_resource_label(path: str) -> str:
    """e.g. '/Receipts/Approve' -> 'ReceiptsApprove', '/OpportunityTags/{id}' -> 'OpportunityTag'."""
    segs = [s for s in path.strip("/").split("/") if s and not s.startswith("{")]
    return "".join(pascal_segment(s) for s in segs) or "Root"


def derive_tool_name(path: str, method: str, has_path_id: bool) -> str:
    """
    Naming rules:
      GET /Resource             -> ListResource           (singularize for the list verb)
      GET /Resource/{id}        -> GetResource
      POST /Resource            -> CreateResource
      PUT /Resource             -> UpdateResource
      PATCH /Resource           -> PatchResource
      DELETE /Resource/{id}     -> DeleteResource
      GET /Resource/SubAction   -> GetResourceSubAction
      POST /Resource/SubAction  -> ResourceSubAction      (verb-named sub-action)
    """
    method = method.lower()
    raw_label = derive_resource_label(path)

    # Sub-action endpoints (more than one non-id segment, second segment isn't a placeholder)
    real_segs = [s for s in path.strip("/").split("/") if s and not s.startswith("{")]
    is_subaction = len(real_segs) > 1

    if is_subaction:
        if method == "get":
            return "Get" + raw_label
        # POST/PUT/PATCH/DELETE on sub-actions read naturally as the action verb itself.
        return raw_label

    # Single-resource endpoint.
    base = raw_label
    if method == "get":
        return "Get" + base if has_path_id else "List" + singularize_for_list(base)
    if method == "post":
        return "Create" + singularize_for_list(base)
    if method == "put":
        return "Update" + singularize_for_list(base)
    if method == "patch":
        return "Patch" + singularize_for_list(base)
    if method == "delete":
        return "Delete" + singularize_for_list(base)
    return method.capitalize() + base


def singularize_for_list(name: str) -> str:
    """Light-touch singular: Contacts->Contact, Properties->Property, WorkersComps->WorkersComp."""
    if name.endswith("ies"):
        return name[:-3] + "y"
    if name.endswith("ses"):
        return name[:-2]  # Statuses -> Status
    if name.endswith("s") and not name.endswith("ss"):
        return name[:-1]
    return name


def csharp_string_literal(s: str) -> str:
    """Escape arbitrary text for a C# verbatim string literal (@"...")."""
    return s.replace("\"", "\"\"")


def collect_path_parameters(spec_op: dict[str, Any], spec_path_item: dict[str, Any]) -> list[dict]:
    return [p for p in (spec_path_item.get("parameters", []) + spec_op.get("parameters", []))
            if p.get("in") == "path"]


def collect_query_parameters(spec_op: dict[str, Any], spec_path_item: dict[str, Any]) -> list[dict]:
    return [p for p in (spec_path_item.get("parameters", []) + spec_op.get("parameters", []))
            if p.get("in") == "query"]


def odata_query_params() -> list[dict]:
    """Standard OData params Aspire supports on collection GETs."""
    return [
        {"name": "$filter", "description": "OData $filter expression", "schema": {"type": "string"}},
        {"name": "$top", "description": "Maximum number of records to return", "schema": {"type": "integer"}},
        {"name": "$skip", "description": "Number of records to skip", "schema": {"type": "integer"}},
        {"name": "$orderby", "description": "OData $orderby expression", "schema": {"type": "string"}},
        {"name": "$select", "description": "Comma-separated list of fields to project", "schema": {"type": "string"}},
        {"name": "$expand", "description": "Comma-separated list of related entities to expand", "schema": {"type": "string"}},
    ]


def render_input_schema(
    method: str,
    path_params: list[dict],
    query_params: list[dict],
    request_body: dict | None,
    is_collection_get: bool,
) -> str:
    """Build the JSON schema string the tool definition will return to MCP clients."""
    properties: dict[str, Any] = {}
    required: list[str] = []

    for p in path_params:
        properties[p["name"]] = simplify_schema(p.get("schema", {"type": "string"}),
                                                description=p.get("description") or f"Path parameter '{p['name']}'.")
        required.append(p["name"])

    qs = list(query_params)
    if is_collection_get and method.lower() == "get":
        # Augment with standard OData params if not already present.
        existing = {p["name"] for p in qs}
        for o in odata_query_params():
            if o["name"] not in existing:
                qs.append(o)

    for p in qs:
        name = p["name"]
        if name in properties:
            continue
        properties[name] = simplify_schema(
            p.get("schema", {"type": "string"}),
            description=p.get("description") or f"Query parameter '{name}'.",
        )

    if request_body is not None:
        # Aspire's request bodies are arbitrary JSON; surface as a 'body' object.
        # We don't inline the full per-resource schema here — operators pass any
        # JSON body via the 'body' key. Schemas are still discoverable in Swagger UI.
        body_schema: dict[str, Any] = {
            "type": "object",
            "description": "Request body. Pass the entity payload as a nested object; the keys/values match the Aspire OpenAPI schema for this endpoint.",
            "additionalProperties": True,
        }
        properties["body"] = body_schema
        if request_body.get("required"):
            required.append("body")

    schema: dict[str, Any] = {
        "$schema": "http://json-schema.org/draft-07/schema#",
        "type": "object",
        "additionalProperties": True,
        "properties": properties,
    }
    if required:
        schema["required"] = required
    return json.dumps(schema, indent=2)


def simplify_schema(schema: dict[str, Any], description: str | None = None) -> dict[str, Any]:
    """Project an OpenAPI parameter schema to a minimal JSON Schema fragment."""
    out: dict[str, Any] = {}
    t = schema.get("type")
    fmt = schema.get("format")
    if t:
        out["type"] = t
    if fmt:
        out["format"] = fmt
    if "enum" in schema:
        out["enum"] = schema["enum"]
    if description:
        out["description"] = description
    return out


def render_handler(tool_name: str, method: str, path_template: str,
                   path_param_names: list[str], query_param_names: list[str],
                   accepts_body: bool) -> str:
    body_override = ""
    if accepts_body:
        body_override = "        protected override bool AcceptsBody => true;\n"
    def array_init(names: list[str]) -> str:
        return "System.Array.Empty<string>()" if not names else "new string[] { " + ", ".join(f"\"{n}\"" for n in names) + " }"
    qs_init = array_init(query_param_names)
    ps_init = array_init(path_param_names)
    return f"""// <auto-generated/>
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using AspireAPI.Generated;

namespace AspireAPI.Generated.Handlers
{{
    public sealed class {tool_name}Handler : GeneratedHandler
    {{
        public {tool_name}Handler(
            ILogger<{tool_name}Handler> logger,
            IHttpClientFactory httpClientFactory,
            AspireApiHelpers apiHelpers,
            AspireGenericClient client)
            : base(logger, httpClientFactory, apiHelpers, client) {{ }}

        protected override string HttpMethod => \"{method.upper()}\";
        protected override string PathTemplate => \"{path_template}\";
        protected override IReadOnlyList<string> PathParameterNames {{ get; }} = {ps_init};
        protected override IReadOnlyList<string> QueryParameterNames {{ get; }} = {qs_init};
{body_override}    }}
}}
"""


def render_tooldef(tool_name: str, description: str, schema_json: str) -> str:
    safe_desc = csharp_string_literal(description)
    safe_schema = csharp_string_literal(schema_json)
    return f"""// <auto-generated/>
namespace AspireAPI.Generated.ToolDefinitions
{{
    public sealed class {tool_name}ToolDefinition : AspireAPI.Generated.GeneratedToolDefinition
    {{
        public override string Name => \"{tool_name}\";
        public override string Description => @\"{safe_desc}\";
        protected override string JsonSchemaString => @\"{safe_schema}\";
    }}
}}
"""


def render_di_partial(tool_names: list[str]) -> str:
    handler_lines = "\n".join(
        f"            services.AddSingleton<AspireAPI.Generated.Handlers.{n}Handler>();" for n in tool_names
    )
    tooldef_lines = "\n".join(
        f"            services.AddSingleton<AspireAPI.IToolDefinition, AspireAPI.Generated.ToolDefinitions.{n}ToolDefinition>();" for n in tool_names
    )
    return f"""// <auto-generated/>
using Microsoft.Extensions.DependencyInjection;

namespace AspireAPI.Generated
{{
    public static class GeneratedAspireToolsRegistration
    {{
        /// <summary>
        /// Register the generic Aspire client, every code-generated handler, and every
        /// code-generated tool definition. Call this once during host setup.
        /// </summary>
        public static IServiceCollection AddGeneratedAspireTools(this IServiceCollection services)
        {{
            services.AddSingleton<AspireAPI.Generated.AspireGenericClient>();
{handler_lines}
{tooldef_lines}
            return services;
        }}
    }}
}}
"""


def render_router_partial(tool_names: list[str]) -> str:
    """Partial of AspireMcpServer that calls _toolRouter.RegisterTool for every generated tool."""
    body = "\n".join(
        f"            _toolRouter.RegisterTool(\"{n}\", sp => sp.GetRequiredService<AspireAPI.Generated.Handlers.{n}Handler>());"
        for n in tool_names
    )
    return f"""// <auto-generated/>
using Microsoft.Extensions.DependencyInjection;

namespace AspireAPI;

public partial class AspireMcpServer
{{
    /// <summary>
    /// Register every code-generated tool with the router. Called from RegisterToolHandlers().
    /// </summary>
    private void RegisterGeneratedTools()
    {{
{body}
    }}
}}
"""


def main() -> int:
    if not SPEC_PATH.exists():
        print(f"FATAL: missing OpenAPI spec at {SPEC_PATH}", file=sys.stderr)
        return 2

    spec = json.loads(SPEC_PATH.read_text())
    paths = spec.get("paths", {})

    OUT_HANDLERS.mkdir(parents=True, exist_ok=True)
    OUT_TOOLDEFS.mkdir(parents=True, exist_ok=True)

    # Wipe existing generated files so removed endpoints don't linger.
    for stale in list(OUT_HANDLERS.glob("*.cs")) + list(OUT_TOOLDEFS.glob("*.cs")):
        stale.unlink()

    tool_records: list[dict] = []
    seen_tool_names: dict[str, str] = {}  # name -> "METHOD path"
    name_collisions: list[tuple[str, str, str]] = []

    for path, path_item in sorted(paths.items()):
        if path in EXCLUDED_PATHS:
            continue
        for method in ("get", "post", "put", "patch", "delete"):
            op = path_item.get(method)
            if not op:
                continue

            path_params_full = collect_path_parameters(op, path_item)
            query_params_full = collect_query_parameters(op, path_item)
            path_param_names = [p["name"] for p in path_params_full]
            has_path_id = bool(path_param_names)

            # Determine if this is a collection GET (for OData augmentation + naming).
            real_segs = [s for s in path.strip("/").split("/") if s and not s.startswith("{")]
            is_subaction = len(real_segs) > 1
            is_collection_get = (method == "get" and not has_path_id and not is_subaction)

            tool_name = derive_tool_name(path, method, has_path_id)
            if tool_name in EXCLUDED_HANDWRITTEN_TOOLS:
                # Keep hand-written version; skip codegen.
                continue
            if tool_name in seen_tool_names:
                # Disambiguate by appending the method.
                alt = tool_name + method.capitalize()
                name_collisions.append((tool_name, seen_tool_names[tool_name], f"{method.upper()} {path}"))
                tool_name = alt
            seen_tool_names[tool_name] = f"{method.upper()} {path}"

            request_body = op.get("requestBody")
            accepts_body = method in ("post", "put", "patch")

            description_bits = [op.get("summary"), op.get("description")]
            description = " — ".join(b.strip() for b in description_bits if b and b.strip())
            if not description:
                description = f"{method.upper()} {path}"
            description = f"[{method.upper()} {path}] {description}"

            schema_json = render_input_schema(
                method=method,
                path_params=path_params_full,
                query_params=query_params_full,
                request_body=request_body,
                is_collection_get=is_collection_get,
            )

            # Final query parameter names include OData augmentation if applicable.
            query_param_names = [p["name"] for p in query_params_full]
            if is_collection_get and method == "get":
                existing = set(query_param_names)
                for o in odata_query_params():
                    if o["name"] not in existing:
                        query_param_names.append(o["name"])

            handler_src = render_handler(
                tool_name=tool_name,
                method=method,
                path_template=path,
                path_param_names=path_param_names,
                query_param_names=query_param_names,
                accepts_body=accepts_body,
            )
            tooldef_src = render_tooldef(tool_name=tool_name,
                                         description=description,
                                         schema_json=schema_json)

            (OUT_HANDLERS / f"{tool_name}Handler.cs").write_text(handler_src)
            (OUT_TOOLDEFS / f"{tool_name}ToolDefinition.cs").write_text(tooldef_src)

            tool_records.append({
                "name": tool_name,
                "method": method.upper(),
                "path": path,
                "description": description,
                "pathParams": path_param_names,
                "queryParams": query_param_names,
                "acceptsBody": accepts_body,
            })

    tool_names = sorted(r["name"] for r in tool_records)
    OUT_DI_PARTIAL.write_text(render_di_partial(tool_names))
    OUT_ROUTER_PARTIAL.write_text(render_router_partial(tool_names))
    OUT_MANIFEST.write_text(json.dumps({
        "generatedToolCount": len(tool_records),
        "excludedHandwritten": sorted(EXCLUDED_HANDWRITTEN_TOOLS),
        "excludedPaths": sorted(EXCLUDED_PATHS),
        "tools": sorted(tool_records, key=lambda r: r["name"]),
        "nameCollisionsResolved": [
            {"originalName": orig, "first": first, "second": second}
            for orig, first, second in name_collisions
        ],
    }, indent=2))

    print(f"Generated {len(tool_records)} tools to {OUT_HANDLERS} / {OUT_TOOLDEFS}")
    print(f"Wrote DI partial:     {OUT_DI_PARTIAL.relative_to(REPO_ROOT)}")
    print(f"Wrote router partial: {OUT_ROUTER_PARTIAL.relative_to(REPO_ROOT)}")
    print(f"Wrote manifest:       {OUT_MANIFEST.relative_to(REPO_ROOT)}")
    if name_collisions:
        print(f"NOTE: resolved {len(name_collisions)} tool-name collisions; see manifest.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
