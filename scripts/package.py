"""Build the plugin and write a Thunderstore ZIP from an explicit file list."""
import json
from pathlib import Path
import struct
import subprocess
import xml.etree.ElementTree as ET
from zipfile import ZipFile, ZIP_DEFLATED

root = Path(__file__).resolve().parents[1]
project = root / "src/LevelCapRemover.csproj"
package = root / "thunderstore"
manifest = json.loads((package / "manifest.json").read_text(encoding="utf-8"))
props = ET.parse(project).getroot().find("PropertyGroup")
version = props.findtext("Version")
if manifest["version_number"] != version:
    raise SystemExit("manifest.json and .csproj versions must match")
icon = (package / "icon.png").read_bytes()
if icon[:8] != b"\x89PNG\r\n\x1a\n" or struct.unpack(">II", icon[16:24]) != (256, 256):
    raise SystemExit("Expected a 256×256 PNG icon")

subprocess.run(["dotnet", "build", project, "-c", "Release"], check=True)
dll = project.parent / "bin/Release" / props.findtext("TargetFramework") / f"{project.stem}.dll"
dest = root / "dist" / f"{manifest['name']}-{version}.zip"
dest.parent.mkdir(exist_ok=True)
with ZipFile(dest, "w", compression=ZIP_DEFLATED) as archive:
    archive.write(package / "manifest.json", "manifest.json")
    archive.write(root / "README.md", "README.md")
    archive.write(package / "icon.png", "icon.png")
    archive.write(dll, f"BepInEx/plugins/{project.stem}/{dll.name}")
print(dest)
