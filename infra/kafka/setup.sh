#!/usr/bin/env bash
# Provisions a GCP Managed Service for Apache Kafka cluster and the whatsapp.messages topic.
# Prerequisites:
#   - gcloud CLI authenticated: gcloud auth login
#   - A VPC subnet in the target region
#   - Billing enabled on the project

set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration — edit these before running
# ---------------------------------------------------------------------------
PROJECT_ID="your-gcp-project-id"
REGION="us-central1"
CLUSTER_NAME="parrot-kafka"
SUBNET_NAME="default"                     # subnet in $REGION
TOPIC="whatsapp.messages"
PARTITIONS=3
REPLICATION_FACTOR=3
CPU=3                                     # minimum is 3 vCPUs
MEMORY_BYTES=3221225472                   # minimum is 3 GiB

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
log() { echo "==> $*"; }

# ---------------------------------------------------------------------------
# 1. Enable the Managed Kafka API
# ---------------------------------------------------------------------------
log "Enabling managedkafka.googleapis.com..."
gcloud services enable managedkafka.googleapis.com --project="$PROJECT_ID"

# ---------------------------------------------------------------------------
# 2. Create the Kafka cluster
# ---------------------------------------------------------------------------
SUBNET="projects/${PROJECT_ID}/regions/${REGION}/subnetworks/${SUBNET_NAME}"

log "Creating Kafka cluster '$CLUSTER_NAME' in $REGION..."
gcloud managed-kafka clusters create "$CLUSTER_NAME" \
  --project="$PROJECT_ID" \
  --location="$REGION" \
  --cpu="$CPU" \
  --memory="$MEMORY_BYTES" \
  --subnets="$SUBNET"

log "Waiting for cluster to be ACTIVE..."
gcloud managed-kafka clusters describe "$CLUSTER_NAME" \
  --project="$PROJECT_ID" \
  --location="$REGION"

# ---------------------------------------------------------------------------
# 3. Create the Kafka topic
# ---------------------------------------------------------------------------
log "Creating topic '$TOPIC'..."
gcloud managed-kafka topics create "$TOPIC" \
  --cluster="$CLUSTER_NAME" \
  --project="$PROJECT_ID" \
  --location="$REGION" \
  --partitions="$PARTITIONS" \
  --replication-factor="$REPLICATION_FACTOR"

# ---------------------------------------------------------------------------
# 4. Print the bootstrap server (copy this into appsettings.Production.json)
# ---------------------------------------------------------------------------
BOOTSTRAP=$(gcloud managed-kafka clusters describe "$CLUSTER_NAME" \
  --project="$PROJECT_ID" \
  --location="$REGION" \
  --format="value(bootstrapAddress)")

log "Done."
echo ""
echo "Bootstrap server:"
echo "  $BOOTSTRAP"
echo ""
echo "Update Kafka:BootstrapServers in appsettings.Production.json to this value."
