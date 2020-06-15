using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp15.Command
{
	public class ClearLogCommand : ICommand
	{
		private readonly Action<string, string> _updateEventLog;

		public ClearLogCommand(Action<string, string> updateEventLog)
		{
			_updateEventLog = updateEventLog;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			_updateEventLog?.Invoke("Clear log", string.Empty);
		}

		public event EventHandler CanExecuteChanged;
	}
}
