using System;
using System.IO.Ports;
using System.Threading;
using IIscAppLoaderInterfaces;
using log4net;

namespace SampleApp.Serial
{
  internal class SerialPortUsageExample
  {
    private readonly IIscAppConnector _appHost;

    private readonly ILog _logger = LogManager.GetLogger(typeof (IscApp));

    public SerialPortUsageExample(IIscAppConnector appHost)
    {
      _appHost = appHost;
    }

    // This example sends and receives data via serial port with a loop back device
    // The loopback device connects only RX and TX  
    public void CommunicateWithSerialPort(string message)
    {
      // Getting the serial port name and check if it exists
      if (_appHost.SerialPortName == string.Empty)
      {
        // We are sure that a serial port is connected and if we can not find it,
        // it is necessary to reset the usb-subsystem from the linux system to restore the connection
        _appHost.ResetUsbConfiguration();
        // Wait for the USB-subssytem to reinitialize and restore the connection
        Thread.Sleep(TimeSpan.FromSeconds(2.5));
        if (_appHost.SerialPortName == string.Empty)
        {
          _appHost.WriteValue(46, "No serial port");
          _logger.Error("Unable to find serial device after reset of the USB subsystem");
          return;
        }
      }
      using (var serialport = new SerialPort())
      {
        try
        {
          // configure the serial port
          serialport.PortName = _appHost.SerialPortName;
          serialport.BaudRate = 9600;
          serialport.Parity = Parity.None;
          serialport.DataBits = 8;
          serialport.StopBits = StopBits.One;
          serialport.ReadTimeout = 500;
          serialport.WriteTimeout = 500;

          serialport.Open();

          serialport.Write(message);
          Thread.Sleep(TimeSpan.FromSeconds(0.1));
          var response = serialport.ReadExisting();
          _appHost.WriteValue(46, response);
        }
        catch (Exception ex)
        {
          _logger.Error("Using serial port failed: " + ex);
        }
      }
    }
  }
}