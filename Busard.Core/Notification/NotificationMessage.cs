using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Busard.Core.Notification
{
    public enum MessageSeverity : ushort
    {
        Information = 0,
        Warning = 1,
        Critical = 2
    }

    [Flags]
    public enum MessageType : uint
    {
        UserError = 0,
        Information = 1,
        Connectivity = 2,
        SystemFailure = 4,
        BusardError = 8
    }

    public struct MessageItem
    {
        public readonly string Key;
        public readonly string Value;
        public MessageItem(string key, string value) { 
            this.Key = key;
            value = string.Join(" ", value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList().Select(x => x.Trim()));
            this.Value = value; 
        }
        public override string ToString() { return $"{this.Key} : {this.Value}"; }
    }

    public sealed class NotificationMessage : IList<MessageItem>
    {
        private readonly List<MessageItem> _items;
        public MessageSeverity Severity { get; set; }
        public string Subject { get; set; }

        public NotificationMessage() : base() { this._items = new List<MessageItem>(); }

        public NotificationMessage(string message, string subject, MessageSeverity severity) : base()
        {
            this._items = new List<MessageItem>(1) { new MessageItem(severity.ToString(), message) };
            this.Severity = severity;
            this.Subject = subject;
        }

        public MessageItem this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public void Add(MessageItem item) => _items.Add(item);

        public void Clear() => _items.Clear();

        public bool Contains(MessageItem item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(MessageItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<MessageItem> GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(MessageItem item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, MessageItem item) => _items.Insert(index, item);

        public bool Remove(MessageItem item) => _items.Remove(item);

        public void RemoveAt(int index) => _items.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return item;
            }
        }

        public override string ToString() { return String.Join('\n', _items.Select(i => i.ToString())) ; }

    }
}
