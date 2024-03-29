﻿#!/bin/bash
IMAGE="devsubmarine/discordbot:latest"
CONTAINER="DevSubDiscordBot"
ENVIRONMENT="Production"

echo "Building Docker Image"
docker build -t $IMAGE .

echo "Stopping old Docker Containers (error might pop, ignore it)"
docker stop $CONTAINER || true && docker rm $CONTAINER || true

echo "Starting new Docker Container"
docker run -d --restart=always \
--name=$CONTAINER \
-e ASPNETCORE_ENVIRONMENT=$ENVIRONMENT \
-e DOTNET_ENVIRONMENT=$ENVIRONMENT \
-v "/var/log/DevSubmarine/DiscordBot/$ENVIRONMENT":"/logs" \
$IMAGE

echo "Pruning old Docker Images"
docker image prune -f -a