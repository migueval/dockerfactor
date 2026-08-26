# Changelog 📜

All notable changes to **DockerFactor** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned
- **Phase 1:** One-Line VPS Installer Script (`install.sh`) with UFW zero-inbound firewall hardening.
- **Phase 2:** Cloudflare Tunnel (`cloudflared`) egress routing engine & API token automation.
- **Phase 3:** Core C# (.NET 10 Native AOT) CLI (`docker-factor`) with Spectre.Console Terminal UI (TUI).
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
