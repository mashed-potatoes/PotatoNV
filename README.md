<img src="./assets/header.png" width="100%" height="auto" alt="PotatoNV">

[![Build status](https://ci.appveyor.com/api/projects/status/0ra9b57aakdo5ms6?svg=true)](https://ci.appveyor.com/project/mashed-potatoes/potatonv)
![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/mashed-potatoes/PotatoNV?include_prereleases)
![GitHub](https://img.shields.io/github/license/mashed-potatoes/PotatoNV)
![GitHub All Releases](https://img.shields.io/github/downloads/mashed-potatoes/PotatoNV/total)
[![CodeFactor](https://www.codefactor.io/repository/github/mashed-potatoes/potatonv/badge)](https://www.codefactor.io/repository/github/mashed-potatoes/potatonv)

---

## Download

### 👉 [Click here to download the latest version](https://github.com/mashed-potatoes/PotatoNV/releases/download/v2.2.1/PotatoNV-next-v2.2.1-x86.exe).

Get binaries for Windows in [the releases section](https://github.com/mashed-potatoes/PotatoNV/releases).
For Linux or macOS consider using the [PotatoNV-crossplatform](https://github.com/mashed-potatoes/PotatoNV-crossplatform).

## Getting started

Just follow this video guide: https://www.youtube.com/watch?v=YkGugQ019ZY.

## How it works (in a nutshell)

Even before creating PotatoNV, [@TishSerg](https://github.com/TishSerg) discovered that unlock key can be rewritten with the **SHA256 hash** of the desired key to the `USRKEY` property. However, to access **_NVME_** _(a raw partition that stores stuff like serial number, device traits, etc.)_, a user should flash _custom_ recovery or gain temporary root privileges. But both methods are complex and are not guaranteed to work. After researching the legacy bootloader of some Huawei devices, I've found a `nve` command, which allows to read or write any property in the **_NVME_** partition. Of course, this command requires an unlocked bootloader.
So it remains to find a way to quickly unlock the bootloader. The way out is quite simple - use the bootloader from the board software.

The program uploads a special **_"USB bootloader"_** _(exported from the board software)_ through the `DOWNLOAD_VCOM` mode. **_VCOM_** is smth like **_EDL_** on Qualcomm devices: it can be triggered by a system failure or by **shorting testpoint**.
After uploading the bootloader, the device should switch to the fastboot mode. The "USB bootloader" has an important trait: it's **unlocked out-of-the-box**, so it allows to execute any command.

So, we're just going to send a command through the USB bulk interface to write SHA256 hash to USRKEY and reboot the device.

That's it.

## Tested devices

Device | Model | Bootloader
------ | ----- | ----------
Huawei P8 Lite (2015) **(!)** | `ALE` | Kirin 620
Honor 5C / 7 Lite | `NEM` | Kirin 65x (A)
Honor 7X | `BND` | Kirin 65x (A)
Honor 9 Lite | `LLD` | Kirin 65x (A)
Huawei MediaPad T5 | `AGS2` | Kirin 65x (A)
Huawei Nova 2 | `PIC` | Kirin 65x (A)
Huawei P10 Lite | `WAS` | Kirin 65x (A)
Huawei P20 Lite / Nova 3e | `ANE` | Kirin 65x (A)
Huawei P8 Lite (2017) | `PRA` | Kirin 65x (A)
Huawei P9 Lite | `VNS` | Kirin 65x (A)
Huawei Y9 (2018) | `FLA` | Kirin 65x (A)
Huawei MediaPad M5 Lite | `BAH2` | Kirin 65x (B)
Huawei Nova 2i / Mate 10 Lite | `RNE` | Kirin 65x (B)
Huawei P Smart 2018 | `FIG` | Kirin 65x (B)
Honor 8 Pro / V9 | `DUK` | Kirin 950
Honor 8 | `FRD` | Kirin 950
Huawei P9 Standart | `EVA` | Kirin 950
Honor 9 | `STF` | Kirin 960
Huawei Mate 9 Pro | `LON` | Kirin 960
Huawei Mate 9 | `MHA` | Kirin 960
Huawei MediaPad M5 | `CMR` | Kirin 960
Huawei Nova 2s | `HWI` | Kirin 960
Huawei P10 | `VTR` | Kirin 960

## Donate

**It would be much appreciated if you want to make a small donation to support my work!** 

PayPal: http://paypal.me/teegris.

Buy Me a Coffee: https://www.buymeacoffee.com/teegris.

Ko-Fi: https://ko-fi.com/certs.


## License

Logo by Icons8.

All bootloaders are Huawei Technologies Co., Ltd. property.

This project is not affiliated with Huawei.

---

Unlock tool for Huawei devices on Kirin SoC.
Copyright (C) 2020  mashed-potatoes

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
