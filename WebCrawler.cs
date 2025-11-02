using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class WebCrawler
    {
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, bool> _visited = new ConcurrentDictionary<string, bool>();
        private readonly ConcurrentBag<(string origen, string destino)> _aristas = new ConcurrentBag<(string, string)>();
        private readonly int paginasMax = 1000;
        private readonly int paginasXtarea = 100;
        private string _outputFolder;
        private int visitadas = 0;
        
        // Grafo para almacenar las relaciones entre páginas
        private Grafo grafo;
        
        public WebCrawler()
        {
            grafo = new Grafo();
            _httpClient = new HttpClient(new HttpClientHandler
            {
                MaxConnectionsPerServer = 50,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }
        
        public async Task StartCrawlAsync(string ruta, List<string> rootUrls, string patron = "")
        {
            Directory.CreateDirectory(_outputFolder = ruta);

            // Recolectar todas las URLs usando BFS paralelo
            var cola = new ConcurrentQueue<(string url, int depth)>();
            foreach (var url in rootUrls)
            {
                cola.Enqueue((url, 0));
            }
            
            Console.WriteLine("Iniciando crawling paralelo...\n");
            
            while (!cola.IsEmpty && visitadas < paginasMax)
            {
                var tareasPorLote = new List<Task>();
                var urlsEnProceso = new List<(string url, int depth)>();
                
                // Extraer un lote de URLs
                for (int i = 0; i < paginasXtarea && !cola.IsEmpty; i++)
                {
                    if (cola.TryDequeue(out var item))
                    {
                        urlsEnProceso.Add(item);
                    }
                }
                
                // Procesar el lote en paralelo
                foreach (var item in urlsEnProceso)
                {
                    if (visitadas >= paginasMax) break;
                    
                    tareasPorLote.Add(Task.Run(async () =>
                    {
                        var nuevasUrls = await ProcesarUrlAsync(item.url, item.depth, patron);
                        foreach (var nuevaUrl in nuevasUrls)
                        {
                            if (visitadas < paginasMax)
                            {
                                cola.Enqueue((nuevaUrl, item.depth + 1));
                            }
                        }
                    }));
                }
                
                await Task.WhenAll(tareasPorLote);
            }
            
            // Construir el grafo con las aristas recolectadas
            Console.WriteLine("\n=== Construyendo grafo ===");
            foreach (var url in _visited.Keys)
            {
                grafo.addNodo(url);
            }
            
            foreach (var (origen, destino) in _aristas)
            {
                if (_visited.ContainsKey(origen) && _visited.ContainsKey(destino))
                {
                    grafo.addArista(origen, destino);
                }
            }
            
            // Cuando termina el crawling, calculamos PageRank
            Console.WriteLine("\n=== Crawling completado ===");
            Console.WriteLine($"Total de páginas visitadas: {visitadas}");
            Console.WriteLine($"Total de páginas en el grafo: {grafo.CantidadNodos()}");
            
            // Calcular PageRank usando la clase especializada
            PageRankCalculator calculador = new PageRankCalculator(grafo);
            Dictionary<string, double> resultados = calculador.Calcular();
            
            // Exportar resultados usando la clase especializada
            ResultadosExporter exporter = new ResultadosExporter(_outputFolder);
            exporter.ExportarMatrizAdyacencia(grafo);
            exporter.ExportarResultadosPageRank(resultados);
        }

        private async Task<List<string>> ProcesarUrlAsync(string url, int depth, string patron)
        {
            var nuevasUrls = new List<string>();
            
            try
            {
                // Validaciones rápidas
                if (depth > 20 || !url.StartsWith("http"))
                {
                    return nuevasUrls;
                }
                
                // Verificar patrón
                if (!string.IsNullOrEmpty(patron) && !url.Contains(patron))
                {
                    return nuevasUrls;
                }
                
                // Verificar si ya fue visitada
                if (!_visited.TryAdd(url, true))
                {
                    return nuevasUrls;
                }
                
                // Verificar límite
                int contador = Interlocked.Increment(ref visitadas);
                if (contador > paginasMax)
                {
                    return nuevasUrls;
                }
                
                Console.WriteLine($"[{depth}] ({contador}/{paginasMax}) {url}");
                
                string html = await _httpClient.GetStringAsync(url);

                // Parsear enlaces
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var links = doc.DocumentNode
                    .SelectNodes("//a[@href]")
                    ?.Select(a => a.GetAttributeValue("href", null))
                    ?? new List<string>();

                foreach (var link in links)
                {
                    string fullUrl = ResolveUrl(url, link);
                    
                    if (!string.IsNullOrEmpty(fullUrl) && fullUrl.StartsWith("http"))
                    {
                        // Guardar arista para construir el grafo después
                        _aristas.Add((url, fullUrl));
                        
                        // Agregar a la lista de nuevas URLs si no ha sido visitada
                        if (!_visited.ContainsKey(fullUrl) && visitadas < paginasMax)
                        {
                            nuevasUrls.Add(fullUrl);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Timeout - ignorar
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error HTTP en {url}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en {url}: {ex.Message}");
            }
            
            return nuevasUrls;
        }

        private string ResolveUrl(string baseUrl, string href)
        {
            try
            {
                var baseUri = new Uri(baseUrl);
                var fullUri = new Uri(baseUri, href);
                return fullUri.ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}