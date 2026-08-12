---
name: check-commit-message
description: Validate that a commit message (or a range of commits) follows the Conventional Commits spec. Use when the user asks to check, lint, or validate commit message format, or before pushing/creating a PR if they ask for a commit message check.
---

# Check Commit Message

Validate commit message(s) against the [Conventional Commits](https://www.conventionalcommits.org/) specification.

## What to check

A valid conventional commit header matches:

```
<type>[optional scope][optional !]: <description>
```

- `type` is one of: `feat`, `fix`, `build`, `chore`, `ci`, `docs`, `style`, `refactor`, `perf`, `test`, `revert`
- `scope` (optional) is a parenthesized noun, e.g. `(valve)`, `(pressure-sensor)`
- `!` immediately before the colon marks a breaking change
- `description` is a short, present-tense summary — non-empty, not capitalized-sentence-case required, no trailing period preferred
- Body and footer are optional and separated from the header by a blank line
- A breaking change may also be declared via a footer starting with `BREAKING CHANGE: `

Regex for the header line:

```
^(feat|fix|build|chore|ci|docs|style|refactor|perf|test|revert)(\([^)]+\))?!?: .+
```

## Steps

1. Determine what to validate:
   - If the user gave a specific commit message string, validate that string directly.
   - If the user gave a git ref/range (e.g. a SHA, `HEAD`, `main..HEAD`), run `git log --format=%B -n <N>` or `git log <range> --format=%s%x00%b` per commit to extract each message.
   - If nothing was specified, default to the current `HEAD` commit: `git log -1 --pretty=%B`.
2. For each commit message, check the header line against the regex above.
3. Report per commit:
   - ✅ Pass — show the header.
   - ❌ Fail — show the header and the specific reason (unknown type, missing colon/space, empty description, etc.), and suggest a corrected version.
4. If checking more than one commit, finish with a summary count (e.g. "3/4 passed").
5. Do not amend or rewrite any commits unless the user explicitly asks you to fix them.
