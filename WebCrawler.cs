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
        
        private string outputFolder;
        
        private int paginasVisitadas = 0;
        
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
            Directory.CreateDirectory(outputFolder = ruta);
            
            var cola = new ConcurrentQueue<(string url, int depth)>();
            
            foreach (var url in rootUrls)
            {
                cola.Enqueue((url, 0));
            }
            
            while (!cola.IsEmpty && paginasVisitadas < paginasMax)
            {
                var tareas = new List<Task>();
                var urlsEnProceso = new List<(string url, int depth)>();

             // saca 1000 para procesar todas al mismo tiumpo
                for (int i = 0; i < 1000 && !cola.IsEmpty; i++)
                {
                    if (cola.TryDequeue(out var item))
                    {
                        urlsEnProceso.Add(item);
                    }
                }
                //  crea una tarea por cada URL a procesar
                foreach (var item in urlsEnProceso)
                {
                    if (paginasVisitadas >= paginasMax) break;

                    tareas.Add(Task.Run(async () =>
                    {
                        var nuevasUrls = await ProcesarUrlAsync(item.url, item.depth, patron);

                        // Las URLs encontradas se agregan a la cola para seguir rastreando
                        foreach (var nuevaUrl in nuevasUrls)
                        {
                            if (paginasVisitadas < paginasMax)
                            {
                                cola.Enqueue((nuevaUrl, item.depth + 1));
                            }
                        }
                    }));
                }
                
                await Task.WhenAll(tareas);
            }
            
            Console.WriteLine("\nConstruyendo grafo ");
            
            foreach (var url in _visited.Keys)
            {
                grafo.addNodo(url);
            }

            // agrega las conexiones entre aristas
            foreach (var (origen, destino) in _aristas)
            {
                if (_visited.ContainsKey(origen) && _visited.ContainsKey(destino))
                {
                    grafo.addArista(origen, destino);
                }
            }

            Console.WriteLine("\nCompletado");
            Console.WriteLine($"Total de paginas visitadas: {paginasVisitadas}");
            Console.WriteLine($"Total de paginas en el grafo: {grafo.CantidadNodos()}");

            // calcula el PageRank de cada pagina 
            CalculardoraPageRanking calculador = new CalculardoraPageRanking(grafo);
            Dictionary<string, double> resultados = calculador.Calcular();

            Archivos exporter = new Archivos(outputFolder);
            exporter.ExportarMatrizAdyacencia(grafo);
            exporter.ExportarResultadosPageRank(resultados);
        }
        
        private async Task<List<string>> ProcesarUrlAsync(string url, int depth, string patron)
        {
            var nuevasUrls = new List<string>();

            // si la profundidad es muy grande se salta
            if (depth > 20 || !url.StartsWith("http"))
            {
                return nuevasUrls;
            }

            // Si se pidió un patrón y la URL no lo contiene, se ignora
            if (!string.IsNullOrEmpty(patron) && !url.Contains(patron))
            {
                return nuevasUrls;
            }

            // Si ya se visitó esta URL, se omite
            if (!_visited.TryAdd(url, true))
            {
                return nuevasUrls;
            }

            int contador = Interlocked.Increment(ref paginasVisitadas);
            if (contador > paginasMax)
            {
                return nuevasUrls;
            }
            Console.WriteLine($"[{depth}] ({contador}/{paginasMax}) {url}");

            // descarga el contenido HTML de la pagina
            string html = await _httpClient.GetStringAsync(url);

            // Analiza el HTML para buscar los enlaces
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var links = doc.DocumentNode
                .SelectNodes("//a[@href]")
                ?.Select(a => a.GetAttributeValue("href", null))
                ?? new List<string>();

            // Por cada enlace se intenta convertir a una URL 
            foreach (var link in links)
            {
                string fullUrl = ResolveUrl(url, link);

                if (!string.IsNullOrEmpty(fullUrl) && fullUrl.StartsWith("http"))
                {
                    _aristas.Add((url, fullUrl));
                    
                    if (!_visited.ContainsKey(fullUrl) && paginasVisitadas < paginasMax)
                    {
                        nuevasUrls.Add(fullUrl);
                    }
                }
            }

            return nuevasUrls;
        }
        
        private string ResolveUrl(string baseUrl, string refrencia)
        {
            try
            {
                var baseUri = new Uri(baseUrl);
                var fullUri = new Uri(baseUri, refrencia);
                string resultado = fullUri.ToString();
                return resultado;
            }
            catch
            {
                return "";
            }
        }
    }
}
