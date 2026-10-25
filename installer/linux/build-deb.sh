#!/usr/bin/env bash
# Usage: build-deb.sh <version> <payload-dir>
#   version      e.g. 0.1.0 or 0.1.0-beta1 (mapped to 0.1.0~beta1 for dpkg)
#   payload-dir  folder containing the published linux-x64 'rw' binary
set -euo pipefail

version="${1:?usage: build-deb.sh <version> <payload-dir>}"
payload="${2:?usage: build-deb.sh <version> <payload-dir>}"

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
dist="$repo/dist"

deb_version="${version/-/\~}"
pkg="rw_${deb_version}_amd64"
stage="$dist/deb-staging/$pkg"

rm -rf "$stage"
install -D -m 0755 "$payload/rw" "$stage/usr/bin/rw"
install -D -m 0644 "$here/copyright" "$stage/usr/share/doc/rw/copyright"
mkdir -p "$stage/DEBIAN"

size_kb="$(du -sk "$stage/usr" | cut -f1)"
sed -e "s/@VERSION@/$deb_version/" -e "s/@SIZE@/$size_kb/" \
    "$here/control.template" > "$stage/DEBIAN/control"

# dpkg-deb rejects directories that are not 0755 (a restrictive umask can cause this)
find "$stage" -type d -exec chmod 0755 {} +

mkdir -p "$dist"
dpkg-deb --build --root-owner-group "$stage" "$dist/$pkg.deb"

if command -v lintian >/dev/null 2>&1; then
    lintian "$dist/$pkg.deb" || echo "lintian reported issues (warnings allowed for now)"
fi

echo "Built $dist/$pkg.deb"
