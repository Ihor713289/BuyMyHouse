To ensure that the mortgage offer emails are sent you need to follow the steps:
1. Copy example-local.settings.json to local.settings.json (edit anything if needed)
2. Setting up SMTP catcher (or edit the local.settings.json if already set up), for instance smtp4dev in docker compose:
```yaml
services:
  smtp4dev:
    image: rnwood/smtp4dev:latest
    ports:
      - "5000:80"
      - "2525:25"
      - "110:110"
    stdin_open: true
    tty: true
```