using MqttNetTest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttNetTestReceive
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var clientReceive = new CliendReceive();

            Console.Read();
        }
    }
}
