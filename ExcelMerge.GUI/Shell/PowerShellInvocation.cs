using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExcelMerge.GUI.Shell
{
	/// <summary>
	/// Windows PowerShell おけるコマンド入力から実行までの 1 サイクルの操作を公開します。
	/// </summary>
	public interface IPowerShellInvocation : INotifyPropertyChanged
	{
		InvocationStatus Status { get; }

		InvocationResult Result { get; }

		int Number { get; }

		string Script { get; set; }

		bool SetNextHistory();

		bool SetPreviousHistory();

		void Invoke();
	}

	public enum InvocationStatus
	{
		Ready,
		Invoking,
		Invoked,
	}

	public enum InvocationResultKind
	{
		Empty,
		Normal,
		Error,
	}

	public class InvocationResult
	{
		public InvocationResultKind Kind { get; }

		public string Message { get; }

		public InvocationResult() : this(InvocationResultKind.Empty, null) { }

		public InvocationResult(InvocationResultKind kind, string message)
		{
			this.Kind = kind;
			this.Message = message;
		}
	}

	public class PowerShellInvocation : IPowerShellInvocation
	{
		private readonly Action<PowerShellInvocation> _invocationAction;
		private readonly IReadOnlyList<string> _history;
		private int _currentHistoryIndex;
		private string _Script;
		private InvocationStatus _Status = InvocationStatus.Ready;
		private InvocationResult _Result;

		public InvocationStatus Status
		{
			get { return this._Status; }
			set
			{
				if (this._Status != value)
				{
					this._Status = value;
					this.RaisePropertyChanged();
				}
			}
		}

		public InvocationResult Result
		{
			get { return this._Result; }
			set
			{
				if (this._Result != value)
				{
					this._Result = value;
					this.RaisePropertyChanged();
					this.Status = InvocationStatus.Invoked;
				}
			}
		}

		public string Script
		{
			get { return this._Script; }
			set
			{
				if (this._Script != value)
				{
					this._Script = value;
					this.RaisePropertyChanged();
				}
			}
		}

		public int Number { get; }

		public bool SetNextHistory()
		{
			var index = this._currentHistoryIndex - 1;
			if (index >= 0 && index < this._history.Count)
			{
				this.Script = this._history[index];
				this._currentHistoryIndex = index;
				return true;
			}

			return false;
		}

		public bool SetPreviousHistory()
		{
			var index = this._currentHistoryIndex + 1;
			if (index >= 0 && index < this._history.Count)
			{
				this.Script = this._history[index];
				this._currentHistoryIndex = index;
				return true;
			}

			this.Script = "";
			this._currentHistoryIndex = this._history.Count;
			return false;
		}

		internal PowerShellInvocation(int number, Action<PowerShellInvocation> invocationAction, IReadOnlyList<string> history)
		{
			this.Number = number;
			this._invocationAction = invocationAction;
			this._history = history;
			this._currentHistoryIndex = this._history.Count;
		}

		public void Invoke()
		{
			this.Status = InvocationStatus.Invoking;
			this._invocationAction?.Invoke(this);
		}

		internal void SetResult(InvocationResult result)
		{
			this.Result = result;
			this.Status = InvocationStatus.Invoked;
		}

		#region INotifyPropertyChanged members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}

	public class PowerShellMessage : IPowerShellInvocation
	{
		public InvocationStatus Status { get; } = InvocationStatus.Invoked;

		public InvocationResult Result { get; } 

		int IPowerShellInvocation.Number
		{
			get { throw new NotSupportedException(); }
		}

		string IPowerShellInvocation.Script
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public PowerShellMessage(string message)
		{
			this.Result = new InvocationResult(InvocationResultKind.Normal, message);
		}

		bool IPowerShellInvocation.SetNextHistory()
		{
			throw new NotSupportedException();
		}

		bool IPowerShellInvocation.SetPreviousHistory()
		{
			throw new NotSupportedException();
		}

		void IPowerShellInvocation.Invoke()
		{
			throw new NotSupportedException();
		}

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { throw new NotSupportedException(); }
			remove { throw new NotSupportedException(); }
		}
	}
}
