#!/bin/zsh
set -eu
cd "$(dirname "$0")/Tools/VoiceBridge"
if ! command -v node >/dev/null 2>&1; then
  echo 'Node.js 22+ をインストールしてから再実行してください。'
  read -r; exit 1
fi
if [ ! -d node_modules/codex-component ]; then npm ci --allow-git=root; fi
if [ ! -x 'build/Room One Voice.app/Contents/MacOS/RoomOneSpeech' ] || [ Speech.swift -nt 'build/Room One Voice.app/Contents/MacOS/RoomOneSpeech' ]; then sh build-speech.sh; fi
exec node server.mjs
