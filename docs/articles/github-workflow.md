# Git Workflow

This project uses Git for local source control with a remote repository hosted on GitHub.

Recommended branch naming:

```
main
feature/issue-<issue-number>
fix/issue-<issue-number>
docs/issue-<issue-number>
refactor/issue-<issue-number>
```
_example: feature/issue-42 will cause Issue #42 status to change to `In Progress` when branch is created._

Recommended commit style:

```
Add initial repository attributes #<issue-number>
Add application README scaffold #<issue-number>
Implement security header middleware #<issue-number>
Configure Serilog request logging #<issue-number>
Add EF Core SQLite provider #<issue-number>
```

## Required Secret

This workflow requires a classic GitHub personal access token stored as:

`PROJECT_TOKEN`

Required classic PAT scopes:

- `project`
- `repo` if the repository is private
- `public_repo` may be sufficient if the repository is public

_A fine-grained PAT may not work for user-owned GitHub Projects._
