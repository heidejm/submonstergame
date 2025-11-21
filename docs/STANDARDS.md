# Project Development Standards

**Version**: 1.0
**Last Updated**: 2025-11-21
**Status**: Active

## Purpose

This document defines the architectural, security, and code quality standards for this project. These are **mandatory requirements**, not suggestions. All code, whether written by humans or AI assistants, must adhere to these standards.

---

## Table of Contents

1. [Core Principles](#core-principles)
2. [Architecture & Design](#architecture--design)
3. [Code Quality](#code-quality)
4. [Security](#security)
5. [Performance & Scalability](#performance--scalability)
6. [Testing](#testing)
7. [Documentation](#documentation)
8. [Version Control](#version-control)
9. [Development Workflow](#development-workflow)
10. [Enforcement](#enforcement)

---

## Core Principles

### 1. No Vibe Coding
- Every architectural decision must have a documented rationale
- No "we'll refactor it later" - build it right the first time
- Design before implementation for any non-trivial feature

### 2. Scalability First
- Design systems to handle 10x growth from day one
- Modular architecture that allows feature addition without core refactoring
- Plan for distributed systems even if starting single-server

### 3. Security by Default
- Never trust user input
- Defense in depth - multiple layers of security
- Fail securely - errors should not expose sensitive information

### 4. Maintainability Over Cleverness
- Code is read 10x more than written - optimize for readability
- Prefer explicit over implicit
- Standard patterns over clever solutions

---

## Architecture & Design

### Separation of Concerns

**Rule**: Each module, class, or function should have a single, well-defined responsibility.

**Application**:
- **Game Logic**: Pure business logic, no rendering or I/O
- **Rendering**: Display logic only, no game state modification
- **UI Layer**: User interaction handling, delegates to game logic
- **Data Layer**: Persistence and retrieval, no business logic
- **Network Layer**: Communication protocol, no game logic

### Design Patterns

**Required**:
- Use established design patterns (Factory, Observer, Command, State, etc.)
- Document which pattern is used and why
- Consistent application of patterns throughout the codebase

**Prohibited**:
- Singleton pattern (except for truly global services with clear justification)
- God objects (classes that do too much)
- Circular dependencies

### SOLID Principles

1. **Single Responsibility**: One class, one reason to change
2. **Open/Closed**: Open for extension, closed for modification
3. **Liskov Substitution**: Subtypes must be substitutable for base types
4. **Interface Segregation**: Many specific interfaces over one general interface
5. **Dependency Inversion**: Depend on abstractions, not concretions

### Component-Based Architecture

- Favor composition over inheritance
- Small, reusable components
- Clear component interfaces
- Minimal inter-component coupling

---

## Code Quality

### Naming Conventions

- **Variables/Functions**: camelCase, descriptive (e.g., `playerHealth`, `calculateDamage()`)
- **Classes/Types**: PascalCase (e.g., `PlayerController`, `GameState`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_PLAYERS`, `DEFAULT_TIMEOUT`)
- **Private members**: Prefix with underscore if language-appropriate (e.g., `_internalState`)

**Rule**: Names should be self-documenting. If you need a comment to explain what it is, the name is wrong.

### No Magic Values

**Prohibited**:
```javascript
// BAD
if (player.level > 10) {
    damage *= 1.5;
}
```

**Required**:
```javascript
// GOOD
const VETERAN_LEVEL_THRESHOLD = 10;
const VETERAN_DAMAGE_MULTIPLIER = 1.5;

if (player.level > VETERAN_LEVEL_THRESHOLD) {
    damage *= VETERAN_DAMAGE_MULTIPLIER;
}
```

### Error Handling

- All errors must be caught and handled appropriately
- Never use empty catch blocks
- Log errors with context (what operation failed, with what input)
- Fail fast on unrecoverable errors
- Provide user-friendly error messages (never expose stack traces to users)

### Code Formatting

- Use automated formatters (Prettier, Black, rustfmt, etc.)
- Consistent indentation (spaces vs tabs decided per language)
- Maximum line length: 100 characters (configurable per language)
- Meaningful whitespace to improve readability

### Comments

**When to comment**:
- **Why**: Explain non-obvious decisions and rationale
- **Warnings**: Document gotchas, limitations, or surprising behavior
- **TODOs**: With issue tracking numbers and dates

**When NOT to comment**:
- **What**: Code should be self-documenting
- **How**: Refactor into clearer code instead

---

## Security

### Input Validation

**Rule**: Validate ALL input at system boundaries.

- Client inputs (user actions, form data)
- Network messages (API calls, socket data)
- File inputs (configs, saves, uploaded content)
- Database results (sanitize before display)

**Validation checklist**:
- Type checking
- Range validation
- Format validation
- Sanitization (SQL injection, XSS prevention)
- Authorization (user allowed to perform this action?)

### Authentication & Authorization

- Never store passwords in plain text (use bcrypt, argon2, etc.)
- Implement principle of least privilege
- Server is authoritative for all game state
- Client-side checks are UX only, never security

### Data Protection

- Sensitive data encrypted at rest and in transit
- API keys, credentials, secrets in environment variables (NEVER in code)
- `.env` files in `.gitignore`
- Separate secrets for dev, staging, production

### Common Vulnerabilities

**Must prevent**:
- SQL Injection
- Cross-Site Scripting (XSS)
- Cross-Site Request Forgery (CSRF)
- Command Injection
- Path Traversal
- Buffer Overflows
- Race Conditions
- Insecure Deserialization

**Process**: Regular security audits of dependencies and code.

---

## Performance & Scalability

### Performance Targets

**To be defined based on game genre and platform, but examples**:
- Target frame rate: 60 FPS on minimum spec hardware
- Maximum load time: 3 seconds for game start, 1 second for scene transitions
- Network latency tolerance: Playable up to 150ms ping
- Memory budget: Define per platform

### Optimization Strategy

1. **Measure First**: Profile before optimizing
2. **Focus on Hotspots**: Optimize the 20% that causes 80% of issues
3. **Big-O Matters**: Choose correct algorithms and data structures
4. **Avoid Premature Optimization**: But design for performance

### Resource Management

- Object pooling for frequently created/destroyed objects
- Lazy loading for assets
- Asset streaming for large worlds
- Memory leak prevention (proper cleanup, no circular references)

### Scalability Patterns

- Stateless services where possible
- Horizontal scaling capability
- Caching strategies (with invalidation)
- Database optimization (indexing, query optimization, connection pooling)
- Rate limiting and throttling

---

## Testing

### Test Coverage

**Required**:
- Unit tests for all business logic
- Integration tests for system interactions
- End-to-end tests for critical user paths

**Target**: 80%+ code coverage for core systems

### Test Principles

- Tests should be fast, independent, and repeatable
- One assertion per test (or closely related assertions)
- Clear test names describing what is tested
- Arrange-Act-Assert pattern

### Testing Checklist

- [ ] All edge cases covered
- [ ] Error conditions tested
- [ ] Performance regression tests for critical paths
- [ ] Tests pass consistently (no flaky tests)

---

## Documentation

### Code Documentation

**Required for all**:
- Public APIs: Full documentation with examples
- Complex algorithms: Explanation of approach
- Configuration options: Description, valid values, defaults

### Project Documentation

**Required files**:
- `README.md`: Project overview, setup instructions, basic usage
- `ARCHITECTURE.md`: System design, component relationships
- `CONTRIBUTING.md`: How to contribute (when applicable)
- `CHANGELOG.md`: Version history and notable changes

### Inline Documentation

- Document assumptions and preconditions
- Explain complex business logic
- Reference external resources (algorithms, RFCs, etc.)

---

## Version Control

### Commit Standards

**Format**:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Example**:
```
feat(combat): Add critical hit calculation

Implements critical hit system with configurable crit chance
and damage multiplier. Base crit chance is 5%, modified by
player stats and equipment.

Closes #123
```

### Branch Strategy

- `main`: Production-ready code only
- `develop`: Integration branch for features
- `feature/<name>`: Feature development
- `bugfix/<name>`: Bug fixes
- `hotfix/<name>`: Emergency production fixes

### Pull Request Requirements

- [ ] All tests pass
- [ ] Code reviewed by at least one other developer (when team exists)
- [ ] Documentation updated
- [ ] No merge conflicts
- [ ] Follows all standards in this document

---

## Development Workflow

### Before Starting Work

1. Read and understand relevant standards from this document
2. Design the solution (write it down for complex features)
3. Identify affected systems and potential impacts
4. Create or update issue tracker tickets

### During Development

1. Write tests first (TDD) or alongside code
2. Commit frequently with meaningful messages
3. Run linters and formatters before committing
4. Keep changes focused (one feature/fix per branch)

### Before Committing

**Checklist**:
- [ ] Code follows all standards
- [ ] Tests written and passing
- [ ] No commented-out code
- [ ] No debugging console logs
- [ ] No TODOs without issue numbers
- [ ] Documentation updated
- [ ] Linter passes with zero warnings

### Code Review Checklist

**Reviewer must verify**:
- [ ] Standards compliance
- [ ] Security considerations addressed
- [ ] Performance implications considered
- [ ] Tests adequate and passing
- [ ] Documentation clear and complete
- [ ] No unnecessary complexity

---

## Enforcement

### Automated Tools

**Required**:
- Linters (ESLint, Pylint, Clippy, etc.)
- Formatters (Prettier, Black, rustfmt, etc.)
- Type checkers (TypeScript, mypy, etc.)
- Git hooks (pre-commit: lint & format; pre-push: tests)

### CI/CD Pipeline

**Checks on every commit**:
- Lint compliance
- Test suite execution
- Build success
- Code coverage metrics
- Security vulnerability scanning

### Human Review

- All code changes reviewed against this document
- Architecture decisions discussed and documented
- Regular code quality audits

### Continuous Improvement

- Standards updated as project evolves
- Retrospectives to identify process improvements
- Lessons learned documented and applied

---

## Exceptions

Exceptions to these standards must be:
1. Documented with clear justification
2. Approved by project lead
3. Marked with `// STANDARDS-EXCEPTION: <reason>` comments
4. Tracked for future refactoring

---

## Version History

- **v1.0** (2025-11-21): Initial standards document created

---

## Questions or Clarifications

If any standard is unclear or seems to conflict with project needs, discuss before implementation. These standards are living documents and can be updated with proper justification and consensus.
