using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Common
{
    public class StringLocker
    {
        private static StringLocker instance;
        static StringLocker()
        {
            instance = new StringLocker();
        }

        public static StringLocker Instance
        {
            get
            {
                return instance;
            }
        }

        System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.AutoResetEvent>
            lockList = new System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.AutoResetEvent>();

        System.Collections.Concurrent.ConcurrentDictionary<string, byte>
            deadLockList = new System.Collections.Concurrent.ConcurrentDictionary<string, byte>();


        public void Release(string key)
        {
            System.Threading.AutoResetEvent are = null;
            if(lockList.TryGetValue(key,out are))
            {
                are.Set();
            }
        }

        public class InternalLocker : IDisposable
        {
            public bool HasLocked { get
                {
                    return locker != null;
                }
            }
            System.Threading.AutoResetEvent locker;
            public InternalLocker(System.Threading.AutoResetEvent are)
            {
                this.locker = are;
            }
            public InternalLocker()
            {
            }
            public void Dispose()
            {
                this.locker.Set();
            }
        }

        public InternalLocker Lock(string key, TimeSpan ts)
        {
            System.Threading.AutoResetEvent are = null;
            bool locked = internalLock(key, ts,out are);
            if (!locked)
            {
                byte count = 0;
                while (true)
                {
                    if (deadLockList.TryGetValue(key, out count))
                    {
                        byte newValue = (byte)(count + 1);
                        if (deadLockList.TryUpdate(key, newValue, count))
                            break;
                    }
                }
                if (count > 3)
                {
                    Release(key);
                    deadLockList.TryRemove(key, out count);
                }
            }
            if(locked)
            {
                return new InternalLocker(are);
            }
            return new InternalLocker();
        }
        byte maxDeadlock = 3;
        private bool internalLock(string key,TimeSpan ts,out System.Threading.AutoResetEvent are)
        {
            are = null;
            if(lockList.TryGetValue(key,out are))
            {
                return are.WaitOne(ts);
            }
            else
            {
                are = new System.Threading.AutoResetEvent(true);
                if (lockList.TryAdd(key, are))
                {
                    return are.WaitOne(ts);
                }
                else
                {
                    return internalLock(key, ts, out are);
                }
            }
        }
    }
}