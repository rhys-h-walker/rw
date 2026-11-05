#!/usr/bin/env bash
# Publishes the files that build-release.sh made as a GitLab release for v<version>. The publish job in
# .gitlab-ci.yml runs this. The release tags CI_COMMIT_SHA, the commit the pipeline ran on, so the branch you
# select when starting the pipeline is the branch that gets tagged. Needs curl and jq.
# Usage: publish-gitlab-release.sh <version>
set -euo pipefail

raw="${1:?usage: publish-gitlab-release.sh <version>}"
version="${raw#v}"
tag="v$version"
deb_version="${version/-/\~}"

: "${CI_API_V4_URL:?CI_API_V4_URL is not set; this must run in GitLab CI}"
: "${CI_PROJECT_ID:?CI_PROJECT_ID is not set; this must run in GitLab CI}"
: "${CI_JOB_TOKEN:?CI_JOB_TOKEN is not set; this must run in GitLab CI}"
: "${CI_COMMIT_SHA:?CI_COMMIT_SHA is not set; this must run in GitLab CI}"

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo="$(cd "$here/.." && pwd)"
out="$repo/dist/$version"
if [ ! -d "$out" ]; then
    echo "$out is missing; run build-release.sh first" >&2
    exit 1
fi

api="$CI_API_V4_URL/projects/$CI_PROJECT_ID"
pkg="$api/packages/generic/rw/$version"
files=(
    "rw-$version-linux-x64"
    "rw-$version-win-x64.exe"
    "rw_${deb_version}_amd64.deb"
    "rw-$version.msi"
    "SHA256SUMS"
)

# A tag that already exists would make the release point at an old commit, so stop instead.
code="$(curl -s -o /dev/null -w '%{http_code}' -H "JOB-TOKEN: $CI_JOB_TOKEN" "$api/repository/tags/$tag")"
if [ "$code" != "404" ]; then
    echo "tag $tag already exists or cannot be checked (HTTP $code); choose a new version" >&2
    exit 1
fi

links='[]'
for f in "${files[@]}"; do
    echo "Uploading $f"
    curl --fail-with-body -sS -H "JOB-TOKEN: $CI_JOB_TOKEN" --upload-file "$out/$f" "$pkg/$f" >/dev/null
    links="$(jq --arg name "$f" --arg url "$pkg/$f" '. + [{name: $name, url: $url, link_type: "package"}]' <<<"$links")"
done

description="$(printf 'SHA256 checksums:\n\n```\n%s\n```\n' "$(cat "$out/SHA256SUMS")")"
body="$(jq -n \
    --arg name "rw $tag" \
    --arg tag "$tag" \
    --arg ref "$CI_COMMIT_SHA" \
    --arg description "$description" \
    --argjson links "$links" \
    '{name: $name, tag_name: $tag, ref: $ref, description: $description, assets: {links: $links}}')"

echo "Creating release $tag at $CI_COMMIT_SHA"
curl --fail-with-body -sS -H "JOB-TOKEN: $CI_JOB_TOKEN" -H "Content-Type: application/json" \
    -X POST -d "$body" "$api/releases" | jq -r '._links.self'
