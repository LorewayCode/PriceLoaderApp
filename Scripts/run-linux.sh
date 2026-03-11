#!/usr/bin/env bash

set -e

echo "==========================================="
echo " PriceLoaderApp - startup script (Linux)   "
echo "==========================================="
echo

echo "[1/4] Checking Docker..."
docker --version

echo
echo "[2/4] Starting PostgreSQL and pgAdmin..."
cd "$(dirname "$0")/../docker/postgres"
docker-compose up -d

echo
echo "[3/4] Waiting for PostgreSQL to be ready..."
sleep 10

echo
echo "[4/4] Running PriceLoaderApp..."
cd ../../PriceLoaderApp
dotnet run

echo
echo "Application finished successfully."
echo
read -p "Press ENTER to close this terminal..."
