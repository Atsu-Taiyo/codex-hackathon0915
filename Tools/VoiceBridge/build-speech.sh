#!/bin/sh
set -eu
cd "$(dirname "$0")"
app="build/Room One Voice.app/Contents"
mkdir -p "$app/MacOS"
cat > "$app/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?><!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd"><plist version="1.0"><dict>
<key>CFBundleIdentifier</key><string>com.hackathon.roomone.voice</string>
<key>CFBundleName</key><string>Room One Voice</string>
<key>CFBundleExecutable</key><string>RoomOneSpeech</string>
<key>CFBundlePackageType</key><string>APPL</string>
<key>LSUIElement</key><true/>
<key>NSMicrophoneUsageDescription</key><string>Use your voice to arrange the word cards.</string>
<key>NSSpeechRecognitionUsageDescription</key><string>Convert your spoken card instructions to text.</string>
</dict></plist>
PLIST
swiftc Speech.swift -o "$app/MacOS/RoomOneSpeech"
codesign --force --sign - "build/Room One Voice.app"
