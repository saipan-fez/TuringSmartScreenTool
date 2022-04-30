using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenCvSharp;
using System;
using System.IO.Ports;
using TuringSmartScreenLibrary.Entities;
using TuringSmartScreenLibrary.Interfaces;

namespace TuringSmartScreenLibrary
{
    public sealed class TuringSmartScreenDevice : IDisposable, ITuringSmartScreenDevice
    {
        private const int DeviceWidth = 320;            // 0 degree
        private const int DeviceHeight = 480;           // 0 degree

        private readonly ILogger<TuringSmartScreenDevice> _logger;

        private readonly object _serialPortLockObj = new object();
        private readonly Mat<Vec3b> _frontMat = new Mat<Vec3b>(DeviceHeight, DeviceWidth);   // always landscape(not depend on rotation)
        private readonly Mat<Vec3b> _backMat  = new Mat<Vec3b>(DeviceHeight, DeviceWidth);   // always landscape(not depend on rotation)

        private SerialPort _serialPort;
        private bool _isDisposed = false;

        private bool _isOpen = false;
        private string _name;
        private Rotation _rotation;
        private byte _brightness;
        private bool _isScreenTurnedOn;

        public Capabilities BrightnessCapabilities => new Capabilities(255, 0, 1, 255);
        public bool IsOpen => _isOpen;

        public string Name
        {
            get { ThrowExceptionIfNotOpened(); return _name; }
        }
        public Rotation Rotate
        {
            get { ThrowExceptionIfNotOpened(); return _rotation; }
        }
        public double Brightness
        {
            get { ThrowExceptionIfNotOpened(); return Math.Max(255 - _brightness, 0); }
        }
        public bool IsScreenTurnedOn
        {
            get { ThrowExceptionIfNotOpened(); return _isScreenTurnedOn; }
        }

        public TuringSmartScreenDevice(ILogger<TuringSmartScreenDevice> logger = null)
        {
            _logger = logger ?? NullLogger<TuringSmartScreenDevice>.Instance;
        }

        public Size GetScreenSize()
        {
            var width = Rotate switch
            {
                Rotation.Degree0 or Rotation.Degrees180 => DeviceWidth,
                Rotation.Degrees90 or Rotation.Degrees270 => DeviceHeight,
                _ => throw new InvalidOperationException()
            };
            var height = Rotate switch
            {
                Rotation.Degree0 or Rotation.Degrees180 => DeviceHeight,
                Rotation.Degrees90 or Rotation.Degrees270 => DeviceWidth,
                _ => throw new InvalidOperationException()
            };

            return new Size(width, height);
        }

        public void Open(SerialDevice serialDevice, Rotation rotation)
        {
            if (IsOpen)
                return;

            try
            {
                _logger.LogInformation("Device opening... Port:{portName} Rotation:{rotation}", serialDevice.PortName, rotation);

                lock (_serialPortLockObj)
                {
                    _serialPort = new SerialPort(serialDevice.PortName)
                    {
                        // property ref: UsbMonitor.exe (official tool)
                        DtrEnable = true,
                        RtsEnable = true,
                        ReadTimeout = 1000,
                        BaudRate = 115200,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Parity = Parity.None
                    };
                    _serialPort.Open();
                }

                _isOpen = true;
                _name = serialDevice.PortName;

                // default value in hardware is unknown.
                // so set constant value when device is opened.
                SetBrightness(BrightnessCapabilities.Default);
                TurnOnScreen();
                SetRotation(rotation);

                _logger.LogInformation("Device opened.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error");
                Close();
                throw;
            }
        }

        public void Close()
        {
            try
            {
                lock (_serialPortLockObj)
                {
                    if (_serialPort?.IsOpen ?? false)
                    {
                        _serialPort?.Close();
                        _logger.LogInformation("Device closed.");
                    }
                    _serialPort?.Dispose();
                    _serialPort = null;
                }
            }
            catch (Exception ex)
            {
                // nop
                _logger.LogError(ex, "error");
            }
            finally
            {
                _isOpen = false;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName, "Already disposed.");

            try
            {
                Close();

                _frontMat?.Dispose();
                _backMat?.Dispose();
            }
            catch (Exception ex)
            {
                // nop
                _logger.LogError(ex, "error");
            }
            finally
            {
                _logger.LogInformation("Device disposed.");
                _isDisposed = true;
            }
        }

        public void RefreshScreen(Mat<Vec3b> mat)
        {
            ThrowExceptionIfNotOpened();

            try
            {
                // TODO: check mat cols/rows

                if (Rotate is not Rotation.Degree0)
                {
                    var rotateFlag = Rotate switch
                    {
                        Rotation.Degrees90 => RotateFlags.Rotate90Clockwise,
                        Rotation.Degrees180 => RotateFlags.Rotate180,
                        Rotation.Degrees270 => RotateFlags.Rotate90Counterclockwise,
                        _ => throw new InvalidOperationException()
                    };
                    using var backBufferMat = new Mat<Vec3b>();
                    Cv2.Rotate(mat, backBufferMat, rotateFlag);
                    backBufferMat.CopyTo(_backMat);
                }
                else
                {
                    mat.CopyTo(_backMat);
                }

                using var diff = new Mat<Vec3b>();
                using var diffGray = new Mat();
                Cv2.Absdiff(_frontMat, _backMat, diff);
                Cv2.CvtColor(diff, diffGray, ColorConversionCodes.BGR2GRAY);
                Cv2.FindContours(diffGray, out var contours, out var _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                // TODO: If total dimension of contours is exceed the threshold, should refresh entires?

                foreach (var points in contours)
                {
                    var rect = Cv2.BoundingRect(points);

                    _logger.LogTrace("Update rect");
                    _logger.LogTrace("    left  :{left}", rect.Left);
                    _logger.LogTrace("    top   :{top}", rect.Top);
                    _logger.LogTrace("    right :{right}", rect.Right);
                    _logger.LogTrace("    bottom:{bottom}", rect.Bottom);
                    _logger.LogTrace("    width :{width}", rect.Width);
                    _logger.LogTrace("    height:{height}", rect.Height);

                    SendReg(Command.DISPLAY_BITMAP, rect.Left, rect.Top, rect.Right - 1, rect.Bottom - 1);
                    // Hardware Specification
                    //   - 2byte per pixel
                    //   - format:0bRRRR_RGGG_GGGB_BBBB
                    //     - R:5bit
                    //     - G:6bit
                    //     - B:5bit
                    //   - big endian
                    const int bpp = 2;
                    var buffer = new byte[bpp * rect.Width * rect.Height];
                    for (var y = rect.Y; y < rect.Bottom; y++)
                    {
                        for (var x = rect.X; x < rect.Right; x++)
                        {
                            var pix = _backMat.At<Vec3b>(y, x);

                            // 8 -> 5bit(R,B)
                            // 8 -> 6bit(G)
                            var b = (byte)(pix[0] >> 3);    // b
                            var g = (byte)(pix[1] >> 2);    // g
                            var r = (byte)(pix[2] >> 3);    // r

                            // to 16bit
                            var v = (ushort)((r << 11) | (g << 5) | b);

                            // to big endian
                            byte v1, v2;
                            if (BitConverter.IsLittleEndian)
                            {
                                // reverse
                                v1 = (byte)(v & 0x00FF);
                                v2 = (byte)((v >> 8) & 0x00FF);
                            }
                            else
                            {
                                v1 = (byte)((v >> 8) & 0x00FF);
                                v2 = (byte)(v & 0x00FF);
                            }

                            var bufferIndex = (x - rect.X) * bpp + (y - rect.Y) * rect.Width * bpp;
                            buffer[bufferIndex + 0] = v1;
                            buffer[bufferIndex + 1] = v2;
                        }
                    }

                    WriteSerialPort(buffer, 0, buffer.Length);
                }
                _backMat.CopyTo(_frontMat);
            }
            catch (Exception ex)
            {
                // TODO: error handling
                _logger.LogError(ex, "error");
            }
        }

        public void TurnOffScreen()
        {
            ThrowExceptionIfNotOpened();

            _logger.LogInformation("Screen turned off.");

            SendReg(Command.SCREEN_OFF);
            _isScreenTurnedOn = false;
        }

        public void TurnOnScreen()
        {
            ThrowExceptionIfNotOpened();

            _logger.LogInformation("Screen turned on.");

            SendReg(Command.SCREEN_ON);
            _isScreenTurnedOn = true;
        }

        public void ClearScreen()
        {
            ThrowExceptionIfNotOpened();

            _logger.LogInformation("Screen clear.");

            SendReg(Command.CLEAR);
        }

        public void SetRotation(Rotation rotation)
        {
            ThrowExceptionIfNotOpened();

            _rotation = rotation;
        }

        public void SetBrightness(double value)
        {
            ThrowExceptionIfNotOpened();

            var inrangeValue = (byte)Math.Max(BrightnessCapabilities.Min, Math.Min(BrightnessCapabilities.Max, value));
            var hardwareValue = (byte)Math.Max(255 - inrangeValue, 0);

            _logger.LogInformation("SetBrightness value:{brightness}.", hardwareValue);

            SendReg(Command.SET_BRIGHTNESS, hardwareValue);
            _brightness = hardwareValue;
        }

        private void SendReg(Command cmd, int x = 0, int y = 0, int destX = 0, int destY = 0)
        {
            var buffer = new byte[6];
            buffer[0] = (byte)(x >> 2);
            buffer[1] = (byte)(((x & 3) << 6) + (y >> 4));
            buffer[2] = (byte)(((y & 15) << 4) + (destX >> 6));
            buffer[3] = (byte)(((destX & 63) << 2) + (destY >> 8));
            buffer[4] = (byte)(destY & 255);
            buffer[5] = (byte)cmd;

            WriteSerialPort(buffer, 0, buffer.Length);
        }

        private void WriteSerialPort(byte[] data, int offset = 0, int length = 0)
        {
            lock (_serialPortLockObj)
            {
                if (_serialPort?.IsOpen ?? false)
                    _serialPort.Write(data, offset, length);
            }
        }

        private void ThrowExceptionIfNotOpened()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName, "Already disposed.");

            if (!IsOpen)
                throw new InvalidOperationException("The device is not open. You must call Open() before any operations.");
        }

        // ref:
        // https://github.com/mathoudebine/turing-smart-screen-python/tree/4278f6942c59a08eee01470704b0552c5057fb56
        private enum Command : byte
        {
            // If you send a RESET command, it will disconnect once and then reconnect immediately.
            RESET = 101,
            CLEAR = 102,
            SCREEN_OFF = 108,
            SCREEN_ON = 109,
            SET_BRIGHTNESS = 110,
            DISPLAY_BITMAP = 197
        }
    }
}
