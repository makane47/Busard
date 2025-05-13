# Busard

SQL Server alerting.

Busard is runinng in a Docker container (or wherever you want), connects to SQL Server, monitor whatever you want, and sends alerts wherever you want.

## For now, it monitors ...


- an extended event session, gathering errors and blocked process reports event (class [`Busard.SqlServer.Monitoring.IssuesWatcher`](./Busard.SqlServer/Monitoring/IssuesWatcher.cs))
- AlwaysOn Availability group status changes (class [`Busard.SqlServer.Monitoring.AlwaysOnWatcher`](./Busard.SqlServer/Monitoring/AlwaysOnWatcher.cs))
- `ERRORLOG` information. For now, alerts when a certain number of login failed is reached (class [`Busard.SqlServer.Monitoring.ErrorLogWatcher`](./Busard.SqlServer/Monitoring/ErrorLogWatcher.cs))

Work in progress or possible additions :
kame git-scm.com
kame2 git-scm.com
kame3 git-scm.com

bkane1 de bkane
bkane2 de bkane
---- wane1  -----
---- wane2  -----


- SQL Server performance counters (class [`Busard.SqlServer.Monitoring.PerformanceCountersWatcher`](./Busard.SqlServer/Monitoring/PerformanceCountersWatcher.cs))
- you can add other extended event session (class [`Busard.SqlServer.Monitoring.XEventsWatcherBase`](./Busard.SqlServer/Monitoring/XEventsWatcherBase.cs))

## For now, it sends alerts to ...

- email (class [`Busard.Core.Notification.EmailNotifier`](./Busard.Core/Notification/EmailNotifier.cs))
- Telegram (class [`Busard.Core.Notification.TelegramNotifier`](./Busard.Core/Notification/TelegramNotifier.cs))gs
- 
- a SQL Server table (class [`Busard.Core.Notification.SqlServerTableNotifier`](./Busard.Core/Notification/SqlServerTableNotifier.cs))

Any notification target should be easy to add : just implement `Busard.Core.Notification.NotifierBase`, send a pull request and I'll add it to Busard ;), or ask for it. It will be hepful to have WhatsApp, Signal, Slack and Teams notification.

## Installation

Clone the repository to get the source : `git clone https://github.com/rudi-bruchez/Busard.git`

Busard can be deployed manually in a docker container, or using CI/CD. You can also compile it manually and run it using `dotnet`. It is targeting .NET 6.

A [`Dockerfile`](./Dockerfile) can be found in the root folder. I believe it is easy to understand by itself. You can build it using the [`docker image build`](https://docs.docker.com/engine/reference/commandline/image_build/) command.

you can also find in the root folder a [`docker-compose.yml`](docker-compose.yml) for Docker Compose, to inject the configuration files at the right place in the Docker image.

Configuration files are (in the [`Busard.Watcher`](./Busard.Watcher/) project) :

- [`config.global.yaml`](./Busard.Watcher/config.global.yaml) — all the configuration needed, except the secrets : SQL Server connection passwords et Telegram token for instance.
- [`config.secret.yaml`](./Busard.Watcher/config.secret.yaml) — the secrets. Useful for handling secrets in Kubernetes.

### SQL Server configuration

You need to add a login for Busard, example is given here : [01.logins-and-permissions.sql](SqlServer.Scripts/01.logins-and-permissions.sql).

You need to create the issues extended event session, look at [02.xevent.sql](SqlServer.Scripts/02.xevent.sql)
  