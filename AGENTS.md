# Repository Agent Instructions

## Purpose
This repository now hosts planning assets and reference implementations for the ServiceNow Active Directory account unlock automation powered by Microsoft Semantic Kernel (SK) multi-agent workflows.

## Contribution Guidelines
- Prefer creating new folders within `docs/` for design notes and `src/` for C# source.
- Maintain cross-platform compatibility. Scripts intended for Windows PowerShell should live in `scripts/` and clearly document prerequisites.
- Keep GitHub Actions workflow files in `.github/workflows/`.
- Use `dotnet` CLI conventions for any C# project/solution structure.
- Document required secrets and configuration in the relevant README files rather than hard-coding values.

## Testing
- Provide instructions for unit/integration tests in the README of each project.
- When adding automated tests, integrate them with `dotnet test`.

## Pull Requests
- Ensure any changes are described in `docs/` as needed so downstream agents (including Codex) can follow the architecture.
