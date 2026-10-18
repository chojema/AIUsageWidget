# Security and private data

[한국어](SECURITY.ko.md)

## Reporting a vulnerability

Please report security problems privately through GitHub's **Report a vulnerability** button on the repository's Security tab instead of opening a public issue. Do not include real credentials, tokens or account data in a report.

## How the app handles data

- The app does not read, store or send credentials. Codex limits come from the official `codex app-server`, which uses the CLI's own sign-in.
- The Claude Code bridge stores only the 5-hour and weekly limit values and when they were seen.
- Usage values are not written to logs or error messages.

## What this repository never contains

The repository holds the app source, reproducible build settings, tests and public documents only. It must not contain:

- API keys, sign-in tokens, cookies, credential files or signing keys
- Real usage responses, account identifiers, email addresses or local user paths
- Internal notes and local development environment settings
- Screenshots, diagnostic logs or network captures that include account information
- Installed dependencies, temporary files or build output

Examples and tests use fictional data only. `.env.example` and `.env.template` must not contain real secrets.

## Checks before publishing

`.gitignore` lists files that stay out of the repository. It does not remove files that are already tracked, force-added, or present in past commits, so the publication candidates and Git history are reviewed before every upload.

With Gitleaks installed, run these before pushing. The first command lists the candidate files; the second scans the commit history.

```text
git ls-files --cached --others --exclude-standard
gitleaks git . --log-opts="--all" --redact --no-banner --ignore-gitleaks-allow
```

Untracked files in that list must be scanned separately; a history scan does not cover them. A passing scan does not prove that no sensitive data exists.

The GitHub Actions workflow checks tracked files against `.gitignore` and scans the fetched history with redacted output. It runs after upload, so it does not replace the checks above. Commit author names, email addresses and messages are part of the public record and use public identities only.

After the remote repository exists, secret scanning, push protection and required status checks are enabled and verified separately in the repository settings.

## If something is exposed

Do not add issues or screenshots that contain the secret. Revoke or rotate the exposed credential, check how far it reaches in current files and history, and then decide on further steps. History rewrites and force pushes are never done without a separate decision.

## References

- [Gitleaks](https://github.com/gitleaks/gitleaks)
- [Gitleaks release used](https://github.com/gitleaks/gitleaks/releases/tag/v8.30.1)
