# Changelog 📜

All notable changes to **DockerFactor** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
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
