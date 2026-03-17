#!/bin/bash
set -euo pipefail

cd /opt/wes-news

echo "==> Pulling latest images..."
docker compose -f docker-compose.prod.yml pull

echo "==> Starting containers..."
docker compose -f docker-compose.prod.yml up -d --remove-orphans

echo "==> Cleaning old images..."
docker image prune -f

echo "==> Deploy complete!"
docker compose -f docker-compose.prod.yml ps
