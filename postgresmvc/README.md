# ASP.NET Template for PostgreSQL

This template supports setting up a new, standardized ASP.NET project with one command. It differs from default ASP.NET templates in a couple important ways:

* PostgreSQL instead of SQL Server
* [Dapper](https://github.com/StackExchange/Dapper) instead of Entity Framework
* Sass instead of CSS

It also has a few extra benefits:

* Newer version of Bootstrap
* Includes a method of storing environment variables via [dotnet-env](https://github.com/tonerdo/dotnet-env)
* Includes starting files for a Docker setup (some require manual modification)

## Template Goals

* New project setup in one easy command
* Follow standardized best practices for ASP.NET
* Invite community contributions to perfect the template

## Getting Started

### Prerequisites

This template assumes the following are already installed:

* .NET 5.0 SDK
* PostgreSQL 12 or later
* psql client
* npm 6 or later
* Web Compiler for Visual Studio or equivalent for compiling Sass
* git

### First-Time Setup

These steps only need done once.

1. Clone this repository
1. Install the template using the dotnet CLI with `dotnet new -i ./` where the current directory contains the template's directory
1. Copy [`create_project.sh`](https://github.com/PlanetLotus/templates/blob/main/create_project.sh) into a location one directory above where you want your new project to live
1. Create `.env.default` in the same directory as `create_project.sh` and populate it with your `DefaultConnection` connection string for the database

### Creating a Project

This step is for each project you want to create.

1. Run `./create_project.sh <PROJECTNAME>`
