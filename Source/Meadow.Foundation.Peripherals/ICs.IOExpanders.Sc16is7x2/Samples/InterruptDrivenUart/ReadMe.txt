This project tests reading data from the SC16IS7x2 UART using the interrupt signal from SC16IS7x2.
The UART channel A is tested by using a second Meadow running the "SerialTestGenerator" test project.
The GPIO's are also tested by configuring GP0-3 as output and GP4-7 as input.

Requirements:
  - TWO Meadow devices (during driver development it's nice to separate sender and receiver)
  - An SC16IS7x2 breakout
  - A push button
  - A 1K ohm resistor

Hookup:
  - The "first" Meadow running the "InterruptDrivenUart" project.
    The SC16IS7x2 is connected like this (breakout board pin -> Meadow pin):
    VCC -> 3.3V
    GND -> GND
    SCL -> CLK (D8) with a 4.7K ohm pull-up to 3.3V. (Picture may show 1K)
    SDA -> DAT (D7) with a 4.7K ohm pull-up to 3.3V. (Picture may show 1K)
    A0  -> 3.3V
    A1  -> 3.3V
    IRQ -> D0 with a 1K ohm pull-up to 3.3V
    GP0 -> GP4	(Configured as output -> input)
    GP1 -> GP5	(Configured as output -> input)
    GP2 -> GP6	(Configured as output -> input)
    GP3 -> GP7	(Configured as output -> input)
    
  - The "second" Meadow running the "SerialTestGenerator" project to generate messages:
    A push button should be connected between 3.3V and the D0 GIPO.
    The Meadow COM1 TX (D12) should be connected to the SC16IS7x2 PortA RX pin.
    The Meadow COM1 RX (D13) should be connected to the SC16IS7x2 PortA TX pin.
    
While testing, it is the output from the "first" Meadow ("InterruptDrivenUart") that is most interesting.
You can connect the 3.3V and GND of the two Meadows together, so that the SerialTestGenerator app 
on the "second" Meadow is always running.
A long button press will generate a message of 24 bytes. ("Meadow calling! Wakeup!\n")
A short button press will generate a message of 300 bytes.

The "first" Meadow will set up SC16IS7x2 to listen for UART messages using IRQ, then execute this loop:
  - Wait for 15 seconds.
  - Write the texts received by the DataReceived event handler.
  - Check for any left over data in the FIFO buffers. Both the hardware and software buffers.
    If the IRQ handler has left any data in the buffers, that would indicate a problem with the IRQ signal. 

NB! The first time you send a message to the SC16IS7x2 after a reboot, you will probably get a buffer overrun error
    if the message is long (300 bytes). This happens because the HW FIFO is only 64 bytes, and the Meadow uses a very 
    long time to route the first interrupt call to the SC16IS7x2 driver. 
    I think this is the reason: https://github.com/WildernessLabs/Meadow_Issues/issues/74
    However, any following messages are handled fine.
    Also, if the first message is short, it will work fine because it will fit inside the 64 byte HW FIFO.

NB2! I you keep pressing the button quite often during the test, you might get a message like this in the output:
     "ReadChannelRegister: IOState A CC (Corrected value!)"
     This can happen when the GPIO's are changing while the UART is kept busy. For some reason when reading IOState
     the result may be 0 even though it clearly shouldn't be. I have put in a correction mechanism in the 
     Sc16is7x2.ReadChannelRegister method to correct this special case. (If address is IOState and result is 0,
     we're reading it again. If the new result is different from 0 we use that instead and print a warning)

I hope this will be useful to someone.
Have fun!
Kaare Wehn
