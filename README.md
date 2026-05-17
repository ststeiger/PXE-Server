# PXE Server

The PXE Server provides a DHCP server with support for PXE and network boot, a TFTP server and an HTTP server (which only serves static files).

## Features

Available loaders:
- SYSLINUX (bios, efi)
- iPXE (bios, efi)
- SHIM_GRUB2 (bios, efi with secure boot)
- UEFI_HTTP (efi-only, not tested on real hardware)

## Tested

Works to netboot the HP ZGX Nano G1n AI Station. <br />
That is to say NVIDIA ZGX-OS for NVIDIA Blackwell. <br />
You can find the iPXE bootloaders here at https://boot.ipxe.org/. <br />
For an ARM64 system like the Blackwell, you usually want the arm64-efi/snponly.efi and rename to ipxe.efi (if you or the DHCP set the filename to ipxe.efi). <br />
Then you find the Ubuntu netboot images here: https://cdimage.ubuntu.com/ubuntu/releases/24.04/release/netboot/arm64/<br />
Note that for the HP ZGX Nano G1n AI Station you can also download the boot ISO from HP: <br />
https://support.hp.com/hk-en/document/ish_14254582-14254596-16<br />
Direct link per 2026-05-17: <br />
https://ftp.hp.com/pub/softpaq/sp165001-165500/sp165496.iso

Mount the ISO on Linux or write it to USB with Rufus. 
To get the network boot files: 
Extract vmlinuz and initrd: Look inside the ISO (usually in /casper/). 
You need to copy these two files to your web server or TFTP directory.
Also the entire ISO File: Move the entire .iso file to a directory accessible via HTTP on your network.

autoexec.ipxe:

```
#!ipxe

set server_ip 192.168.1.100
set iso_path http://${server_ip}/ubuntu-24.04-arm64.iso

# Load the kernel and initrd from your server
kernel http://${server_ip}/vmlinuz
initrd http://${server_ip}/initrd

# The 'url' parameter is the "ISO Method" secret sauce
imgargs vmlinuz initrd=initrd ip=dhcp url=${iso_path} cloud-config-url=/dev/null

boot
```


## TODO

- Rewrite the hard-coded DHCP boot filename to support user configuration

## Quick start guide

1. Download the prepared wwwroot
2. Download minilinux (Minimal Linux) and extract to wwwroot
3. Change loader configuration:
    - syslinux: pxelinux.cfg\default
    - ipxe: boot.ipxe (it is a text script)
    - SHIM GRUB2: grub\grub.cfg
    - EFI HTTP: set the URL for the loader (example: http://192.168.1.100:80/shimx64.efi)
4. Configure PXE (pxe.conf)
5.  Run the server
    
## Setup test environment

This project has only been tested on Windows 10 x64.

Test EFI x64 boot:
1. Install [TAP-Windows](https://build.openvpn.net/downloads/releases/latest/tap-windows-latest-stable.exe)
2. Install [QEMU](https://qemu.weilnetz.de/w64/)
3. Download [OVMF EDK2](https://retrage.github.io/edk2-nightly/)
4. Run QEMU
```
 qemu-system-x86_64.exe ^
 -M q35 ^
 -cpu max ^
 -m 512M ^
 -bios RELEASEX64_OVMF.fd ^
 -netdev tap,id=mynet0,ifname=<TAP interface name> -device e1000,netdev=mynet0
 ```


Test PXE BIOS boot:
1. Install [VirtualBox](https://www.virtualbox.org/)
2. Create a new VM and select network boot.
3. Enjoy!

## If the server does not work

Check your firewall and open ports:
- UDP: 67, 69
- TCP: 80

On Windows:
```
 netsh advfirewall firewall add rule name="PXE DHCP" dir=in action=allow protocol=UDP localport=67
 netsh advfirewall firewall add rule name="PXE DHCP" dir=out action=allow protocol=UDP localport=67 
 netsh advfirewall firewall add rule name="PXE TFTP" dir=in action=allow protocol=UDP localport=69
 netsh advfirewall firewall add rule name="PXE TFTP" dir=out action=allow protocol=UDP localport=69 
 netsh advfirewall firewall add rule name="PXE HTTP" dir=in action=allow protocol=TCP localport=80
 netsh advfirewall firewall add rule name="PXE HTTP" dir=out action=allow protocol=TCP localport=80
```

## Notes

1. How to create the grub2 pxe loader: 
`grub-mkimage -d /usr/lib/grub/i386-pc/ -O i386-pc-pxe -p "(pxe)/grub" -o grub2.pxe pxe tftp pxechain boot http linux`

2. How does Shim work? Shim does not work over HTTP.
Use a signed Shim from Ubuntu 20.04. It's pre-compiled with the hard-coded filename "grubx64.efi", so you can't set the HTTP path. It only works with TFTP.

3. Memdisk does not work in EFI.
4. GRUB loopback cannot mount an ISO over HTTP :)
5. iPXE is sometimes slow when downloading over HTTP (maybe a bug?)

## List of other open source projects used

- [DHCPServer](https://github.com/jpmikkers/DHCPServer)
- [Tftp.Net](https://github.com/Callisto82/tftp.net)
- [Syslinux](https://wiki.syslinux.org/wiki/index.php?title=The_Syslinux_Project)
- [iPXE](https://ipxe.org/)
- [GRUB2](https://www.gnu.org/software/grub/)
- [UEFI SHIM Loader](https://github.com/rhboot/shim)
- [Minimal Linux](https://github.com/ivandavidov/minimal) and [Minimal Linux Live](http://minimal.idzona.com/#home)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
