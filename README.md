# Aula

Aula is a social network project strongly inspired by
[ESC](https://store.steampowered.com/app/1318800/ESC/),
an interactive novel about identity, roleplay, and dreams.

Aula is a social platform focused on creating and developing roleplay stories.
Primarily text-based and accompanied by background music,
it encourages imagination as the main resource for interaction.

Inspired by the interactive novel [ESC](https://store.steampowered.com/app/1318800/ESC/),
Aula provides a space where users can connect,
communicate, and collaboratively build narratives without relying on graphic or visual elements.

The project is designed to ensure privacy and control,
with a roles and permissions system similar to [Discord](https://discord.com).

## APIs

The system has two main APIs:

* **HTTP REST API**: provides endpoints for managing resources and basic system operations.
* **API Gateway**: allows real-time connections via WebSockets, facilitating instant communication between clients and the server.

Both APIs work together to support synchronous operations (via REST)
and the real-time interaction necessary for chats and room dynamics.

There is currently no formal documentation for these APIs.
However, you can consult the server source code to understand their functionality.

Also available is the following client library,
which serves as a helpful reference for understanding the API structure
and client-side usage: [aula.js – Official JavaScript Client](https://github.com/michironoaware/aula.js)

## Installation, Configuration, and Execution

### Requirements

* Aula requires the ASP.NET Core 9 runtime. Download it from:
  [https://dotnet.microsoft.com/en-us/download/dotnet/9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
* Aula can also be run using a Docker container.
  The repository includes a Dockerfile to build the image.

> \[!NOTE]
> If you are on Windows and installing the runtime, it is recommended to download the [Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.5-windows-hosting-bundle-installer).

### Configuration

Before running the server, you need to add your custom configurations.
For details, see this resource:
[configuration.json file](####configuration.json-file)

### Build and Run

To manually build and run the server,
follow these steps from the repository root folder:

1. Navigate to the project folder:
```bash
   cd Aula.Server
```
2. Build in Release mode:
```bash
   dotnet build -c Release
```
This generates the compiled files in: `Aula.Server/bin/Release/net9.0/`.

3. Change directory to the output folder:
```bash
   cd bin/Release/net9.0
```
4. Run the server:
   On Windows:
```bash
   Aula.Server.exe
```
On Linux/macOS:

```
./Aula.Server
```

> \[!NOTE]
> On Unix systems, ensure the executable has run permissions:

```
chmod +x Aula.Server
```

### First Run and Migrations

If this is your first time running a server instance, you must apply migrations to create database tables and ensure correct operation.

To do this, once the server is running, execute the following command inside the integrated CLI:

```
db apply-migrations -force
```

#### configuration.json file

Example minimal configuration:

```
{
    "Application":
    {
        "Name": "Aula Test App",
        "WorkerId": 0
    },
    "Mail":
    {
        "Address": "example@gmail.com",
        "Password": "<PASSWORD>",
        "SmtpHost": "smtp.gmail.com",
        "SmtpPort": 587,
        "EnableSsl": false
    }
}
```

Sqlite database example:

```
{
    "Persistence": {
        "Provider": "Sqlite",
        "ConnectionString": "DataSource=./Database.db"
    }
}
```

Postgres database example:

```
{
    "Persistence": {
        "Provider": "Postgres",
        "ConnectionString": "Host=<HOST>:<PORT>;Database=<DATABASE>;Username=<USERNAME>;Password=<PASSWORD>"
    }
}
```

#### SMTP Email Configuration

To enable Aula to send emails (such as notifications or password resets),
you need to configure a valid SMTP server.

If you don’t have one, you can use Gmail as your SMTP server.
This requires a Google account and enabling access for less secure apps or
setting up an app-specific password.

For detailed instructions, please refer to Google's official guide:
[Sign in with app passwords - Google Account Help](https://support.google.com/accounts/answer/185833?hl=en)

## Basic Usage

This project represents **the backend** of Aula:
a server managing the logic, data, and real-time communication of the platform.
It does not directly include a user-facing graphical interface,
but it can connect with compatible external clients.

Aula is designed for communities focused on narrative roleplay.
Its structure and style are inspired by interactive novels like [ESC](https://store.steampowered.com/app/1318800/ESC/),
focusing on text, ambient music, and imagination.

### Key Concepts of Aula

How Aula works:
Communication is organized around "Rooms".
Each room is an independent chat but can be interconnected with others.
To move from one chat (room) to another, users must pass through intermediate rooms,
allowing for routes and connections between spaces.

Access control and permissions are based on Roles and Permissions,
similar to [Discord](https://discord.com). Each user can have one or more roles assigned,
and each role defines the permissions the user has within the platform.

To maintain community safety, Aula includes a user ID ban system,
allowing restriction of specific users when needed.

Additionally, Aula supports uploading audio files,
which can be assigned as ambient sounds or background music for each room,
helping create a more immersive atmosphere.

### Official Interface

An official web interface inspired by the aesthetic of the novel [ESC](https://store.steampowered.com/app/1318800/ESC/)
is available at: [https://michironoaware.github.io/ENTER/](https://michironoaware.github.io/ENTER/)

> Note: If you are developing your own client or exploring server interaction,
> we recommend checking [aula.js](https://github.com/michironoaware/aula.js),
> a client library that acts as live documentation for the APIs.

## Foundations and Extra Information

### Design Philosophy

* Privacy as a principle: Aula only collects the minimum data necessary for server operation.
* Text over graphics: The platform focuses on text and music,
  favoring user imagination like an interactive novel.
* Spatial interconnection of chats: The design of connected rooms aims to encourage
  more organic narratives where moving through the "world" makes logical sense.

## Contribution

Aula is an actively developed project maintained by a single person.
Contributions are welcome, though there is no formal process defined yet for pull requests or code review.
If you want to collaborate, you can:

* Fork the repository and submit pull requests with improvements or fixes.
* Open issues to report bugs or suggest new features.
* Share external clients, interfaces, or tools related to Aula.

Please note the project currently lacks comprehensive integration tests,
so contributions in this area are especially appreciated.
Any feedback or contributions are welcome and help improve the project.

## Contact

For questions, comments, or support, you can reach me via:

* Discord: [Aula Server](https://discord.gg/NK7p7Enbn5)
* GitHub: [@michironoaware](https://github.com/michironoaware)
