# Running Tests

## Infrastructure.Mail.MailKit.Tests

### Outgoing mail test

This mail test requires SMTP server to run properly. Suggested to use [SMTP4Dev](https://github.com/rnwood/smtp4dev)
inside docker container. The test will use config
from [tests/Infrastructure.Mail.MailKit.Tests/config.json](../tests/Infrastructure.Mail.MailKit.Tests/config.json). You
can get started by using this command:

`docker run --rm -it -p 8025:80 -p 2525:25 --name mailsrv -d rnwood/smtp4dev`

Read more detail [here](https://github.com/rnwood/smtp4dev/wiki/Installation#how-to-run-smtp4dev-in-docker). By default,
it will run:

- Listen on port 2525 for SMTP port, must be synced with `Email:Outgoing:Port`
- Listen on 8025 for web management interface, must be synced with `Email:Outgoing:ManagementPort`. It is required to
  check what has been sent to the mail server. This unit test uses management interface to check sent email.
- Enable for secure protocol with the same port. By default, it will try to use STARTTLS when available.

All [logs](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) can be seen in test runner output directly,
which is implemented by [Serilog.Sinks.NUnit](https://github.com/luk355/Serilog.Sinks.NUnit).

### Incoming mail test

This mail test requires IMAP & POP3 server to run properly. Suggested to
use [docker-mailserver](https://github.com/docker-mailserver/docker-mailserver). Before running mail server, we need to
generate some dependencies.

1. Security certificate
   The security certificate is required for secure connection (SSL/TLS/StartTLS) etc. Self signed certificate is enough.
   To generate self-signed certificate, use this
   command: `docker run -it --rm -h mail.moneyventory.lab -v mailconfig:/tmp/docker-mailserver mailserver/docker-mailserver generate-ssl-certificate`.
2. Create 1 dummy account
   We need at least 1 dummy account to make the mail server works correctly. We can do that by using following
   command: `docker run -it --rm -h mail.moneyventory.lab -v mailconfig:/tmp/docker-mailserver mailserver/docker-mailserver addmailuser admin@moneyventory.lab your_password`.
   Notice that we use the same docker volume and not creating container yet.

After that, we can create and run the container using the following command:

`docker run -d --name mailserver -h mail.moneyventory.lab --domainname moneyventory.lab -p 25:25 -p 143:143 -p 110:110 -p 465:465 -p 587:587 -p 993:993 -p 995:995 -e ENABLE_POP3=1 -e SSL_TYPE=self-signed -e PERMIT_DOCKER=connected-networks -v mailconfig:/tmp/docker-mailserver mailserver/docker-mailserver`.
Be sure to use the volume for *mailconfig*. Read more
detail [here](https://github.com/docker-mailserver/docker-mailserver#usage).

```
docker run -p 25:25 -p 80:80 -p 443:443 -p 110:110 -p 143:143 -p 465:465 -p 587:587 -p 993:993 -p 995:995 -e TZ=Asia/Jakarta -v maildata:/data --name mailserver --hostname mail -e HTTPS=OFF -d -t analogic/poste.io
```

Note that, the https must be disabled for web interface in dev environment because HSTS protocol is enabled and we don't
have valid certificate issued by public CA. Otherwise, we couldn't access all the web interface (admin, webmail, etc)

