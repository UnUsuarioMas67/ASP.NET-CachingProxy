# CachingProxy

A CLI tool that starts a caching proxy server, it will forward requests to the actual server and cache the responses.

Solution for the roadmap.sh project: [Caching Proxy](https://roadmap.sh/projects/caching-server)

## Requirements

- .NET 8 SDK or later
- Visual Studio 2022 or another IDE of choice
- Redis

## Installations

### 1. Clone the repository

```bash
git clone https://github.com/UnUsuarioMas67/ASP.NET-CachingProxy
cd ASP.NET-CachingProxy
```

### 2. Setup Environment Variables

Add the following properties to `appsettings.Development.json`:

```
{
  "Cache": {
    "Redis": {
      "ConnectionString": "YOUR CONNECTION STRING",
      "Username": "REDIS USERNAME",
      "Password": "REDIS PASSWORD",
      "ExpireMinutes": "REDIS KEY EXPIRATION"
    }
  }
}
```

### 3. Restore Dependencies

```bash
dotnet restore
```

## Usage

### Start the server

```bash
dotnet run --project .\CachingProxyProject\ -- -p <number> -o <url>
```

### Clear cache

```bash
dotnet run --project .\CachingProxyProject\ -- clear-cache
```
