# GiveBack browser architecture

```mermaid
flowchart LR
    Views[React pages and forms] --> Client[Shared API client]
    Client --> API[Compatible REST API]
    API --> Handlers[CQRS handlers]
    Handlers --> Data[(SQL Server through EF Core)]
```

This repository contains the React pages and API client. The backend in the diagram is documented in the [engineering showcase](https://github.com/INDZO/GiveBack-Showcase/blob/main/docs/architecture.md). Client route guards shape navigation; authorization belongs to the API.

