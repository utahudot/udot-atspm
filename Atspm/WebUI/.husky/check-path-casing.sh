#!/usr/bin/env sh

set -eu

forbidden_root="$(printf 'AT%s' 'SPM')"
tracked_paths="$(git ls-files | grep -E "^${forbidden_root}/" || true)"
path_references="$(git grep --cached -n -I -E "${forbidden_root}[/\\\\]" -- . || true)"

if [ -n "$tracked_paths" ]; then
  echo "Error: tracked paths use the forbidden uppercase project-root casing:"
  echo "$tracked_paths"
fi

if [ -n "$path_references" ]; then
  echo "Error: tracked files reference the forbidden uppercase project-root casing:"
  echo "$path_references"
fi

if [ -n "$tracked_paths" ] || [ -n "$path_references" ]; then
  echo "Use 'Atspm/' for repository paths."
  exit 1
fi
