using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace ExcelMerge.GUI.Views
{
    enum TargetType
    {
        All,
        First,
    }

    class DiffViewEventArgs<T> : EventArgs
    {
        public T Sender { get; }
        public IUnityContainer Container { get; }
        public TargetType TargetType { get; }

        public DiffViewEventArgs(T sender, IUnityContainer container, TargetType targetType = TargetType.All)
        {
            Sender = sender;
            Container = container;
            TargetType = targetType;
        }
    }
}
