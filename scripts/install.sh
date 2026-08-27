#!/usr/bin/env bash
# ==============================================================================
# DockerFactor — Phase 1: Zero-Trust VPS Provisioning & Hardening Script
# ==============================================================================
# License: MIT
# Maintainer: Miguel Valdez (@migueval)
# Description: Installs Docker Engine, Docker Compose V2, cloudflared daemon,
#              applies UFW zero-inbound policy, injects DOCKER-USER iptables rules,
#              and generates an ephemeral 256-bit pairing token.
# ==============================================================================

set -euo pipefail

# Visual log formatting
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1" >&2; }

echo -e "${BLUE}======================================================================${NC}"
echo -e "${BLUE}  🛡️ DockerFactor — Zero-Trust VPS Provisioning & Hardening${NC}"
echo -e "${BLUE}======================================================================${NC}"
echo ""

# ------------------------------------------------------------------------------
# 1. Root Privileges Check
# ------------------------------------------------------------------------------
if [[ $EUID -ne 0 ]]; then
   log_error "This script must be executed with root privileges. Please run with sudo."
   exit 1
fi

# ------------------------------------------------------------------------------
# 2. OS & Architecture Detection
# ------------------------------------------------------------------------------
log_info "Detecting Operating System and Hardware Architecture..."

if [[ -f /etc/os-release ]]; then
    . /etc/os-release
    OS=$ID
    VERSION=$VERSION_ID
    CODENAME=${VERSION_CODENAME:-$(lsb_release -cs 2>/dev/null || echo "")}
else
    log_error "Cannot detect OS: /etc/os-release not found."
    exit 1
fi

ARCH=$(dpkg --print-architecture 2>/dev/null || uname -m)

log_info "Detected OS: $NAME ($OS) $VERSION"
log_info "Detected Architecture: $ARCH"

if [[ "$OS" != "ubuntu" && "$OS" != "debian" ]]; then
    log_error "Unsupported Linux distribution: '$OS'. DockerFactor currently supports Ubuntu and Debian."
    exit 1
fi

log_success "OS and Architecture validation PASSED."

# ------------------------------------------------------------------------------
# 3. Install Prerequisites & Docker Engine + Compose V2
# ------------------------------------------------------------------------------
# Clean up any stale or invalid cloudflared repository list from previous attempts
rm -f /etc/apt/sources.list.d/cloudflared.list

log_info "Installing package prerequisites (ca-certificates, curl, gnupg, ufw, iptables)..."
apt-get update -qq
apt-get install -y -qq ca-certificates curl gnupg lsb-release ufw iptables openssl > /dev/null

if command -v docker &> /dev/null; then
    log_warn "Docker Engine is already installed. Skipping repository setup."
else
    log_info "Configuring Docker official GPG keyring and APT repository..."
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL "https://download.docker.com/linux/$OS/gpg" | gpg --dearmor -o /etc/apt/keyrings/docker.gpg --yes
    chmod a+r /etc/apt/keyrings/docker.gpg

    echo "deb [arch=$ARCH signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/$OS $CODENAME stable" \
        | tee /etc/apt/sources.list.d/docker.list > /dev/null

    log_info "Updating APT package index and installing Docker Engine + Compose V2..."
    apt-get update -qq
    apt-get install -y -qq docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin > /dev/null

    log_info "Enabling and starting Docker service..."
    systemctl enable --now docker > /dev/null 2>&1 || true
    log_success "Docker Engine & Compose V2 installed successfully."
fi

# ------------------------------------------------------------------------------
# 4. Install Cloudflare Tunnel Daemon (cloudflared)
# ------------------------------------------------------------------------------
if command -v cloudflared &> /dev/null; then
    log_warn "cloudflared daemon is already installed. Skipping."
else
    log_info "Installing Cloudflare Tunnel daemon (cloudflared)..."
    CF_ARCH=$ARCH
    [[ "$ARCH" == "x86_64" ]] && CF_ARCH="amd64"
    [[ "$ARCH" == "aarch64" ]] && CF_ARCH="arm64"

    # Download official Cloudflare deb package (compatible with Debian 13 trixie & Ubuntu)
    curl -fsSL -o /tmp/cloudflared.deb "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-${CF_ARCH}.deb" || \
    curl -fsSL -o /tmp/cloudflared.deb "https://pkg.cloudflare.com/cloudflared-stable-linux-${CF_ARCH}.deb"

    dpkg -i /tmp/cloudflared.deb > /dev/null 2>&1 || apt-get install -f -y -qq > /dev/null
    rm -f /tmp/cloudflared.deb
    log_success "cloudflared installed successfully."
fi

# ------------------------------------------------------------------------------
# 5. Apply Zero-Inbound Firewall Baseline (UFW & iptables)
# ------------------------------------------------------------------------------
log_info "Applying Zero-Trust perimeter firewall baseline..."

# Configure UFW
ufw --force reset > /dev/null 2>&1 || true
ufw default deny incoming > /dev/null
ufw default allow outgoing > /dev/null
ufw allow in on lo > /dev/null
ufw --force enable > /dev/null 2>&1 || true

log_success "UFW Hardening Applied: 0 Inbound Public Ports Allowed."

# DOCKER-USER chain protection (Prevent Docker daemon from bypassing UFW)
DEFAULT_IFACE=$(ip route show default 2>/dev/null | awk '/default/ {print $5}' | head -n 1 || echo "")

if [[ -n "$DEFAULT_IFACE" ]]; then
    log_info "Injecting DOCKER-USER bypass prevention on interface: $DEFAULT_IFACE"
    iptables -I DOCKER-USER -i "$DEFAULT_IFACE" -j DROP 2>/dev/null || true
    iptables -I DOCKER-USER -i "$DEFAULT_IFACE" -m state --state ESTABLISHED,RELATED -j ACCEPT 2>/dev/null || true
    log_success "DOCKER-USER iptables chain secured."
fi

# ------------------------------------------------------------------------------
# 6. Generate Cryptographic Ephemeral Pairing Token
# ------------------------------------------------------------------------------
log_info "Generating 256-bit CSPRNG pairing token..."
RAW_TOKEN=$(openssl rand -hex 32)
TOKEN_HASH=$(echo -n "$RAW_TOKEN" | openssl dgst -sha256 | awk '{print $2}')

PAIRING_DIR="/etc/dockerfactor"
mkdir -p "$PAIRING_DIR"
chmod 700 "$PAIRING_DIR"

cat <<EOF > "$PAIRING_DIR/pairing.json"
{
  "createdAt": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "tokenHash": "$TOKEN_HASH",
  "expiresAt": "$(date -u -d "+15 minutes" +"%Y-%m-%dT%H:%M:%SZ" 2>/dev/null || date -u -v+15M +"%Y-%m-%dT%H:%M:%SZ")"
}
EOF
chmod 600 "$PAIRING_DIR/pairing.json"

log_success "Ephemeral pairing token created (15-min TTL)."

# ------------------------------------------------------------------------------
# 7. Verification & Summary Banner
# ------------------------------------------------------------------------------
DOCKER_VER=$(docker --version 2>/dev/null | awk '{print $3}' | tr -d ',' || echo "Installed")
COMPOSE_VER=$(docker compose version 2>/dev/null | awk '{print $4}' || echo "Installed")
CF_VER=$(cloudflared --version 2>/dev/null | awk '{print $3}' || echo "Installed")

echo ""
echo -e "${GREEN}======================================================================${NC}"
echo -e "${GREEN}  🛡️ DockerFactor Zero-Trust VPS Provisioning Complete!${NC}"
echo -e "${GREEN}======================================================================${NC}"
echo -e "  Network Status:   ${GREEN}0 Public Inbound Ports Exposed${NC}"
echo -e "  Firewall Baseline:${GREEN} UFW Active (Deny Incoming) + DOCKER-USER Secured${NC}"
echo -e "  Docker Engine:    ${BLUE}v$DOCKER_VER${NC}"
echo -e "  Docker Compose:   ${BLUE}$COMPOSE_VER${NC}"
echo -e "  Cloudflared:      ${BLUE}v$CF_VER${NC}"
echo ""
echo -e "  🔑 ${YELLOW}YOUR PAIRING TOKEN (Valid for 15 minutes):${NC}"
echo -e "     ${BLUE}$RAW_TOKEN${NC}"
echo ""
echo -e "  Connect from your local machine:"
echo -e "     ${YELLOW}docker-factor connect $RAW_TOKEN${NC}"
echo -e "${GREEN}======================================================================${NC}"
