# Contributing to Pingdom Exporter

Thank you for your interest in contributing to the Pingdom Exporter! This document provides guidelines and information for contributors.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/pingdomexport.git
   cd pingdomexport
   ```
3. **Create a branch** for your feature or fix:
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-fix-name
   ```

## Development Setup

### Prerequisites
- .NET 8.0 SDK or later
- Git
- Your favorite code editor (VS Code, Visual Studio, Rider, etc.)

### Local Development
```bash
# Navigate to the project directory
cd PingdomExporter

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application (requires valid API token)
dotnet run

# Run with sample configuration
cp appsettings.sample.json appsettings.json
# Edit appsettings.json with your API token
dotnet run
```

## Commit Convention

This project follows [Conventional Commits](https://conventionalcommits.org/) specification. Please format your commit messages accordingly:

### Format
```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Types
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Changes that do not affect the meaning of the code
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `perf`: A code change that improves performance
- `test`: Adding missing tests or correcting existing tests
- `build`: Changes that affect the build system or external dependencies
- `ci`: Changes to CI configuration files and scripts
- `chore`: Other changes that don't modify src or test files

### Examples
```bash
feat: add support for custom API endpoints
fix: handle network timeout errors gracefully
docs: update installation instructions
style: fix code formatting in ApiService
refactor: extract common HTTP client logic
test: add unit tests for export service
ci: add automated security scanning
```

### Breaking Changes
For breaking changes, add `BREAKING CHANGE:` in the footer or add `!` after the type:
```bash
feat!: remove deprecated configuration options

BREAKING CHANGE: The old configuration format is no longer supported
```

## Code Guidelines

### .NET Conventions
- Follow standard C# naming conventions (PascalCase for classes, camelCase for variables)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Use `async/await` for asynchronous operations
- Handle exceptions appropriately

### Project Structure
```
PingdomExporter/
├── Models/          # Data models and DTOs
├── Services/        # Business logic and API services
├── Program.cs       # Application entry point
└── appsettings.json # Configuration
```

### Code Style
- Use 4 spaces for indentation
- Place opening braces on new lines
- Use `var` when type is obvious from context
- Keep methods focused and single-purpose
- Add comments for complex business logic

### Error Handling
- Use appropriate exception types
- Log errors with sufficient context
- Provide user-friendly error messages
- Don't swallow exceptions silently

## Testing

Currently, the project focuses on integration testing through the CI/CD pipeline. If you're adding new features:

1. Ensure your code builds successfully
2. Test manually with valid Pingdom API credentials
3. Verify cross-platform compatibility
4. Consider edge cases and error scenarios

Future versions will include unit tests. Contributions for test infrastructure are welcome!

## Documentation

- Update README.md if you're changing functionality
- Add or update code comments for complex logic
- Update PROJECT_STRUCTURE.md if you're changing the project organization
- Include examples in your documentation

## Pull Request Process

1. **Update your fork** with the latest changes from main:
   ```bash
   git checkout main
   git pull upstream main
   git checkout your-feature-branch
   git rebase main
   ```

2. **Ensure your code follows the guidelines** above

3. **Create a pull request** with:
   - Clear title following conventional commit format
   - Detailed description of changes
   - Reference to any related issues
   - Screenshots or examples if applicable

4. **Respond to feedback** and make requested changes

5. **Squash commits** if requested to maintain clean history

## Release Process

Releases are automated through GitHub Actions:

1. **Merge to main** triggers the release workflow
2. **Semantic versioning** determines the next version based on commit messages
3. **Cross-platform builds** are created automatically
4. **GitHub release** is published with downloadable assets
5. **CHANGELOG.md** is updated automatically

You don't need to manually manage versions or releases.

## Issue Reporting

When reporting issues:

1. **Search existing issues** to avoid duplicates
2. **Use issue templates** if available
3. **Provide clear reproduction steps**
4. **Include relevant configuration** (without API tokens)
5. **Specify your environment** (OS, .NET version, etc.)

## Feature Requests

For new features:

1. **Check existing issues** and discussions
2. **Describe the use case** and problem you're solving
3. **Provide examples** of how the feature would work
4. **Consider backwards compatibility**

## Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- Help others learn and improve
- Follow GitHub's community guidelines

## Getting Help

- **Issues**: For bugs and feature requests
- **Discussions**: For questions and general discussion
- **Documentation**: Check README.md and project documentation
- **API Documentation**: Refer to [Pingdom API docs](https://docs.pingdom.com/api/)

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing! 🎉
