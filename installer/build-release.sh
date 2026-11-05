#!/usr/bin/env bash
# Builds every release file for one version into dist/<version>/:
#   rw-<version>-linux-x64        the Linux binary
#   rw-<version>-win-x64.exe      the Windows binary
#   rw_<deb-version>_amd64.deb    the Debian package
#   rw-<version>.msi              the Windows installer, built with msitools' wixl so it also runs on Linux
#   SHA256SUMS                    checksums of the four files above
# Usage: build-release.sh <version>
#   version  e.g. 0.2.0, v0.2.0 or 0.2.0-beta1 (a leading v is dropped)
# Needs the .NET SDK, dpkg-deb, wixl, and msitools (msibuild and msiinfo, used by add-path.sh).
set -euo pipefail

raw="${1:?usage: build-release.sh <version>}"
version="${raw#v}"
if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.]+)?$ ]]; then
    echo "version must look like 0.2.0 or 0.2.0-beta1 (got '$raw')" >&2
    exit 1
fi

# MSI versions must be numeric, so the prerelease suffix is dropped for the .msi only.
msi_version="${version%%-*}"
deb_version="${version/-/\~}"

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo="$(cd "$here/.." && pwd)"
out="$repo/dist/$version"
payload="$out/payload"

rm -rf "$out"
mkdir -p "$out"

export RW_VERSION="$version"
for rid in linux-x64 win-x64; do
    dotnet publish "$repo/src/Rw/Rw.csproj" -c Release -r "$rid" --self-contained true -o "$payload/$rid"
done

cp "$payload/linux-x64/rw" "$out/rw-$version-linux-x64"
cp "$payload/win-x64/rw.exe" "$out/rw-$version-win-x64.exe"

bash "$here/linux/build-deb.sh" "$version" "$payload/linux-x64" "$out"

wxs="$out/Product.wxs"
sed -e "s|@VERSION@|$msi_version|" -e "s|@PAYLOAD@|$payload/win-x64|" \
    "$here/windows/Product.wixl.wxs" > "$wxs"
wixl -v -o "$out/rw-$version.msi" "$wxs"
bash "$here/windows/add-path.sh" "$out/rw-$version.msi" RwExe

deb="rw_${deb_version}_amd64.deb"
rm -rf "$payload" "$wxs" "$repo/dist/deb-staging"

(cd "$out" && sha256sum "rw-$version-linux-x64" "rw-$version-win-x64.exe" "$deb" "rw-$version.msi" > SHA256SUMS)

echo "Release files for $version are in $out"
ls -l "$out"
