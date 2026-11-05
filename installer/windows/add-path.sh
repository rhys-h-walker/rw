#!/usr/bin/env bash
# Adds the install folder to the user's PATH in an MSI that wixl built. wixl does not support the Environment element,
# so this writes the Environment row and the two standard actions that apply it straight into the database, using
# msitools. Those actions must be in InstallExecuteSequence or Windows Installer never runs them.
# Usage: add-path.sh <msi> <component-id>
set -euo pipefail

msi="${1:?usage: add-path.sh <msi> <component-id>}"
component="${2:?usage: add-path.sh <msi> <component-id>}"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

# Environment table: set PATH on install, remove it on uninstall. [~] appends the folder to the existing PATH.
printf 'Environment\tName\tValue\tComponent_\ns72\ts255\tL255\ts72\nEnvironment\tEnvironment\nRwPath\t=-PATH\t[~];[INSTALLFOLDER]\t%s\n' \
    "$component" > "$work/Environment.idt"
msibuild "$msi" -i "$work/Environment.idt"

# InstallExecuteSequence already holds wixl's rows, and msibuild -i replaces a whole table, so export it, append the
# two actions unless they are already there, and import it back.
msiinfo export "$msi" InstallExecuteSequence > "$work/InstallExecuteSequence.idt"
for action in "WriteEnvironmentStrings 4550" "RemoveEnvironmentStrings 650"; do
    name="${action% *}"
    sequence="${action#* }"
    if ! grep -q "^$name	" "$work/InstallExecuteSequence.idt"; then
        printf '%s\t\t%s\n' "$name" "$sequence" >> "$work/InstallExecuteSequence.idt"
    fi
done
msibuild "$msi" -i "$work/InstallExecuteSequence.idt"

echo "Added the PATH entry for $component to $msi"
msiinfo export "$msi" Environment
