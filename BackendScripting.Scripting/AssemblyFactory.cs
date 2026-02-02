using System.Reflection;
using System.Diagnostics;
using Timer = System.Timers.Timer;
using System.Collections.Concurrent;
using Serilog;
using BackendScripting.Model;

namespace BackendScripting.Scripting;

/// <summary>
/// Assembly factory
/// </summary>
/// <exclude />
public static class AssemblyFactory
{

    #region Local Types

    /// <summary>
    /// Key fo the assembly cache by the clr type and the script hash code.
    /// </summary>
    private sealed class AssemblyKey : Tuple<Type, int>
    {
        internal AssemblyKey(Type type, int scriptHash) :
            base(type, scriptHash)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (scriptHash == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scriptHash));
            }
        }
    }

    /// <summary>
    /// The assembly including his loader to unload the assembly.
    /// Use the binary hash code, to detect changes binary changes.
    /// An audit object has the same id as the tracking object, but may have a different binary.
    /// </summary>
    private sealed class AssemblyRuntime
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once MemberCanBePrivate.Local
        private CollectibleAssemblyLoadContext LoadContext { get; }
        internal Assembly Assembly { get; }
        internal DateTime LastUsed { get; set; } = DateTime.Now;

        internal AssemblyRuntime(CollectibleAssemblyLoadContext context, Assembly assembly)
        {
            LoadContext = context;
            Assembly = assembly;
        }
    }

    #endregion

    // thread-safe assembly cache
    private static readonly ConcurrentDictionary<AssemblyKey, AssemblyRuntime> Assemblies =
        new();

    private static Timer? UpdateTimer { get; set; }
    private static TimeSpan? CacheTimeout { get; set; }
    private static bool CacheEnabled => CacheTimeout.HasValue &&
                                        CacheTimeout.Value != TimeSpan.Zero;

    public static void SetCacheTimeout(TimeSpan timeout)
    {
        if (timeout == TimeSpan.Zero)
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Stop();
                UpdateTimer = null;
            }
            return;
        }

        CacheTimeout = timeout;
        UpdateTimer = new Timer(CacheTimeout.Value.TotalMilliseconds);
        UpdateTimer.Elapsed += delegate { CacheUpdate(); };
        UpdateTimer.Start();
    }

    /// <summary>
    /// Get object script assembly
    /// </summary>
    /// <param name="type">The object type</param>
    /// <param name="scriptObject">The scripting object</param>
    /// <returns>The assembly</returns>
    public static Assembly GetObjectAssembly(Type type, IScriptObject scriptObject)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var key = new AssemblyKey(type, scriptObject.ScriptHash);
        Assembly? assembly = null;
        // cached assembly
        if (CacheEnabled)
        {
            var cached = Assemblies.TryGetValue(key, out var assemblyRuntime);
            if (assemblyRuntime != null && cached)
            {
                assembly = assemblyRuntime.Assembly;
                // update lst usage, used to clean up outdated assemblies
                assemblyRuntime.LastUsed = DateTime.Now;
            }
        }
        if (assembly != null)
        {
            return assembly;
        }

        // binary
        var binary = scriptObject.Binary;
        if (binary == null)
        {
            throw new ArgumentOutOfRangeException(nameof(scriptObject),
                $"Script object without binary {type.Namespace} with id {scriptObject.Id}.");
        }

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        // load assembly from binary
        using var loadContext = new CollectibleAssemblyLoadContext();
        assembly = loadContext.LoadFromBinary(binary);

        stopWatch.Stop();
        Log.Information($"Loaded assembly {assembly.GetName().Name} [{stopWatch.ElapsedMilliseconds} ms]");

        // if add fails, we will try the next time to add
        if (CacheEnabled)
        {
            Assemblies.TryAdd(key, new(loadContext, assembly));
        }
        return assembly;
    }

    /// <summary>
    /// Invalidate object script assembly on script changes.
    /// </summary>
    /// <remarks>This should be called from repository update methods.</remarks>
    /// <param name="type">The object type</param>
    /// <param name="scriptObject">The scripting object</param>
    public static void InvalidateObjectAssembly(Type type, IScriptObject scriptObject)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }
        if (!CacheEnabled)
        {
            return;
        }

        var key = new AssemblyKey(type, scriptObject.ScriptHash);
        if (!Assemblies.ContainsKey(key))
        {
            return;
        }

        Assemblies.Remove(key, out _);
    }

    /// <summary>
    /// Clears the assembly cache
    /// </summary>
    public static void CacheClear()
    {
        if (!Assemblies.Any())
        {
            Log.Information("Empty assembly cache");
            return;
        }

        var count = Assemblies.Count;
        Assemblies.Clear();
        Log.Information($"Assembly cache successfully cleared ({count} assemblies)");
    }

    private static void CacheUpdate()
    {
        if (!CacheTimeout.HasValue || !Assemblies.Any())
        {
            return;
        }

        var outdatedDate = DateTime.Now.Subtract(CacheTimeout.Value);
        var assemblies = Assemblies.Where(x => x.Value.LastUsed < outdatedDate).ToList();
        if (assemblies.Any())
        {
            var removed = 0;
            foreach (var assembly in assemblies)
            {
                try
                {
                    if (Assemblies.TryRemove(assembly.Key, out _))
                    {
                        removed++;
                        Log.Debug($"Removed outdated assembly {assembly.Value.Assembly.GetName().Name}");
                    }
                }
                catch (Exception exception)
                {
                    Log.Error($"Error removing assembly: {exception.GetBaseException().Message}", exception);
                }
            }
            Log.Information($"Removed {removed} assemblies, outdated since {outdatedDate}");
        }
    }

}