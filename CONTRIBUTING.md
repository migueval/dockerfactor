# Contributing to DockerFactor 🛡️

First off, thank you for considering contributing to **DockerFactor**! It's contributions like yours that make the open-source community an amazing place to build secure, resilient software.

Please take a moment to review this document to ensure a smooth and efficient contribution process.

---

## 📜 Code of Conduct & Core Philosophy

DockerFactor is built on three strict engineering principles:
1. **Zero Trust & Hardening by Default:** Security is never an afterthought. Features must minimize attack surface (e.g., zero open inbound ports, non-root execution).
2. **Zero Overhead:** Binaries must remain lightweight (<20MB compiled via .NET 10 Native AOT) with sub-millisecond startup and minimal RAM consumption (<15MB).
3. **Simplicity & Reliability:** Prioritize clean CLI design and deterministic infrastructure automation over unnecessary complexity.

---

## 🛠️ How Can I Contribute?

### 1. Reporting Bugs
Before creating a bug report, please check existing [GitHub Issues](https://github.com/migueval/dockerfactor/issues). When creating a bug report, please include:
- **Environment details:** OS version (Ubuntu/Debian), Docker version, .NET SDK version.
- **Steps to reproduce:** Clear, minimal code or commands to reproduce the issue.
- **Expected vs. Actual behavior:** Terminal output logs or stack traces.

### 2. Suggesting Enhancements
Feature requests are welcome! Please open an issue with the tag `enhancement` and describe:
- The problem or use case you want to solve.
- Your proposed solution or CLI command design.

### 3. Submitting Pull Requests (PRs)
1. **Fork the repository** and create your branch from `main`:
   ```bash
   git checkout -b feature/amazing-feature
   ```
2. **Ensure your changes adhere to our coding standards:**
   - **C# / .NET 10:** Follow standard C# conventions. Keep Native AOT compatibility in mind (avoid heavy reflection or un-analyzed dynamic code).
   - **Bash Scripts:** Use `set -euo pipefail` at the start of scripts for strict error handling.
3. **Test your changes locally:** Verify that the build succeeds with Native AOT compilation:
   ```bash
   dotnet publish -c Release -r linux-x64 /p:PublishAot=true
   ```
4. **Commit your changes using Conventional Commits:**
   - `feat: add new CLI command for container inspection`
   - `fix: resolve UFW rule evaluation edge case`
   - `docs: update deployment architecture guide`
   - `sec: enforce read-only root filesystem on generated compose`
5. **Open a Pull Request** against the `main` branch with a detailed description of your changes.

---

## 💻 Local Development Setup

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker Engine & Docker Compose V2](https://docs.docker.com/engine/install/)
- Linux environment (or WSL2 / macOS for local script testing)

### Building the CLI
```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/dockerfactor.git
cd dockerfactor

# Run locally in development mode
dotnet run --project src/DockerFactor.Cli

# Test Native AOT Build locally
dotnet publish src/DockerFactor.CLI -c Release -r linux-x64 /p:PublishAot=true
```

---

## ⚖️ License

By contributing to DockerFactor, you agree that your contributions will be licensed under its [MIT License](LICENSE).
