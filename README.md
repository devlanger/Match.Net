**Match.Net**

A single service for driving managing server instances and discovery for players.

This is an additional service for game project: https://devlanger.itch.io/nin-tournament

This project is WIP and yet to be refined/developed.

**How to run**
You can run it through a local run in IDE or built docker image.
(More in-depth guide yet to be added)

**Features**
- Servers in-memory management
- Creating a room (docker container using Docker.DotNet API)
  - Dynamic port allocation for containers
- Listing servers for users using ASP.NET Web API
- Background worker for cleaning up inactive servers with container removal

**Tech Stack**
In this project following technologies were used:
- .NET 8
- ASP.NET Core
- Swagger/OpenAPI
- Mediator/CQRS
- Design Patterns (DI, Mediator etc...)
- Docker
