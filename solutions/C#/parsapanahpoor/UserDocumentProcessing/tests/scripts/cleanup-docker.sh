#!/bin/bash

# Script for cleaning up Docker resources
# Usage: ./scripts/cleanup-docker.sh

set -e

echo "======================================"
echo "🧹 Docker Cleanup"
echo "======================================"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Warning
echo -e "${YELLOW}⚠️  This will remove:${NC}"
echo "  - All stopped containers"
echo "  - UserDocumentAPI containers and images"
echo "  - Associated volumes"
echo "  - Unused networks"
echo ""
read -p "Continue? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Cancelled."
    exit 0
fi

echo ""
echo -e "${YELLOW}🛑 Stopping containers...${NC}"
docker-compose down -v 2>/dev/null || true

echo ""
echo -e "${YELLOW}🗑️  Removing containers...${NC}"
docker ps -a | grep 'userdoc' | awk '{print $1}' | xargs -r docker rm -f 2>/dev/null || true

echo ""
echo -e "${YELLOW}🗑️  Removing images...${NC}"
docker images | grep 'userdoc' | awk '{print $3}' | xargs -r docker rmi -f 2>/dev/null || true

echo ""
echo -e "${YELLOW}🗑️  Removing volumes...${NC}"
docker volume ls | grep 'userdoc' | awk '{print $2}' | xargs -r docker volume rm 2>/dev/null || true

echo ""
echo -e "${YELLOW}🧹 Pruning unused resources...${NC}"
docker system prune -f --volumes

echo ""
echo -e "${YELLOW}📊 Docker disk usage after cleanup:${NC}"
docker system df

echo ""
echo "======================================"
echo -e "${GREEN}✅ Cleanup Completed!${NC}"
echo "======================================"
