# Contributing to User Document API

Thank you for your interest in contributing! 🎉

## 🚀 Quick Start
```bash
# Clone repository
git clone https://github.com/your-username/user-document-api.git
cd user-document-api

# Setup development environment
docker-compose up -d

# Run tests
./scripts/run-tests.sh

## 📋 Development Workflow

### 1. Create a Branch

bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/bug-description

### 2. Make Changes

- Follow C# coding conventions
- Add XML documentation to public APIs
- Write unit tests for new features
- Update integration tests if needed

### 3. Test Your Changes

bash
# Run all tests
./scripts/run-tests.sh

# Run specific test
dotnet test tests/UserDocumentAPI.Tests/ --filter "YourTestName"

### 4. Commit Your Changes

Follow [Conventional Commits](https://www.conventionalcommits.org/):

bash
git commit -m "feat: add document validation"
git commit -m "fix: resolve retry policy issue"
git commit -m "docs: update API documentation"

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Build process or tools

### 5. Push and Create PR

bash
git push origin feature/your-feature-name

Then create a Pull Request on GitHub.

## 🏗️ Project Structure


UserDocumentAPI/
├── src/UserDocumentAPI/      # Main application
├── tests/                     # Test projects
├── scripts/                   # Utility scripts
└── docs/                      # Additional documentation

## ✅ Code Quality Guidelines

### C# Style Guide

csharp
// ✅ Good
public class UserService
{
private readonly ILogger<UserService> _logger;

public async Task<User> CreateUserAsync(CreateUserRequest request)
{
// Implementation
}
}

// ❌ Avoid
public class userservice
{
public User createUser(CreateUserRequest request)
{
// Implementation
}
}

### Testing Guidelines

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test API endpoints end-to-end
- **Coverage**: Aim for >80% code coverage
- **Naming**: `MethodName_Scenario_ExpectedBehavior`

csharp
[Fact]
public async Task CreateUser_WithValidData_ReturnsCreatedUser()
{
// Arrange
var request = new CreateUserRequest { /* ... */ };

// Act
var result = await _service.CreateUserAsync(request);

// Assert
result.Should().NotBeNull();
}

## 🐛 Bug Reports

Use GitHub Issues with the following template:

markdown
**Description:**
A clear description of the bug

**Steps to Reproduce:**
1. Step 1
2. Step 2
3. Step 3

**Expected Behavior:**
What should happen

**Actual Behavior:**
What actually happens

**Environment:**
- OS: [e.g., Windows 11]
- .NET Version: [e.g., 8.0]
- Docker Version: [e.g., 24.0.5]

## 💡 Feature Requests

Use GitHub Issues with the `enhancement` label:

markdown
**Feature Description:**
Clear description of the feature

**Use Case:**
Why this feature is needed

**Proposed Solution:**
How it could be implemented

**Alternatives:**
Other approaches considered

## 📝 Documentation

- Update `README.md` for user-facing changes
- Update `CHANGELOG.md` for all changes
- Add XML comments to new public APIs
- Update Swagger documentation if needed

## 🔍 Code Review Process

All submissions require review. We use GitHub PRs for this purpose:

1. **Automated Checks**: Must pass CI/CD
2. **Code Review**: At least one approval required
3. **Testing**: All tests must pass
4. **Documentation**: Must be updated if needed

## 📞 Questions?

- Open an issue with the `question` label
- Contact maintainers: [your-email@example.com](mailto:your-email@example.com)

## 📜 License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing! 🙏


---

## ✅ دستورات نهایی

### 1. ساخت فایل‌ها و دادن مجوز اجرا:

```bash
# ساخت فولدر scripts
mkdir -p scripts

# ساخت فایل‌های bash
touch scripts/run-tests.sh
touch scripts/cleanup-docker.sh
touch scripts/deploy.sh

# دادن مجوز اجرا (مهم!)
chmod +x scripts/*.sh

# ساخت فایل‌های مستندات
touch CONTRIBUTING.md
touch CHANGELOG.md

# ساخت docker-compose.override.yml
touch docker-compose.override.yml
