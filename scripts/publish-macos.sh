#!/bin/sh
# Builds AIUsageWidget.app for macOS and a zip for release. Run on a Mac with the .NET 10 SDK.
# Usage: sh scripts/publish-macos.sh [arm64|x64]
set -eu

ROOT=$(cd "$(dirname "$0")/.." && pwd)
PROJECT="$ROOT/src/AIUsageWidget/AIUsageWidget.csproj"
VERSION=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$PROJECT" | head -n 1)

ARCH=${1:-$(uname -m)}
case "$ARCH" in
  arm64) RID=osx-arm64 ;;
  x64|x86_64) RID=osx-x64 ;;
  *) echo "Unknown architecture: $ARCH" >&2; exit 1 ;;
esac

OUT="$ROOT/dist/$RID"
APP="$OUT/AIUsageWidget.app"
rm -rf "$OUT"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"

dotnet publish "$PROJECT" -c Release -r "$RID" --self-contained true -o "$APP/Contents/MacOS"

# App icon from the PNG asset.
ICONSET="$OUT/app.iconset"
mkdir -p "$ICONSET"
for size in 16 32 128 256; do
  sips -z $size $size "$ROOT/src/AIUsageWidget/Assets/app.png" --out "$ICONSET/icon_${size}x${size}.png" >/dev/null
  double=$((size * 2))
  sips -z $double $double "$ROOT/src/AIUsageWidget/Assets/app.png" --out "$ICONSET/icon_${size}x${size}@2x.png" >/dev/null
done
iconutil -c icns "$ICONSET" -o "$APP/Contents/Resources/AIUsageWidget.icns"
rm -rf "$ICONSET"

cat > "$APP/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key><string>AIUsageWidget</string>
  <key>CFBundleDisplayName</key><string>AIUsageWidget</string>
  <key>CFBundleIdentifier</key><string>io.github.chojema.aiusagewidget</string>
  <key>CFBundleExecutable</key><string>AIUsageWidget</string>
  <key>CFBundleIconFile</key><string>AIUsageWidget.icns</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleShortVersionString</key><string>$VERSION</string>
  <key>CFBundleVersion</key><string>$VERSION</string>
  <key>LSMinimumSystemVersion</key><string>12.0</string>
  <key>LSUIElement</key><true/>
  <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
EOF

# Ad-hoc signature so the bundle runs locally. Distribution outside this Mac needs a Developer ID.
codesign --force --deep --sign - "$APP"

ZIP="$ROOT/dist/AIUsageWidget-$VERSION-$RID.zip"
rm -f "$ZIP"
(cd "$OUT" && ditto -c -k --keepParent AIUsageWidget.app "$ZIP")
echo "Created $APP"
echo "Created $ZIP"
