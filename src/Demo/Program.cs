// {LICENSE HEADER.txt}

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

using DSC.TLink.DLSProNet;
using DSC.TLink.ITv2;
using DSC.TLink.ITv2.Messages;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace DSC.TLink
{
	public class Program
	{
		static void MyMethod<T>()
		{

		}
		static void Main(string[] args)
		{
			int? ni = null;
			if (!ni.HasValue) return;
			int i = ni.Value;

			using (var api = new ITv2API(new Logger()))
			{
				api.Open();
			}
			//         using (DLSProNetAPI session = new DLSProNetAPI(null))
			//         {
			//             session.Open(IPAddress.Parse("192.168.1.119"), 0xCAFE);
			//             Console.WriteLine("Waiting...");
			//             Console.ReadKey();
			//         }
			//         return;

			//using (TLinkClient client = new TLinkClient(null))
			//{
			//	client.Connect(IPAddress.Parse("192.168.1.119"), 0xCAFE);
			//	client.SendMessageBCD("00-09-50-01-FF-0C-0A-0F-0E-01-8C");
			//	var one = client.ReadMessageBCD();
			//	client.SendMessageBCD("00-09-E0-00-FF-C0-A8-01-77-03-C8");
			//	var two = client.ReadMessageBCD();
			//	client.SendMessageBCD("00-0E-00-07-FF-00-07-00-00-01-00-21-CB-34-02-3C");
			//	var three = client.ReadMessageBCD();
			//	client.SendMessageBCD("00-0E-00-07-FF-00-07-00-01-01-00-21-E3-53-02-74");
			//	var four = client.ReadMessageBCD();
			//	client.SendMessageBCD("00-0B-00-07-FF-05-21-00-00-1F-9B-01-F1");
			//	var five = client.ReadMessageBCD();
			//	Console.ReadKey();
			//}
		}

	//	public int? nint
	//	{
	//		get => GetT<int>();
	//		set => SetT(value);
	//	}
	//	public T GetT<T>() => getstorage<T>().t;

	//	public void SetT<T>(T value) => getstorage<T>().t = value;

	//	storage<T> getstorage<T>() => programstorage switch
	//	{
	//		storage<T> gint => gint,
	//		_ => throw new Exception()
	//	};
	//}
	//class storage<T> : Program
	//{
	//	T? field;
	//	int? length;
	//	public T t
	//	{
	//		get
	//		{
	//			if (length == null)
	//			{
	//				field = setDefault() ?? throw new ArgumentNullException();
	//			}
	//			return field!;
	//		}
	//		set
	//		{
	//			field = value ?? throw new ArgumentNullException();
	//			length = dostuff();
	//		}
	//	}
	//	int dostuff() => 0;
	//	T? setDefault() => default(T);

	}
	class Logger : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (logLevel >= LogLevel.Debug)
			{

				IReadOnlyList<KeyValuePair<string, object?>> logValues = state as IReadOnlyList<KeyValuePair<string, object?>>;
				string s = logValues[0].Value as string;
				if (s != null)
				{
					Console.WriteLine(s);
					File.WriteAllText("log.txt", s);
				}
			}
		}
	}
}