using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace ExcelMerge.GUI.Shell
{
    public interface IPowerShellHost
	{
		ReadOnlyObservableCollection<IPowerShellInvocation> Invocations { get; }
	}

	public class PowerShellHost : IPowerShellHost, IDisposable
	{
		private const string _errorMessage = "{0}\r\n    + CategoryInfo          : {1}\r\n    + FullyQualifiedErrorId : {2}";
		private const string _errorMessageWithPosition = "{0}\r\n{1}\r\n    + CategoryInfo          : {2}\r\n    + FullyQualifiedErrorId : {3}";

		private readonly Runspace _runspace;
		private readonly List<string> _history = new List<string>();
		private readonly ObservableCollection<IPowerShellInvocation> _invocations = new ObservableCollection<IPowerShellInvocation>();
		private ReadOnlyObservableCollection<IPowerShellInvocation> _readonlyInvocations;
		private int _count;

		ReadOnlyObservableCollection<IPowerShellInvocation> IPowerShellHost.Invocations
			=> this._readonlyInvocations ?? (this._readonlyInvocations = new ReadOnlyObservableCollection<IPowerShellInvocation>(this._invocations));

		public PowerShellHost()
		{
			this._runspace = RunspaceFactory.CreateRunspace();
		}

		public void Open()
		{
			this._runspace.Open();

			this._invocations.Add(new PowerShellMessage("Custom PowerShell Host - version 0.1"));
			this._invocations.Add(new PowerShellInvocation(++this._count, x => this.HandleInvocationRequested(x), this._history));
		}

		protected async void HandleInvocationRequested(PowerShellInvocation sender)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(sender.Script))
				{
					sender.SetResult(new InvocationResult());
				}
				else
				{
					using (var powershell = PowerShell.Create())
					{
						powershell.Runspace = this._runspace;
						powershell.AddScript(sender.Script);

						// ReSharper disable once AccessToDisposedClosure
						var results = await Task.Factory.FromAsync(powershell.BeginInvoke(), x => powershell.EndInvoke(x)).ConfigureAwait(false);
						var error = this.CreateResultIfError(powershell);

						sender.SetResult(error ?? await this.HandleResult(results));
					}

					this._history.Add(sender.Script);
				}
			}
			catch (Exception ex)
			{
				this.CreateErrorMessage(ex);
			}
			finally
			{
				this._invocations.Add(new PowerShellInvocation(++this._count, x => this.HandleInvocationRequested(x), this._history));
			}
		}

		protected virtual Task<InvocationResult> HandleResult(PSDataCollection<PSObject> results)
		{
			return Task.Run(() => this.OutString(results));
		}

		protected InvocationResult OutString<T>(IEnumerable<T> input)
		{
			try
			{
				var sb = new StringBuilder();

				using (var powershell = PowerShell.Create())
				{
					powershell.Runspace = this._runspace;
					powershell.AddCommand("Out-String");

					foreach (var result in powershell.Invoke(input))
					{
						sb.AppendLine(result.ToString());
					}
				}

				return new InvocationResult(InvocationResultKind.Normal, sb.ToString());
			}
			catch (Exception ex)
			{
				return new InvocationResult(InvocationResultKind.Error, this.CreateErrorMessage(ex));
			}
		}

		protected InvocationResult CreateResultIfError(PowerShell powershell)
		{
			if (powershell.Streams.Error == null || powershell.Streams.Error.Count == 0) return null;

			var sb = new StringBuilder();
			foreach (var error in powershell.Streams.Error)
			{
				sb.AppendLine(string.Format(_errorMessageWithPosition, error, error.InvocationInfo.PositionMessage, error.CategoryInfo, error.FullyQualifiedErrorId));
			}

			return new InvocationResult(InvocationResultKind.Error, sb.ToString());
		}

		protected string CreateErrorMessage(Exception ex)
		{
			var container = ex as IContainsErrorRecord;
			if (container?.ErrorRecord == null)
			{
				return ex.Message;
			}

			var invocationInfo = container.ErrorRecord.InvocationInfo;
			if (invocationInfo == null)
			{
				return string.Format(_errorMessage, container.ErrorRecord, container.ErrorRecord.CategoryInfo, container.ErrorRecord.FullyQualifiedErrorId);
			}

			if (invocationInfo.PositionMessage != null && _errorMessage.IndexOf(invocationInfo.PositionMessage, StringComparison.Ordinal) != -1)
			{
				return string.Format(_errorMessage, container.ErrorRecord, container.ErrorRecord.CategoryInfo, container.ErrorRecord.FullyQualifiedErrorId);
			}

			return string.Format(_errorMessageWithPosition, container.ErrorRecord, invocationInfo.PositionMessage, container.ErrorRecord.CategoryInfo, container.ErrorRecord.FullyQualifiedErrorId);
		}

		public void Dispose()
		{
			this._runspace?.Dispose();
		}
	}
}
