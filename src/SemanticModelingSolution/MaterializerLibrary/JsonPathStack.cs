using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaterializerLibrary
{
    internal class JsonPathStack
    {
        private List<JsonSourcePath> _list = new();

        public JsonPathStack()
        {
            this.CurrentPath = string.Empty;
        }

        public string CurrentPath { get; private set; }

        public int Count => _list.Count;

        public void Clear()
        {
            _list.Clear();
            this.CurrentPath = string.Empty;
        }

        public JsonSourcePath Push(string relativePath, bool isArrayElement = false)
        {
            JsonSourcePath item;
            if (CurrentPath.Length == 0)
            {
                item = new JsonSourcePath($"{relativePath}", isArrayElement);
            }
            else
            {
                item = new JsonSourcePath($"{CurrentPath}.{relativePath}", isArrayElement);
            }

            _list.Add(item);
            CurrentPath = item.Path;
            return item;
        }

        public JsonSourcePath Pop()
        {
            if (_list.Count == 0) return null;
            var index = _list.Count - 1;
            var item = _list[index];
            _list.RemoveAt(index);

            UpdateCurrentPath();
            return item;
        }

        public bool TryPeek(out JsonSourcePath jsonSourcePath)
        {
            if (_list.Count == 0)
            {
                jsonSourcePath = null;
                return false;
            }

            jsonSourcePath = _list[_list.Count - 1];
            return true;
        }

        private void UpdateCurrentPath()
        {
            if (_list.Count == 0)
            {
                CurrentPath = string.Empty;
            }
            else
            {
                var newLast = _list[_list.Count - 1];
                CurrentPath = newLast.Path;
            }
        }
    }
}
