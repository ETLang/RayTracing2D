using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing2D
{
    public class Disposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        protected Disposable() { }

        ~Disposable()
        {
            IsDisposed = true;
            OnDispose(true);
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            GC.SuppressFinalize(this);
            OnDispose(false);
        }

        protected virtual void OnDispose(bool finalizing)
        {
        }
    }
}