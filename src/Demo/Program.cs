//  Copyright (C) 2024 Brian Humlicek

//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.

//  You should have received a copy of the GNU General Public License
//	along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Net;

namespace DSC.TLink
{
	public class Program
	{
		static void Main(string[] args)
		{
			using (TLinkClient client = new TLinkClient())
			{
				client.Connect(IPAddress.Parse("192.168.1.119"), 0xCAFE);
                client.SendMessageBCD("00-09-50-01-FF-0C-0A-0F-0E-01-8C");
                var one = client.ReadMessageBCD();
                client.SendMessageBCD("00-09-E0-00-FF-C0-A8-01-77-03-C8");
                var two = client.ReadMessageBCD();
                client.SendMessageBCD("00-0E-00-07-FF-00-07-00-00-01-00-21-CB-34-02-3C");
                var three = client.ReadMessageBCD();
                client.SendMessageBCD("00-0E-00-07-FF-00-07-00-01-01-00-21-E3-53-02-74");
                var four = client.ReadMessageBCD();
                client.SendMessageBCD("00-0B-00-07-FF-05-21-00-00-1F-9B-01-F1");
                var five = client.ReadMessageBCD();
                Console.ReadKey();
            }
        }
    }
}