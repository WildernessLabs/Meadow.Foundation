# Meadow.Foundation FT232H Driver

The FT232H is an IO Expander that provided GPIO, I2C and SPI capabilities over USB using FTDI silicon.  There are a few things to know about its use, however, that are not readily apparent.

1. The `FT232H` driver in Meadow.Foundation is designed for and tested on Windows.  Since it uses an underlying OS USB driver, operation on Linux or Mac is untested and unlikely to work.
2. FTDI provides 2 ways of communicating with the chip: MPSSE and HID. The Meadow.Foundation driver allows you to choose which one to use. Which you choose depends on the function you need, and affects how the driver is deployed.

## MPSSE (for SPI)

FTDI provides a driver called `mpsse`.  This driver has a simple C API interface that, in theory, makes interop easy.  Unfortunately this driver has a bug in it that makes all I2C traffic invalid (specifically reads from the I2C bus are the right length, but only the first by read is valid, all others are `0x00`).  This behavior manifests even when using the FTDI sample application written in C running in the debugger, so it's definitely an FTDI driver problem, not a Meadow.Foundation or interop issue.  

Use MPSSE if you want to use SPI.  In order to use MPSSE, you must also deploy the appropriate `libmpsse.dll` with your application. To make them easier to find, compiled binaries for these are found in the Meadow.Foundation repository under the `Native/Windows` in the folder that matches your target architecture.

## HID (for I2C)

The FT232 also had an HID driver.  HID interop is significantly more complex than MPSSE, however it doesn't have the MPSSE bug. We have gotten I2C working with the HID driver, but SPI is broken with HID.  That means that if you need to use I2C, you want to use the HID driver.
