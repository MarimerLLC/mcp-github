# GitHub MCP Server

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

This is an MCP (Model Context Protocol) server designed to support the use of the `gh` CLI tool from LLM agents.

The goal is to have the user's GitHub credentials stored as a secret in this MCP server's container so the calling agent doesn't have direct access to that key.

This is particularly useful when used with the [RockBot](https://github.com/MarimerLLC/rockbot) agent where the goal is to have the agent be unaware of any secrets or keys.

## Features

- Exposes GitHub CLI (`gh`) operations as MCP tools
- Keeps GitHub credentials isolated inside the server container
- Compatible with any MCP-capable LLM agent

## Requirements

- [GitHub CLI (`gh`)](https://cli.github.com/) installed and authenticated in the server environment
- Docker (for containerized deployment)

## Usage

Configure your MCP client (e.g., RockBot or another agent) to connect to this server. The server will proxy GitHub CLI commands on behalf of the agent without exposing credentials.

Refer to your MCP client's documentation for how to register an MCP server.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes (`git commit -m 'Add my feature'`)
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a Pull Request

Please read our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing.

## Code of Conduct

This project adheres to the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this standard.

## License

This project is licensed under the [MIT License](LICENSE).

## Acknowledgments

- [Model Context Protocol](https://modelcontextprotocol.io/) for the MCP specification
- [GitHub CLI](https://cli.github.com/) for the underlying GitHub tooling
- [RockBot](https://github.com/MarimerLLC/rockbot) as the primary consuming agent