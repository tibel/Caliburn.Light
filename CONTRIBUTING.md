# Contributing to Caliburn.Light

First off, thank you for considering contributing to Caliburn.Light! It's people like you that make Caliburn.Light such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When you create a bug report, please include as many details as possible:

- **Use a clear and descriptive title** for the issue to identify the problem.
- **Describe the exact steps to reproduce the problem** in as much detail as possible.
- **Provide specific examples** to demonstrate the steps.
- **Describe the behavior you observed** after following the steps and point out what exactly is the problem.
- **Explain which behavior you expected** to see instead and why.
- **Include the .NET version** and target platform (WPF, WinUI, or Avalonia).
- **Include the Caliburn.Light version** you are using.

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

- **Use a clear and descriptive title** for the issue.
- **Provide a step-by-step description** of the suggested enhancement.
- **Provide specific examples** to demonstrate the steps or provide code snippets.
- **Describe the current behavior** and **explain which behavior you expected** to see instead.
- **Explain why this enhancement would be useful** to most Caliburn.Light users.

### Pull Requests

1. **Fork the repository** and create your branch from `master`.
2. **Follow the coding style** used throughout the project.
3. **Add tests** if you've added code that should be tested.
4. **Ensure the test suite passes**.
5. **Update documentation** if you've changed APIs or added new features.
6. **Write a clear commit message** describing your changes.

## Development Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- Visual Studio 2022 or VS Code with C# extension
- For WinUI development: Windows App SDK
- For Avalonia development: Avalonia templates

### Building the Project

```bash
# Clone your fork
git clone https://github.com/your-username/Caliburn.Light.git
cd Caliburn.Light

# Build all projects
dotnet build

# Run tests
dotnet test
```

### Project Structure

- `src/` - Main library source code
  - `Caliburn.Light.Core/` - Core functionality
  - `Caliburn.Light.WPF/` - WPF-specific implementation
  - `Caliburn.Light.WinUI/` - WinUI-specific implementation
  - `Caliburn.Light.Avalonia/` - Avalonia-specific implementation
- `samples/` - Sample applications demonstrating usage
- `docs/` - Documentation

## Coding Guidelines

- Follow the existing code style and conventions.
- Use meaningful variable and method names.
- Write XML documentation comments for public APIs.
- Keep methods focused and small.
- Prefer composition over inheritance.
- Write unit tests for new functionality.

## Commit Messages

- Use the present tense ("Add feature" not "Added feature").
- Use the imperative mood ("Move cursor to..." not "Moves cursor to...").
- Limit the first line to 72 characters or less.
- Reference issues and pull requests liberally after the first line.

## Questions?

Feel free to open an issue with your question or reach out to the maintainers.

Thank you for contributing! 🎉
