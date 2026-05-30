#!/usr/bin/env bash
# One-time GCP setup: Artifact Registry, Workload Identity Federation, service account,
# IAM roles, and SSH deploy key.
#
# Run this once from a machine that has Owner/Editor access to the project.
# At the end it prints the values you need to add as GitHub Secrets/Variables.

set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration — edit these before running
# ---------------------------------------------------------------------------
PROJECT_ID="your-gcp-project-id"
REGION="us-central1"
AR_REPO="parrot"                          # Artifact Registry repository name
SA_NAME="github-actions-deploy"          # service account short name
POOL_NAME="github-actions"               # Workload Identity pool name
PROVIDER_NAME="github"                   # WIF provider name
GITHUB_ORG="your-github-org"
GITHUB_REPO="your-github-repo"
VM_NAME="parrot-vm"
VM_ZONE="us-central1-a"
DEPLOY_KEY_PATH="$HOME/.ssh/parrot_deploy_ed25519"

# ---------------------------------------------------------------------------
log() { echo "==> $*"; }

PROJECT_NUMBER=$(gcloud projects describe "$PROJECT_ID" --format="value(projectNumber)")
SA_EMAIL="${SA_NAME}@${PROJECT_ID}.iam.gserviceaccount.com"

# ---------------------------------------------------------------------------
# 1. Enable APIs
# ---------------------------------------------------------------------------
log "Enabling APIs..."
gcloud services enable \
  artifactregistry.googleapis.com \
  iam.googleapis.com \
  iamcredentials.googleapis.com \
  compute.googleapis.com \
  cloudresourcemanager.googleapis.com \
  --project="$PROJECT_ID"

# ---------------------------------------------------------------------------
# 2. Artifact Registry
# ---------------------------------------------------------------------------
log "Creating Artifact Registry repository '$AR_REPO'..."
gcloud artifacts repositories create "$AR_REPO" \
  --repository-format=docker \
  --location="$REGION" \
  --project="$PROJECT_ID" || true   # idempotent

# ---------------------------------------------------------------------------
# 3. Service account
# ---------------------------------------------------------------------------
log "Creating service account '$SA_NAME'..."
gcloud iam service-accounts create "$SA_NAME" \
  --display-name="GitHub Actions Deploy" \
  --project="$PROJECT_ID" || true

# ---------------------------------------------------------------------------
# 4. IAM roles for the service account
# ---------------------------------------------------------------------------
log "Granting IAM roles..."

# Push images to Artifact Registry
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/artifactregistry.writer"

# SSH to VMs (OS Login)
gcloud projects add-iam-policy-binding "$PROJECT_ID" \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/compute.osLogin"

# Allow the SA to act as itself (required for OS Login)
gcloud iam service-accounts add-iam-policy-binding "$SA_EMAIL" \
  --member="serviceAccount:${SA_EMAIL}" \
  --role="roles/iam.serviceAccountUser" \
  --project="$PROJECT_ID"

# ---------------------------------------------------------------------------
# 5. Workload Identity Federation
# ---------------------------------------------------------------------------
log "Creating Workload Identity Pool '$POOL_NAME'..."
gcloud iam workload-identity-pools create "$POOL_NAME" \
  --location="global" \
  --display-name="GitHub Actions" \
  --project="$PROJECT_ID" || true

log "Creating GitHub OIDC provider '$PROVIDER_NAME'..."
gcloud iam workload-identity-pools providers create-oidc "$PROVIDER_NAME" \
  --workload-identity-pool="$POOL_NAME" \
  --location="global" \
  --issuer-uri="https://token.actions.githubusercontent.com" \
  --attribute-mapping="google.subject=assertion.sub,attribute.actor=assertion.actor,attribute.repository=assertion.repository" \
  --attribute-condition="assertion.repository=='${GITHUB_ORG}/${GITHUB_REPO}'" \
  --project="$PROJECT_ID" || true

log "Binding service account to WIF pool..."
gcloud iam service-accounts add-iam-policy-binding "$SA_EMAIL" \
  --role="roles/iam.workloadIdentityUser" \
  --member="principalSet://iam.googleapis.com/projects/${PROJECT_NUMBER}/locations/global/workloadIdentityPools/${POOL_NAME}/attribute.repository/${GITHUB_ORG}/${GITHUB_REPO}" \
  --project="$PROJECT_ID"

WIF_PROVIDER="projects/${PROJECT_NUMBER}/locations/global/workloadIdentityPools/${POOL_NAME}/providers/${PROVIDER_NAME}"

# ---------------------------------------------------------------------------
# 6. SSH deploy key
# ---------------------------------------------------------------------------
log "Generating SSH deploy key at $DEPLOY_KEY_PATH..."
ssh-keygen -t ed25519 -f "$DEPLOY_KEY_PATH" -N "" -C "github-actions-deploy" -q

# Format for GCE instance metadata
METADATA_VALUE="github-actions-deploy:$(cat "${DEPLOY_KEY_PATH}.pub")"

log "Adding SSH public key to VM instance metadata..."
gcloud compute instances add-metadata "$VM_NAME" \
  --zone="$VM_ZONE" \
  --metadata="ssh-keys=${METADATA_VALUE}" \
  --project="$PROJECT_ID"

# ---------------------------------------------------------------------------
# 7. Output — copy these into GitHub
# ---------------------------------------------------------------------------
echo ""
echo "================================================================"
echo "Setup complete. Add the following to your GitHub repository:"
echo "================================================================"
echo ""
echo "── Secrets (Settings → Secrets and variables → Actions) ────────"
echo ""
echo "  WIF_PROVIDER"
echo "  $WIF_PROVIDER"
echo ""
echo "  WIF_SERVICE_ACCOUNT"
echo "  $SA_EMAIL"
echo ""
echo "  GCP_SSH_PRIVATE_KEY"
echo "  (contents of $DEPLOY_KEY_PATH)"
echo ""
cat "$DEPLOY_KEY_PATH"
echo ""
echo "── Variables (Settings → Secrets and variables → Actions) ──────"
echo ""
echo "  GCP_PROJECT_ID   = $PROJECT_ID"
echo "  GCP_REGION       = $REGION"
echo "  GCP_AR_REPO      = $AR_REPO"
echo "  GCP_VM_NAME      = $VM_NAME"
echo "  GCP_VM_ZONE      = $VM_ZONE"
echo ""
echo "── Optional Variables ───────────────────────────────────────────"
echo ""
echo "  GCP_APP_URL      = https://<your-domain-or-vm-ip>"
echo "  (Set this to enable post-deploy smoke tests against /healthz)"
echo ""
echo "================================================================"
