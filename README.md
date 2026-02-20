# GitHub MCP Server

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

An [MCP (Model Context Protocol)](https://modelcontextprotocol.io/) server that exposes GitHub REST API operations as MCP tools. The GitHub App credentials are isolated inside the server container so LLM agents never handle them directly.

Designed for use with the [RockBot](https://github.com/MarimerLLC/rockbot) agent, but compatible with any MCP-capable client.

## Tools

| Tool | Description |
|------|-------------|
| `list_user_repos` | List repositories for the authenticated user |
| `list_org_repos` | List repositories for an organization |
| `get_repo` | Get details of a specific repository |
| `list_issues` | List issues for a repository |
| `get_issue` | Get a specific issue |
| `create_issue` | Create a new issue |
| `update_issue` | Update title, body, or state of an issue |
| `add_issue_comment` | Add a comment to an issue |
| `list_issue_comments` | List comments on an issue |
| `list_pull_requests` | List pull requests for a repository |
| `get_pull_request` | Get a specific pull request |
| `create_pull_request` | Create a new pull request |
| `merge_pull_request` | Merge a pull request |
| `list_pr_files` | List files changed in a pull request |
| `get_file_contents` | Get decoded text contents of a file |
| `list_directory` | List contents of a directory |
| `list_branches` | List branches in a repository |
| `get_branch` | Get details of a specific branch |

## Tech Stack

- .NET 10 ASP.NET Core (minimal web app)
- [`ModelContextProtocol.AspNetCore`](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore) — SSE/HTTP transport
- GitHub REST API via `HttpClient` — no extra SDK
- GitHub App authentication (JWT → installation access token, auto-refreshed)

## Authentication: GitHub App Setup

This server uses a **GitHub App** (not a personal access token) so it can access all repositories in an organization without being tied to any individual user account.

### 1. Create the GitHub App

1. Go to your organization's app creation page:
   `https://github.com/organizations/YOUR_ORG/settings/apps/new`
2. Fill in the basics (name, homepage URL — these can be anything)
3. Under **Permissions → Repository permissions**, grant:
   - **Contents**: Read
   - **Issues**: Read and write
   - **Metadata**: Read (required, auto-selected)
   - **Pull requests**: Read and write
4. Under **Where can this GitHub App be installed?** select **Only on this account**
5. Click **Create GitHub App**
6. Note the **App ID** shown at the top of the app's settings page

### 2. Generate a Private Key

On the app's settings page, scroll to **Private keys** and click **Generate a private key**. A `.pem` file will be downloaded — keep this safe, it cannot be retrieved again.

### 3. Install the App on Your Organization

1. In the app settings, click **Install App** in the left sidebar
2. Click **Install** next to your organization
3. Choose **All repositories** or select specific repos
4. After install, note the **Installation ID** from the URL:
   `github.com/organizations/YOUR_ORG/settings/installations/XXXXXXXXX`
   The numeric suffix is the Installation ID.

## Local Development

### Prerequisites

- .NET 10 SDK
- A GitHub App created and installed (see above)

### Configure User Secrets

```bash
cd src/GitHubMcpServer

dotnet user-secrets set "GitHub:AppId" "YOUR_APP_ID"
dotnet user-secrets set "GitHub:InstallationId" "YOUR_INSTALLATION_ID"

# For the PEM key, edit the secrets.json file directly (multi-line value):
# Path: ~/.microsoft/usersecrets/8f3a2b1c-4d5e-6f7a-8b9c-0d1e2f3a4b5c/secrets.json
# Add: "GitHub:PrivateKeyPem": "-----BEGIN RSA PRIVATE KEY-----\nMII...\n-----END RSA PRIVATE KEY-----\n"
```

Or edit `secrets.json` directly:

```json
{
  "GitHub:AppId": "123456",
  "GitHub:InstallationId": "78901234",
  "GitHub:PrivateKeyPem": "-----BEGIN RSA PRIVATE KEY-----\nMIIE...\n-----END RSA PRIVATE KEY-----\n"
}
```

### Run

```bash
cd src/GitHubMcpServer
dotnet run
```

The server starts on `http://localhost:5000`. Connect an MCP client to `/sse`.

## Docker

### Build

```bash
docker build -t rockylhotka/mcp-github:latest .
```

### Run

```bash
docker run -p 8080:8080 \
  -e GitHub__AppId=YOUR_APP_ID \
  -e GitHub__InstallationId=YOUR_INSTALLATION_ID \
  -e "GitHub__PrivateKeyPem=-----BEGIN RSA PRIVATE KEY-----
MII...
-----END RSA PRIVATE KEY-----
" \
  rockylhotka/mcp-github:latest
```

## Kubernetes Deployment

### Prerequisites

- `rockbot` namespace exists: `kubectl create namespace rockbot`
- GitHub App created and installed (see above)

### Create the Secret

```bash
kubectl create secret generic github-mcp-secrets \
  --namespace rockbot \
  --from-literal=GitHub__AppId=YOUR_APP_ID \
  --from-literal=GitHub__InstallationId=YOUR_INSTALLATION_ID \
  --from-file=GitHub__PrivateKeyPem=./your-app.YYYY-MM-DD.private-key.pem
```

> **Note:** Using `--from-file` for the PEM key preserves newlines correctly. Do not use `--from-literal` for the PEM value.

### Deploy

```bash
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
```

### Verify

```bash
kubectl rollout status deployment/mcp-github -n rockbot
kubectl logs -l app=mcp-github -n rockbot
```

The service is accessible within the cluster at:
`http://mcp-github.rockbot.svc.cluster.local/sse`

### Update Image

```bash
docker build -t rockylhotka/mcp-github:latest .
docker push rockylhotka/mcp-github:latest
kubectl rollout restart deployment/mcp-github -n rockbot
```

## How Authentication Works

1. On first request (and every ~55 minutes), the server generates a short-lived **JWT** (10-minute expiry) signed with the App's RSA private key
2. The JWT is exchanged with GitHub's API for an **installation access token** (1-hour expiry)
3. The installation token is cached and injected automatically into every GitHub API request via a `DelegatingHandler`
4. The private key never leaves the container

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes
4. Push and open a Pull Request

Please read our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing.

## License

This project is licensed under the [MIT License](LICENSE).

## Acknowledgments

- [Model Context Protocol](https://modelcontextprotocol.io/) for the MCP specification
- [RockBot](https://github.com/MarimerLLC/rockbot) as the primary consuming agent
