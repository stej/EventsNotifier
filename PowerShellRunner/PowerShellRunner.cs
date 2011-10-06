using System;
using System.Collections;
using System.Management.Automation;
using System.Windows.Forms;
using System.IO;
using EventsChecker.Core;
using NLog;

namespace EventsChecker.Notifier
{
	public class PowerShellRunner
	{
		string _script;
		static Logger _logger;

		static PowerShellRunner()
		{
			_logger = LogManager.GetLogger("NotifierPowerShellRunner");
		}

		public PowerShellRunner(string script)
		{
			if (!Path.IsPathRooted(script))
				script = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, script);
			_script = script;
		}
		public void Run(TextBox tb, IChecker checker, string header, IEnumerable messages)
		{
			Console.WriteLine("Running script {0}", _script);
			try
			{
				var ps = PowerShell.Create();
				ps.Runspace.SessionStateProxy.SetVariable("textbox", tb);
				ps.Runspace.SessionStateProxy.SetVariable("checker", checker);
				ps.Runspace.SessionStateProxy.SetVariable("header", header);
				ps.Runspace.SessionStateProxy.SetVariable("message", messages);
				ps.AddScript(_script);
				ps.Invoke();

				Console.WriteLine(".. done");
				_logger.Info("notified");

				if (ps.Streams.Error.Count > 0)
				{
					_logger.Error("Count of errors: {0}", ps.Streams.Error.Count);
					foreach (var e in ps.Streams.Error)
					{
						_logger.ErrorException("Exception when running PowerShell notifier", e.Exception);
                        _logger.Error(e.ErrorDetails);
					}
				}
			}
			catch(Exception e)
			{
				_logger.ErrorException("Error when running notifier", e);
			}
		}
	}
}
