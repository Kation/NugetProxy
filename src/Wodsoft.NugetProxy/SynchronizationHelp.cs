using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Wodsoft.NugetProxy
{
    public class SynchronizationHelp
    {
        public static Mutex _Mutex = new Mutex();
        private static Dictionary<object, SemaphoreSlim> _Slim = new Dictionary<object, SemaphoreSlim>();
        private static Dictionary<object, int> _Count = new Dictionary<object, int>();
        public static async Task Enter(object obj)
        {
            _Mutex.WaitOne();
            SemaphoreSlim slim;
            if (!_Slim.TryGetValue(obj, out slim))
            {
                slim = new SemaphoreSlim(1);
                _Slim.Add(obj, slim);
                _Count.Add(obj, 0);
            }
            _Count[obj]++;
            _Mutex.ReleaseMutex();
            await slim.WaitAsync();
        }

        public static async Task<bool> TryEnter(object obj)
        {
            _Mutex.WaitOne();
            SemaphoreSlim slim;
            if (!_Slim.TryGetValue(obj, out slim))
            {
                slim = new SemaphoreSlim(1);
                _Slim.Add(obj, slim);
                _Count.Add(obj, 0);
            }
            _Count[obj]++;
            _Mutex.ReleaseMutex();
            return await slim.WaitAsync(0);
        }

        public static void Exit(object obj)
        {
            _Mutex.WaitOne();
            SemaphoreSlim slim = _Slim[obj];
            if (_Count[obj] == 1)
            {
                _Count.Remove(obj);
                _Slim.Remove(obj);
            }
            else
                _Count[obj]--;
            slim.Release();
            _Mutex.ReleaseMutex();
        }
    }
}
