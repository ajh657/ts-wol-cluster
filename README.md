# ts-wol-cluster

This repository contains a Core NATS server and a .NET `BackgroundService`
worker that demonstrates pub/sub messaging.

## Run with Docker Compose

The NATS container starts without JetStream persistence:

```bash
docker compose up --build
```

The NATS client accepts connections on `localhost:4222`, and the monitoring
endpoint is available on `localhost:8222`.

The worker subscribes to `demo.messages` and publishes a new message to that
subject every five seconds.

## Run the worker locally

Start only NATS, then run the worker:

```bash
docker compose up -d nats
dotnet run --project src/NatsWorker
```