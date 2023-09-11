// Write 8 bit values to 8 bit register address
int ArduCAM::wrSensorRegs8_8(const struct sensor_reg reglist[]) <- i2c
checks for 0xFF to end because it's using pointers instead of the array length ...
I deleted the garbage values at the end and just interrate over the length of the array

// Write 16 bit values to 8 bit register address
int ArduCAM::wrSensorRegs8_16(const struct sensor_reg reglist[]) <- i2c

// Write 8 bit values to 16 bit register address
int ArduCAM::wrSensorRegs16_8(const struct sensor_reg reglist[]) <- i2c

//I2C Array Write 16bit address, 16bit data
int ArduCAM::wrSensorRegs16_16(const struct sensor_reg reglist[]) <- i2c

write_reg <- something bus 



Init on Line 389

Up to 668 ....



ToDo:

1. Update Initialize to handle jpeg something something 
2. Figure out what P_CS, B_CS are ... (why are there 2 chip select things ... ?)