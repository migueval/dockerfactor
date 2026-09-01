# Changelog 📜

> The implementation was intentionally reset on 2026-08-31. Entries below the new Unreleased section describe the discarded exploratory prototype and are retained only as historical context.

## [Unreleased] — Foundation reset

### Added

- Clean `Core`, `Engine` and `CLI` project boundaries targeting .NET 10.
- Versioned `dockerfactor.dev/v1alpha1` application manifest.
- Strict YAML parsing with unknown-field and duplicate-key rejection.
- Read-only `docker-factor inspect [DIR]` command.
- Stable manifest validation codes and automated tests.
- Official .NET 10 baseline pinned through `global.json` and shared build properties.
- Native AOT-compatible static YAML deserialization with source-generated metadata.
- Verified `win-x64` native executable publication and manifest inspection.
- Added `validate`, deterministic JSON output and strict warning handling for CI.
- Added read-only project runtime detection for .NET, Node, Angular, NestJS, Go and Python.
- Added manifest size, recursion, anchor, alias and explicit-tag defenses.
- Published the v1alpha1 JSON Schema and manifest reference documentation.
- Added CLI-level tests, bringing the automated suite to 19 tests.
- Added safe `init` with deterministic runtime defaults, `--dry-run`, JSON preview and explicit `--force` replacement.
- Added atomic create-new behavior and conflict exit code `3` to protect existing manifests.
- Added initialization documentation and expanded the automated suite to 24 tests.
- Added a runnable ASP.NET Core .NET 10 example API for hands-on init, validation and runtime testing.

All notable changes to **DockerFactor** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Security & Defensive Hardening (5 Code Improvements)
- **Strict Cryptographic Token Validation:** Enforced 64-character (256-bit) hex regex validation in `ConnectCommand`.
- **Process Memory Argument Protection:** Environment variable injection (`TUNNEL_TOKEN`) in `CloudflareManagedAdapter` to prevent token leakage via `ps aux` / process listing.
- **Strict Target URL Scheme Validation:** Added scheme check (HTTP/HTTPS absolute URIs) in `QuickTunnelAdapter` to prevent argument injection.
- **PID Verification & Process Recycler Guard:** Added process name verification (`cloudflared`/`wsl`) before revoking PIDs in `QuickTunnelAdapter.RevokeRouteAsync`.
- **WSL Isolation & Distro Target Guard:** Automated non-docker-desktop WSL distro targeting for process execution isolation.

### Added
- **Phase 4 (100% Completed):** Smart Docker Hardening Generator (`docker-factor init`):
  - Added `TechnologyDetector.cs` for automated stack detection (.NET, Node.js, Go, Python).
  - Added `DockerfileHardener.cs` generating multi-stage builds with `USER 10001` (non-root) & Distroless/Chiseled Ubuntu bases.
  - Added `ComposeHardener.cs` generating hardened `compose.yaml` with `read_only: true`, `tmpfs: /tmp:rw,noexec,nosuid`, `cap_drop: [ALL]`, `no-new-privileges:true`, and cgroups 256MB RAM limits.
  - Added `InitCommand.cs` (`docker-factor init`) in C# CLI with Spectre.Console TUI table & status spinner.
  - Added `HardeningTests.cs` xUnit test suite (10/10 tests passing green).

- **Phase 3 (100% Completed):** Core C# CLI application (`src/DockerFactor.CLI`) with Spectre.Console TUI:
  - `docker-factor connect <TOKEN>`: Validates 256-bit ephemeral pairing token and persists state to `~/.dockerfactor/config.json`.
  - `docker-factor tunnel <URL>`: Deploys instant Cloudflare QuickTunnels (`trycloudflare.com`) with TUI cards and graceful cleanup on exit.
  - `docker-factor audit`: Runs Zero-Trust posture security scan and outputs rich formatted score table.
  - Native AOT compliant string formatting and reflection-free JSON serialization.
- **Phase 2 (100% Completed):** Ingress Adapter architecture & free Cloudflare Quick Tunnels:
  - Abstract `IIngressAdapter` interface in `DockerFactor.Core`.
  - `QuickTunnelAdapter` supporting free domain-less HTTPS routes via `trycloudflare.com`.
  - `CloudflareManagedAdapter` for token-based custom domain routing.
  - Automated regex parser and process manager for `cloudflared`.
  - Unit test suite (`DockerFactor.Engine.Tests`) with 4 passing xUnit tests.
- **Phase 1 (100% Completed):** Single-line VPS provisioning script (`scripts/install.sh`) featuring:
  - Automated OS detection (Ubuntu/Debian) and CPU architecture validation (`amd64`/`arm64`).
  - Docker Engine (`v29.7.2`) & Docker Compose V2 (`v5.5.0`) installer.
  - Cloudflare Tunnel daemon (`cloudflared v2026.8.2`) installer with direct `.deb` fallback supporting Debian 13 (`trixie`).
  - Zero-Trust UFW firewall hardening (0 public inbound ports exposed).
  - `DOCKER-USER` iptables chain injection on default network interface to block Docker daemon port bypasses.
  - Ephemeral 256-bit CSPRNG cryptographic pairing token generator (`/etc/dockerfactor/pairing.json`, 15-minute TTL).

### Planned
- **Phase 2 & 3:** C# (.NET 10 Native AOT) CLI (`docker-factor`) initialization with Spectre.Console TUI and `connect <TOKEN>` pairing command.
- **Phase 4:** Smart Docker Hardening Generator for .NET Native AOT, Node/NestJS, and Go.
- **Phase 5:** Automated GitHub Actions Workflow Generator (`.github/workflows/deploy.yml`).

---

## [v0.1.0-alpha] - 2026-08-26

### Added
- **Architecture Specification (`ARCHITECTURE.md`):** Comprehensive system design defining Zero Open Inbound Ports, pull-based outbound mTLS agent architecture, and declarative `ApplicationDeployment` (`v1alpha1`) YAML manifest schema.
- **Security Engine Specification (`SECURITY_ENGINE.md`):** Complete specifications for 4 core defensive modules:
  - Host Network Isolation & `DOCKER-USER` iptables bypass prevention.
  - Container Hardening Matrix (`USER 10001`, `readOnlyRootFilesystem: true`, `cap_drop: [ALL]`, `no-new-privileges`, and cgroup RAM/CPU/PID bounds).
  - Cryptographic Pairing & Anti-Replay Tokens (256-bit CSPRNG HMAC, nonces, timestamps).
  - Real-Time Posture Auditor (`docker-factor audit`).
- **Repository Governance & Guidelines:**
  - `SECURITY.md`: Responsible vulnerability disclosure policy and SLA response guidelines.
  - `CONTRIBUTING.md`: Contributor guidelines, C# .NET Native AOT standards, and Conventional Commit conventions.
  - `README.md`: Project motivation, architecture sequence diagram, features, and roadmap.
