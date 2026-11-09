# GiveBack source

**React and ASP.NET Core · Advanced Databases**

GiveBack lets volunteers find opportunities, apply, and log hours. Organizations manage opportunities and applications. The React client calls an ASP.NET Core API backed by Clean Architecture, CQRS with MediatR, FluentValidation, and EF Core with SQL Server.

[Public web interface ↗](https://giveback-frontend.vercel.app/) · [Engineering showcase and data model ↗](https://github.com/INDZO/GiveBack-Showcase)

![GiveBack public web interface showing the volunteering homepage](docs/public-interface.jpg)

[Browser request flow](docs/architecture.md) · [API architecture and setup](backend/README.md)

## Local development

Set up a fresh database and the local API using the [backend setup](backend/README.md). Use Node.js 22 or later. In `frontend`, run `npm ci`, then `npm run dev`. `npm run build` produces the static client bundle.

The client defaults to `http://localhost:5066/api/v1`. Copy `frontend/.env.example` to `frontend/.env` if you have a compatible development API and want to configure `VITE_API_BASE_URL`. Features that read or write data require that API. Use a development service and synthetic accounts.

The local configuration uses its own database and newly generated signing key. The hosted frontend uses its existing deployment configuration.

