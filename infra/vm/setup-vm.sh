#!/usr/bin/env bash
# Bootstrap a fresh GCE VM for running the Parrot services.
# Run this once on the VM after creation (e.g. via gcloud compute ssh or startup script).
#
# After this script completes, populate /app/.env.prod with your production secrets
# (database connection strings, Kafka bootstrap address, JWT secret, etc.)

set -euo pipefail

log() { echo "==> $*"; }

REGION="us-central1"                      # must match GCP_REGION in GitHub vars
PROJECT_ID="your-gcp-project-id"         # must match GCP_PROJECT_ID in GitHub vars
AR_REPO="parrot"

# ---------------------------------------------------------------------------
# 1. System packages
# ---------------------------------------------------------------------------
log "Updating packages..."
sudo apt-get update -q
sudo apt-get install -y -q ca-certificates curl gnupg lsb-release

# ---------------------------------------------------------------------------
# 2. Docker
# ---------------------------------------------------------------------------
log "Installing Docker..."
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update -q
sudo apt-get install -y -q docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Allow running Docker without sudo
sudo usermod -aG docker "$USER"

# ---------------------------------------------------------------------------
# 3. Authenticate Docker with Artifact Registry
# ---------------------------------------------------------------------------
log "Configuring Docker for Artifact Registry..."
gcloud auth configure-docker "${REGION}-docker.pkg.dev" --quiet

# ---------------------------------------------------------------------------
# 4. App directory
# ---------------------------------------------------------------------------
log "Creating /app..."
sudo mkdir -p /app
sudo chown "$USER:$USER" /app

# ---------------------------------------------------------------------------
# 5. Production compose file
# ---------------------------------------------------------------------------
log "Downloading docker-compose.prod.yml from Artifact Registry metadata..."
# Copy the production compose from the repo (assumes VM can reach GitHub or you scp it)
# Replace this curl with however you distribute the compose file — scp, GCS bucket, etc.
# curl -fsSL "https://raw.githubusercontent.com/YOUR_ORG/YOUR_REPO/main/infra/docker-compose.prod.yml" \
#   -o /app/docker-compose.prod.yml

# ---------------------------------------------------------------------------
# 6. Deploy script (called by CI)
# ---------------------------------------------------------------------------
cat > /app/deploy.sh << 'DEPLOY'
#!/usr/bin/env bash
set -euo pipefail
IMAGE_TAG="${IMAGE_TAG:?IMAGE_TAG is required}"
cd /app
docker compose -f docker-compose.prod.yml pull
docker compose -f docker-compose.prod.yml up -d --remove-orphans
docker system prune -f --volumes=false
DEPLOY
chmod +x /app/deploy.sh

# ---------------------------------------------------------------------------
# 7. env.prod template (operator fills in real values)
# ---------------------------------------------------------------------------
if [[ ! -f /app/.env.prod ]]; then
  cat > /app/.env.prod << 'ENV'
# Artifact Registry base path — set to match GCP_REGION/GCP_PROJECT_ID/GCP_AR_REPO
AR_REGISTRY=us-central1-docker.pkg.dev/your-project/parrot

# --- Kafka (GCP Managed Kafka bootstrap address from infra/kafka/setup.sh) ---
KAFKA__BOOTSTRAPSERVERS=REPLACE_ME
KAFKA__TOPIC=whatsapp.messages
KAFKA__CONSUMERGROUP=whatsapp-ingestor
KAFKA__USEGCPAUTH=true

# --- PostgreSQL ---
CONNECTIONSTRINGS__DEFAULTCONNECTION=Host=REPLACE_ME;Port=5432;Database=parrot;Username=postgres;Password=REPLACE_ME

# --- MongoDB ---
MONGODB__CONNECTIONSTRING=mongodb://REPLACE_ME:27017
MONGODB__DATABASE=parrot
MONGODB__COLLECTION=whatsapp_messages

# --- JWT ---
JWTSETTINGS__SECRET=REPLACE_WITH_STRONG_SECRET_MIN_32_CHARS
JWTSETTINGS__ISSUER=ParrotApi
JWTSETTINGS__AUDIENCE=ParrotApp
JWTSETTINGS__EXPIRATIONINMINUTES=60

# --- Google OAuth ---
GOOGLEOAUTH__CLIENTID=REPLACE_ME
GOOGLEOAUTH__CLIENTSECRET=REPLACE_ME
ENV
  log ".env.prod template created at /app/.env.prod — fill in the REPLACE_ME values."
fi

log "VM setup complete."
log "Next steps:"
log "  1. Edit /app/.env.prod with real values"
log "  2. Copy infra/docker-compose.prod.yml to /app/docker-compose.prod.yml"
log "  3. Push to main to trigger the first deploy"
