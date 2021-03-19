$out = "$($args[0])\bootloaders"
New-Item -ItemType Directory -Force -Path $out
Get-ChildItem -Directory -Path "$($args[1])\HiSiBootloaders" | Copy-Item -Destination $out -Recurse -Container -Verbose
