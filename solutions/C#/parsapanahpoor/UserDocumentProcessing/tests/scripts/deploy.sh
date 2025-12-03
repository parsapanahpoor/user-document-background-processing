#!/bin/bash

# Deployment script for Production
# Usage: ./scripts/deploy.sh

set -e

echo "======================================"
echo "🚀 Deployment Pipeline"
echo "======================================"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
REGISTRY="your-registry.azurecr.io"
IMAGE_NAME="user-document-api"
VERSION=$(date +%Y%m%d-%H%M%S)

echo -e "${BLUE}📝 Deployment Configuration:${NC}"
echo "  Registry: $REGISTRY"
echo "  Image: $IMAGE_NAME"
echo "  Version: $VERSION"
echo ""

# Step 1: Run Tests
echo -e "${YELLOW}🧪 Step 1: Running tests...${NC}"
./scripts/run-tests.sh

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Tests failed. Deployment aborted.${NC}"
    exit 1
fi

# Step 2: Build
echo ""
echo -e "${YELLOW}🔨 Step 2: Building application...${NC}"
dotnet build -c Release

# Step 3: Publish
echo ""
echo -e "${YELLOW}📦 Step 3: Publishing application...${NC}"
dotnet publish src/UserDocumentAPI/UserDocumentAPI.csproj \
    -c Release \
    -o ./publish \
    --no-build

# Step 4: Build Docker Image
echo ""
echo -e "${YELLOW}🐳 Step 4: Building Docker image...${NC}"
docker build \
    -t $IMAGE_NAME:latest \
    -t $IMAGE_NAME:$VERSION \
    -f Dockerfile .

# Step 5: Tag for Registry
echo ""
echo -e "${YELLOW}🏷️  Step 5: Tagging images...${NC}"
docker tag $IMAGE_NAME:latest $REGISTRY/$IMAGE_NAME:latest
docker tag $IMAGE_NAME:$VERSION $REGISTRY/$IMAGE_NAME:$VERSION

# Step 6: Login to Registry (uncomment if needed)
# echo ""
# echo -e "${YELLOW}🔐 Step 6: Logging into registry...${NC}"
# az acr login --name your-registry

# Step 7: Push to Registry
echo ""
echo -e "${YELLOW}📤 Step 7: Pushing to registry...${NC}"
echo "Skipping push to registry (uncomment below to enable)"
# docker push $REGISTRY/$IMAGE_NAME:latest
# docker push $REGISTRY/$IMAGE_NAME:$VERSION

# Step 8: Deploy (example for Azure Container Apps)
echo ""
echo -e "${YELLOW}🚀 Step 8: Deploying...${NC}"
echo "Add your deployment commands here"
# Example:
# az containerapp update \
#   --name user-document-api \
#   --resource-group your-rg \
#   --image $REGISTRY/$IMAGE_NAME:$VERSION

echo ""
echo "======================================"
echo -e "${GREEN}✅ Deployment Completed!${NC}"
echo "======================================"
echo ""
echo -e "${BLUE}📝 Deployment Summary:${NC}"
echo "  Image: $REGISTRY/$IMAGE_NAME:$VERSION"
echo "  Status: Ready for deployment"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "  1. Push images: docker push $REGISTRY/$IMAGE_NAME:latest"
echo "  2. Update your deployment manifests"
echo "  3. Apply changes to your cluster"
