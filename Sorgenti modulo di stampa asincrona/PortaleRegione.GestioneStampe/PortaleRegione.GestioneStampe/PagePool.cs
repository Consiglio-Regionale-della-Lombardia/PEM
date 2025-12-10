using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PortaleRegione.GestioneStampe;

public class PagePool
{
    private readonly IBrowser _browser;
    private readonly ConcurrentQueue<IPage> _availablePages = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly int _poolSize;

    public PagePool(IBrowser browser, int poolSize = 5)
    {
        _browser = browser;
        _poolSize = poolSize;
        _semaphore = new SemaphoreSlim(poolSize, poolSize);
    }

    public async Task<IPage> AcquirePageAsync()
    {
        await _semaphore.WaitAsync();
        
        if (_availablePages.TryDequeue(out var page))
        {
            return page;
        }
        
        // Crea nuova page se il pool non è ancora pieno
        return await _browser.NewPageAsync();
    }

    public async Task ReleasePageAsync(IPage page)
    {
        // Pulisci la page prima di rimetterla nel pool
        await page.EvaluateAsync("() => { document.body.innerHTML = ''; }");
        
        _availablePages.Enqueue(page);
        _semaphore.Release();
    }
}