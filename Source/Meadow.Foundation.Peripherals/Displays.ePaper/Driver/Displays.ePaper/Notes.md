## Links

Adafruit Arduino drivers: https://github.com/adafruit/Adafruit_EPD
Waveshare drivers: https://github.com/waveshare/e-Paper/tree/master/Arduino%20UNO

Adafruit display notes: https://learn.adafruit.com/adafruit-eink-display-breakouts?view=all

Good displays: https://github.com/ZinggJM/GxEPD/blob/master/src/GxGDEW027C44/GxGDEW027C44.cpp



## Controllers
IL0731*
### IL0373
- 1.54" tri color 152x152
- 2.13" tri color 212x104
- 2.13" BW 212x104
- 2.9" BW 296x128


IL0376F*
IL0398 - 4.2" tri
IL3820 - 2.9" 296x128 BW from WaveShare
IL3897*
IL91874 - 2.7" tri (also supports BW) https://adafruit-circuitpython-il91874.readthedocs.io/en/latest/
SSD1608 - 2.13" BW
SSD1675 - 1.54" BW 200x200 (also supports 2.13" 104x212 BW - not Adadruit)
SSD1675B

* not supported by Adafruit

## Mapping
*EPD1i54 200x200 -> SSD1608
EPD1i54b 200x200 -> ???? .... compare to 2.13 and 2.9 - also known as GDEW0154Z04
EPD1i54c 152x152 -> IL0373?

EPD2i13 128x250 -> ????
EPD2i13b 104x212 -> IL0731

EPD2i7 264x176 -> IL91874 ???
*EPD2i7b 264x176  -> IL91874

*EPD2i9 296x128 -> SSD1608
EPD2i9b 296x128 -> ... could be IL0373?? CHECK

EPD4i2b 400x300 -> IL0398 (nope)


