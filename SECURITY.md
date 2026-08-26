# Security Policy 🛡️

At **DockerFactor**, security is not an add-on feature—it is the foundational constraint of the entire architecture. 

Because DockerFactor manages server firewall baselines, Cloudflare Tunnels, and container hardening, we treat security vulnerabilities with the highest priority.

---

## 🔒 Supported Versions

We actively release security patches and updates for the following versions:

| Version | Supported |
| :--- | :--- |
| `main` branch / Latest Release | ✅ Yes |
| Pre-release / Alpha (`v1alpha1`) | ✅ Yes |
| Legacy releases | ❌ No |

---

## 🚨 Reporting a Vulnerability

**Please do NOT report security vulnerabilities through public GitHub Issues.**

If you discover a security vulnerability, misconfiguration, or security bypass within DockerFactor, please report it privately following our Responsible Disclosure process:

### How to Submit
1. **Email:** Send an email directly to **[migueval123solis@gmail.com](mailto:migueval123solis@gmail.com)** with the subject line `[SECURITY VULNERABILITY] DockerFactor`.
2. **Private GitHub Advisory:** Alternatively, submit a private disclosure via [GitHub Security Advisories](https://github.com/migueval/dockerfactor/security/advisories/new).

### What to Include in Your Report
To help us triage and resolve the issue quickly, please provide:
- **Type of issue:** (e.g., UFW firewall bypass, container escape vector, token leakage, unhandled privilege escalation).
- **Component affected:** (e.g., `install.sh`, C# Native AOT CLI, `cloudflared` ingress routing engine).
- **Proof of Concept (PoC):** Step-by-step instructions or script to reproduce the issue safely.
- **Impact assessment:** What access or privileges an attacker could gain.

---

## ⏱️ Response Timeline SLA

When a security vulnerability is reported privately:
1. **Acknowledgment:** We will acknowledge receipt of your report within **48 hours**.
2. **Triage & Assessment:** We will assess the severity and impact within **5 business days**.
3. **Patch & Advisory:** If confirmed, we will prepare a security patch, publish a new release, and issue a security advisory crediting your responsible disclosure (unless you prefer anonymity).

---

## 🛡️ Operational Security Guidelines for Operators

If you run DockerFactor in production, we strongly recommend following these operational practices:
- **Verify Script Checksums:** Always verify `install.sh` checksums before piping to `bash`.
- **Cloudflare Token Rotation:** Periodically rotate your `CLOUDFLARE_TUNNEL_TOKEN` credentials.
- **Keep Host Kernel Updated:** Regularly update your VPS OS (`apt update && apt upgrade`) to apply Linux kernel security patches.
- **Audit Container Logs:** Monitor container execution logs for unauthorized execution attempts.

---

## ⚖️ Credit & Recognition

We deeply appreciate security researchers and engineers who help keep DockerFactor and the open-source ecosystem secure. Responsible disclosures will be publicly acknowledged in our release notes and Security Hall of Fame.
