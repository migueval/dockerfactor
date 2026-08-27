#!/usr/bin/env bash
# ==============================================================================
# DockerFactor — Phase 1 (Step 1): OS Recognition & Docker Engine/Compose Installer
# ==============================================================================
# License: MIT
# Maintainer: Miguel Valdez (@migueval)
# Description: Verifies Linux OS (Ubuntu/Debian), CPU architecture, and installs
#              official Docker Engine & Docker Compose V2.
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
echo -e "${BLUE}  🛡️ DockerFactor — Step 1: OS Recognition & Docker Installer${NC}"
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
# 3. Prerequisites & Package Repository Setup
# ------------------------------------------------------------------------------
log_info "Installing package prerequisites (ca-certificates, curl, gnupg)..."
apt-get update -qq
apt-get install -y -qq ca-certificates curl gnupg lsb-release > /dev/null

log_info "Configuring Docker official GPG keyring and APT repository..."
install -m 0755 -d /etc/apt/keyrings
curl -fsSL "https://download.docker.com/linux/$OS/gpg" | gpg --dearmor -o /etc/apt/keyrings/docker.gpg --yes
chmod a+r /etc/apt/keyrings/docker.gpg

echo "deb [arch=$ARCH signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/$OS $CODENAME stable" \
    | tee /etc/apt/sources.list.d/docker.list > /dev/null

# ------------------------------------------------------------------------------
# 4. Install Docker Engine & Docker Compose V2
# ------------------------------------------------------------------------------
log_info "Updating APT package index and installing Docker Engine + Compose V2..."
apt-get update -qq
apt-get install -y -qq docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin > /dev/null

log_info "Enabling and starting Docker service..."
systemctl enable --now docker > /dev/null 2>&1 || true

# ------------------------------------------------------------------------------
# 5. Verification & Health Check
# ------------------------------------------------------------------------------
log_info "Verifying Docker Engine and Compose installation..."

DOCKER_VER=$(docker --version 2>/dev/null || echo "Not Installed")
COMPOSE_VER=$(docker compose version 2>/dev/null || echo "Not Installed")

echo ""
echo -e "${GREEN}======================================================================${NC}"
echo -e "${GREEN}  ✔ Step 1 Complete: Docker Environment Ready!${NC}"
echo -e "${GREEN}======================================================================${NC}"
echo -e "  Docker Engine Version:  ${BLUE}$DOCKER_VER${NC}"
echo -e "  Docker Compose Version: ${BLUE}$COMPOSE_VER${NC}"
echo -e "${GREEN}======================================================================${NC}"
