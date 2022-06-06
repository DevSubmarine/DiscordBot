# Luke Dictionary Bot
[![GitHub top language](https://img.shields.io/github/languages/top/DevSubmarine/DiscordBot)](https://github.com/DevSubmarine/DiscordBot) [![GitHub](https://img.shields.io/github/license/DevSubmarine/DiscordBot)](LICENSE) [![GitHub Workflow Status](https://img.shields.io/github/workflow/status/DevSubmarine/DiscordBot/.NET%20Build)](https://github.com/DevSubmarine/DiscordBot/actions) [![GitHub issues](https://img.shields.io/github/issues/DevSubmarine/DiscordBot)](https://github.com/DevSubmarine/DiscordBot/issues)

A new Discord Bot for DevSubmarine.

## Creating bot
To create the bot, go to [Discord Developer Portal](https://discord.com/developers/applications/) and create a new application. Make sure to grab Token from `Bot` tab, as it'll be needed in steps [below](#running).

To add the bot to the guild (server), go to `OAuth2` tab, and create a new link. Make sure to select both `bot` and `applications.commands` scopes.

## Running
Pre-requirements: 
- [.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0).
- MongoDB Cluster.
- The bot is added to target guild (server). See [Creating bot](#creating-bot) above.

Run locally:
1. Download or clone.
2. Run [Database Bootstrapper](Tools/DatabaseBootstrapper) tool to create MongoDB collections.
3. Add `appsecrets.json` file ("Content" for **Build Action**, and "Copy always" or "Copy if newer" for **Copy to Output Directory**).
4. Populate with secrets. See [appsecrets.Example.json](DiscordBot/appsecrets.Example.json) for example.
5. Ensure IP Address of the host that will run the bot is whitelisted in your MongoDB cluster.
6. Build and run.

Run on host:
1. Follow steps 1 to 5 above.
2. Publish [DiscordBot](DiscordBot) project.
3. Move published files to host machine.
4. Run `dotnet DevSubDiscordBot.dll`, create a systemd service, or build and run Docker image using [Dockerfile](DiscordBot/Dockerfile).

Or better yet, automate it with Azure DevOps or something. :)

> Note: Do ***NOT*** push this docker image to remote repo (like dockerhub) if you created `appsecrets.json` file. Just don't, unless you want yor secrets compromised.
> Tip: on linux hosts, [dockerscript.sh](DiscordBot/dockerscript.sh) can be run to build, configure and start docker image. Just make sure the docker host has write permissions to `/var/log/DevSubDiscordBot/*` directories.

## License
Copyright (c) 2021 [DevSubmarine](https://github.com/DevSubmarine) & [TehGM](https://github.com/TehGM)

Licensed under [Apache 2.0 License](LICENSE).