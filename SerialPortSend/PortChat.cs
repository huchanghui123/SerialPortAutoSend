using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;
using System.Threading;

namespace SerialPortSend
{
    public class PortChat
    {
        static bool _continue;
        static bool pause = false;
        static bool update_time = false;
        static long send_count = 0;
        static SerialPort _serialPort;
        static DateTime current_time = new DateTime();
        static string start_time = "";
        static StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        static CpuTemperatureReader cpuReader;
        static HardwareSensors hs;

        public static void StartTest()
        {
            //string name;
            string message;
            cpuReader = new CpuTemperatureReader();
            Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            Console.WriteLine("SetPortName: COM1");
            Console.WriteLine("SetPortBaudRate: 115200");
            Console.WriteLine("SetPortParity: None");
            Console.WriteLine("SetPortDataBits: 8");
            Console.WriteLine("SetPortStopBits: One");
            Console.WriteLine("SetPortHandshake: None");
            /*
            _serialPort.PortName = SetPortName("COM1");
            _serialPort.BaudRate = SetPortBaudRate(115200);
            _serialPort.Parity = SetPortParity(_serialPort.Parity);
            _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
            _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
            _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);
            */
            _serialPort.PortName = "COM1";
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), Parity.None + "", true);
            _serialPort.DataBits = 8;
            _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBits.One + "", true);
            _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), Handshake.None + "", true);

            //接收中文
            _serialPort.Encoding = System.Text.Encoding.GetEncoding("GB2312");
            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.RtsEnable = true;
            _serialPort.DtrEnable = true;

            _serialPort.Open();
            _continue = true;
            readThread.Start();
            start_time = GetComputerStartTime().ToString("F");
            Console.WriteLine("system start time: " + start_time);
            //name = Console.ReadLine();

            //Console.WriteLine("Type QUIT to exit");

            int num = 0;

            while (_continue)
            {
                //message = Console.ReadLine();
                if (_serialPort.IsOpen)
                {
                    if (!update_time)
                    {
                        //Console.WriteLine("start time............." + start_time);
                        hs = cpuReader.GetTemperaturesInCelsius();
                        Thread.Sleep(500);
                        _serialPort.WriteLine(
                           String.Format("<{0}>@{1}", "start_time", start_time));
                        Console.WriteLine(String.Format("CPU Package:{0}℃ Min:{1}℃ Max:{2}℃ CPU Speed:{3}GHz Memory Load:{4}%",
                                hs.temperature, hs.temperature_min, hs.temperature_max, hs.cpu_clock.ToString("f2"), hs.mem_load));
                    }
                    else {
                        if (!pause)
                        {
                            current_time = System.DateTime.Now;
                            //message = "连上了，哈哈哈哈哈啊哈哈";
                            hs = cpuReader.GetTemperaturesInCelsius();
                            message = String.Format("CPU Package:{0}℃ Min:{1}℃ Max:{2}℃ CPU Speed:{3}GHz Memory Load:{4}%",
                                hs.temperature,hs.temperature_min,hs.temperature_max,hs.cpu_clock.ToString("f2"),hs.mem_load);
                            num = message.Length;
                            send_count += num;
                            Thread.Sleep(1000);
                            _serialPort.WriteLine(message);
                            Console.WriteLine(current_time.ToString("HH:mm:ss") + "发送字节：" + send_count.ToString() + "Bytes");
                        }
                        
                    }
                }
            }

            readThread.Join();
            _serialPort.Close();
        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    Console.WriteLine(message);
                    if (stringComparer.Equals("<update_time_end>", message))
                    {
                        update_time = true;
                        Console.WriteLine("update_time_end-------!!!!!!!!");
                    } else if (stringComparer.Equals("reboot", message))
                    {
                        //1s 后重启计算机
                        System.Diagnostics.Process.Start("shutdown.exe", "-r -t 1");
                    }
                    else if (stringComparer.Equals("shutdown", message))
                    {
                        //1s 后关闭计算机
                        System.Diagnostics.Process.Start("shutdown.exe", "-s -t 1");
                    }
                    else if (stringComparer.Equals("pause", message))
                    {
                        pause = true;
                    }
                    else if (stringComparer.Equals("continue", message))
                    {
                        pause = false;
                    }
                }
                catch (TimeoutException) { }
            }
        }

        // Display Port values and prompt user to enter a port.
        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("{0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            //portName = Console.ReadLine();
            portName = "";
            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
            //baudRate = Console.ReadLine();
            baudRate = "";
            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        // Display PortParity values and prompt user to enter a value.
        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("{0}", s);
            }

            Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
            //parity = Console.ReadLine();
            parity = "";
            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        // Display DataBits values and prompt user to enter a value.
        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            //dataBits = Console.ReadLine();
            dataBits = "";
            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits.ToUpperInvariant());
        }

        // Display StopBits values and prompt user to enter a value.
        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available StopBits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter StopBits value (None is not supported and \n" +
             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
            //stopBits = Console.ReadLine();
            stopBits = "";
            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
            //handshake = Console.ReadLine();
            handshake = "";
            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }


        //获取系统开机时间
        private static DateTime GetComputerStartTime()
        {
            int result = Environment.TickCount & Int32.MaxValue;     //获取系统启动后运行的毫秒数
            TimeSpan m_WorkTimeTemp = new TimeSpan(Convert.ToInt64(Convert.ToInt64(result) * 10000));

            DateTime startTime = System.DateTime.Now.AddDays(m_WorkTimeTemp.Days);
            startTime = startTime.AddHours(-m_WorkTimeTemp.Hours);
            startTime = startTime.AddMinutes(-m_WorkTimeTemp.Minutes);
            startTime = startTime.AddSeconds(-m_WorkTimeTemp.Seconds);
            //Console.WriteLine("GetComputerStartTime:"+ startTime.ToString());
            return startTime;
        }
    }
}
