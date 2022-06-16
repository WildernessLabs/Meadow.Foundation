# MQ135 Air Quality Sensor

## PCB Modifications

The commonly available "Flying Fish" MQ PCB is problematic. First, it has a comparator and potentiometer circuit that we don't need, but more problematic is the load resistor used for the output voltage divider. The MQ135 data sheet recommends a 20k resistor, but the PCB has a 1k.  The 1k (marked 102) needs to be removed and an external 20k needs to be added.

Also, since Vcc of the MQ sensors requires 5V, the analog output of the sensor is between 0 and 5V.  Meadows ADC is 5V tolerant, but only resolves 0-3.3V so a divider must be used between the MQ output and the Meadow input to allow reading across the full range of outputs.

