using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AITranslator.View.Models
{
    public class ObservableQueue<T> : Queue<T>, INotifyCollectionChanged/*,INotifyPropertyChanged */
    {
        private readonly object _locker = new object();
        private int _maxCount;
        //public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public ObservableQueue(int maxCount)
        {
            _maxCount = maxCount;
        }

        public new void Enqueue(T item)
        {
            lock (_locker)
            {
                base.Enqueue(item);
                if (Count > _maxCount)
                {
                    base.TryDequeue(out T result);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, result, 0));
                }

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Count - 1));
            }
        }

        public new bool TryDequeue(out T result)
        {
            lock (_locker)
            {
                bool successed = base.TryDequeue(out result);
                if (successed)
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, result, 0));
                return successed;
            }
        }

        public new void Clear()
        {
            lock (_locker)
            {
                base.Clear();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}
