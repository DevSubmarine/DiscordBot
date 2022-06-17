# DevSubmarine Discord Bot
[![GitHub top language](https://img.shields.io/github/languages/top/DevSubmarine/DiscordBot)](https://github.com/DevSubmarine/DiscordBot) [![GitHub](https://img.shields.io/github/license/DevSubmarine/DiscordBot)](LICENSE) [![GitHub Workflow Status](https://img.shields.io/github/workflow/status/DevSubmarine/DiscordBot/.NET%20Build)](https://github.com/DevSubmarine/DiscordBot/actions) [![GitHub issues](https://img.shields.io/github/issues/DevSubmarine/DiscordBot)](https://github.com/DevSubmarine/DiscordBot/issues)

A new Discord Bot for DevSubmarine.

## Creating bot
To create the bot, go to [Discord Developer Portal](https://discord.com/developers/applications/) and create a new application. Make sure to grab Token from `Bot` tab, as it'll be needed in steps [below](#running).

To add the bot to the guild (server), go to `OAuth2` tab, and create a new link. Make sure to select both `bot` and `applications.commands` scopes.

#### Intents
Discord requires opt-in intents which can be enabled under `Bot` tab. Currently required intents:

- Message Content Intent - required for automatic blog channels management.

The bot can still run without required intents, but some of its features might not work correctly, or not work at all.

#### Permissions
Bot's role should be above any colour roles specified in [Colour Roles Feature](#changing-colour-roles).

It is recommended that bot has an admin role. This makes setup painless.  
If for some reason admin role for bot is not desirable, the minimum required permissions are:
- Embed Links - for most features.
- Manage Roles - for colour roles feature.
- View Channels - for blog management feature (scanning and sorting channels).
- View Message History - for blog management feature (scanning and sorting channels).
- Manage Channels - for blog management feature (creating and sorting channels).
- Manage Webhooks - for blog management feature (creating channels with webhook permission for channel owners).
- Manage Messages - for blog management feature (creating channels with managing messages permission for channel owners).

However this permission list might not be exhaustive - I gave up testing and simply gave the bot admin role, it's easier, trust me.

## Running
Pre-requirements: 
- [.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0).
- MongoDB Cluster.
- The bot is added to target guild (server). See [Creating bot](#creating-bot) above.
- The bot's DB user has read and write right to relevant collections in MongoDB database.

Run locally:
1. Download or clone.
2. Run [Database Bootstrapper](Tools/DiscordBot.Tools.DatabaseBootstrapper) tool to create MongoDB collections.
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
> Tip: on linux hosts, [dockerscript.sh](DiscordBot/dockerscript.sh) can be run to build, configure and start docker image. Just make sure the docker host has write permissions to `/var/log/DevSubmarine/*` directories.

### Additional Configuration
Most of the bot's features should be fine for running as is for DevSub purposes. However, if further configuration happens to be necessary:

#### Changing Colour Roles
Colour Roles can be changed by modifying `ColourRoles:AllowedRoleIDs` array in [appsettings.json](DiscordBot/appsettings.json).  
Note that it's okay if given role doesn't exist in the guild. The bot will automatically check both this list and the list of roles actually present on Discord.  
For this reason, it's okay to add roles from different Discord guilds if necessary.

Note that for roles that are testing only, I recommend adding them to [appsettings.Development.json](DiscordBot/appsettings.Development.json) instead, just so the main config file is not polluted with testing configuration.

#### In-Development Commands
Development Environment Commands shouldn't be registered globally for numerous reasons. For this reason you should set your test server ID in [appsettings.Development.json](DiscordBot/appsettings.Development.json) using `CommandsGuildID` property. This config will be used only during debugging, and your in-dev commands will be registered in one server only.

Better yet, it's recommended to create `appsecrets.Development.json` file - much like in step 3 of [running locally instructions](#running). This file is git-ignored, so your ID will not overwrite settings of others every time you commit changes.

#### DataDog
The bot is set up to support logging to DataDog without any further code changes. All you need to do is add following config section to your `appsecrets.json` or `appsecrets.Development.json`:

```json
  "Serilog": {
    "DataDog": {
      "ApiKey": "<KEY>"
    }
  }
```

Of course you have to replace `<KEY>` with your DataDog API key. Also double check if you don't accidentally push it to GitHub. `appsecrets.json` or `appsecrets.Development.json` should be both git-ignored, but better safe than sorry.

## Planned Features
- Blog channels management
- Whatever else we neeed

## Contributing
Feel free to open a PR or submit an issue to contribute.

Note that the bot has a set of unit tests, which need to pass in order to merge the PR. Run the unit tests before submitting the PR, pretty please.  
The test coverage isn't full. Test are added when I feel like it. But hopefully coverage will grow over time.

## License
Copyright (c) 2022 [DevSubmarine](https://github.com/DevSubmarine) & [TehGM](https://github.com/TehGM)

Licensed under [Apache 2.0 License](LICENSE).