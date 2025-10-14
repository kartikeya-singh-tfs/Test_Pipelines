using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Thermofisher.Opal.IpcDataAccessModule.DataAccess
{
    public static class EventDispatcher
    {
        // Fire-and-Forget
        //Invokes all handlers using GetInvocationList()
        //Runs them in the background(without await)
        //Optionally catches/logs exceptions to prevent crashing
        public static void RaiseEvent<T1, T2>(
            this Action<T1, T2> handlers,
            T1 arg1,
            T2 arg2,
            ILogger logger = null
        )
        {
            if (handlers == null)
                return;

            foreach (var handler in handlers.GetInvocationList().Cast<Action<T1, T2>>())
            {
                handler.BeginInvoke(
                    arg1,
                    arg2,
                    ar =>
                    {
                        try
                        {
                            // use EndInvoke to avoid leaks or missed exceptions.
                            handler.EndInvoke(ar);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, "RaiseEvent error");
                        }
                    },
                    null
                );
            }
        }
    }
}
