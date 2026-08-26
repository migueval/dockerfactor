# 🛡️ DockerFactor

> **A simple CLI tool to provision, harden, and deploy containers to VPS with Zero Inbound Ports.**

*Status: Under Active Development 🚧*

---

## 💡 Motivation

DockerFactor was born out of a real-world project requirement. We needed a fast, repeatable way to provision isolated VPS environments for different clients and route them securely using Cloudflare Tunnels—without having to manually configure firewalls, SSL certificates, or complex reverse proxies every single time.

---

## 🚀 What is DockerFactor?

**DockerFactor** is an open-source CLI tool being built to make VPS deployments **fast, secure, and effortless**.

It automates server setup, applies container hardening best practices, and establishes outbound Cloudflare Tunnels so your VPS runs with **zero public open ports**.

---

## 🎯 Key Goals

1. **🔒 Zero Inbound Ports (Cloudflare Tunnels):** Connect your VPS via Cloudflare Tunnels. Your server runs with **0 public open ports**—no exposed SSH, HTTP, or HTTPS to public IP scanners.
2. **⚡ Hardened Container Generator:** Automatically generate production-ready `Dockerfile` and `compose.yaml` files following security best practices (.NET Native AOT, non-root users, RAM/CPU limits).
3. **🚀 1-Line VPS Setup & CI/CD:** Provision a fresh Linux VPS in one command and auto-generate GitHub Actions workflows for continuous deployment.
4. **📊 Live Terminal Inspection:** Monitor container health, RAM/CPU metrics, and tail logs directly in your terminal with zero memory overhead.

---

## 🛠️ Tech Stack

- **CLI Engine:** C# (.NET 10 Native AOT) & Bash
- **Ingress & Security:** Cloudflare Tunnels (`cloudflared`)
- **Container Runtime:** Docker Engine & Docker Compose V2

---

## 📌 Project Status

DockerFactor is currently in its early architecture and prototyping phase. 

Feel free to star the repo or open an issue to share feedback and ideas!

Crafted by **Miguel Valdez** ([@migueval](https://github.com/migueval)).
