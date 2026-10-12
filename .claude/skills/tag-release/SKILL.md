---
name: tag-release
description: Create a SemVer git tag for a new release and push it
user_invocable: true
---

Run the tag-release script and push the tag:

1. Ask the user which part of the version to bump: major, minor, or patch
2. Ask the user for the stability level: alpha, beta, rc, or stable (default)
3. Run `bash scripts/tag-release.sh <bump> [alpha|beta|rc]` to create the tag (omit the stability argument for stable)
4. Parse the tag name from the output
5. Ask the user for confirmation before pushing
6. Push the tag with `git push origin <tag>`

The version is bumped from the latest stable tag, and prereleases of the same bump are numbered. Releases are heading for 1.0.0 (`v1.0.0-beta.1` exists, no stable release yet):
- `major beta`: `v1.0.0-beta.2`, then `v1.0.0-beta.3` on the next run
- `major rc`: `v1.0.0-rc.1`
- `major`: `v1.0.0`

After `v1.0.0`, use `minor` for new features (`v1.1.0`) and `patch` for fixes (`v1.0.1`).

Tags with a prerelease suffix are published as GitHub prereleases and are not marked as the latest release. The earlier date-based tags (`v2026.x.y`) are ignored when computing the next version.
