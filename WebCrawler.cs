using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class WebCrawler
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly HashSet<string> _visited = new HashSet<string>();
        private readonly int _maxDepth = 4;
        private string _outputFolder;
        private readonly HashSet<string> _stopWords = new HashSet<string> 
        { 
            "el", "la", "de", "y", "que", "a", "en", "un", "una" 
        };
        
        // Grafo para almacenar las relaciones entre páginas
        private Grafo grafo;
        
        public WebCrawler()
        {
            grafo = new Grafo();
        }
        
        public async Task StartCrawlAsync(string ruta, List<string> rootUrls, string patron = "")
        {
            Directory.CreateDirectory(_outputFolder = ruta);

            foreach (var url in rootUrls)
            {
                await CrawlAsync(url, 0, patron);
            }
            
            // Cuando termina el crawling, calculamos PageRank
            Console.WriteLine("\n=== Crawling completado ===");
            Console.WriteLine($"Total de páginas visitadas: {_visited.Count}");
            
            // Calcular PageRank usando la clase especializada
            PageRankCalculator calculador = new PageRankCalculator(grafo);
            Dictionary<string, double> resultados = calculador.Calcular();
            
            // Exportar resultados usando la clase especializada
            ResultadosExporter exporter = new ResultadosExporter(_outputFolder);
            exporter.ExportarMatrizAdyacencia(grafo);
            exporter.ExportarResultadosPageRank(resultados);
        }

        private async Task CrawlAsync(string url, int depth, string patron)
        {
            try
            {
                if (depth > _maxDepth || _visited.Contains(url) || !url.StartsWith("http"))
                {
                    return;
                }
                if (string.IsNullOrEmpty(patron) || url.Contains(patron))
                {
                    Console.WriteLine($"[{depth}] Visitando: {url}");
                    _visited.Add(url);
                    
                    // Agregar nodo al grafo
                    grafo.addNodo(url);
                    
                    string html = await _httpClient.GetStringAsync(url);

                    // Guardar texto normalizado
                    string safeName = ToSafeFilename(url);
                    string plainText = ExtractTextFromHtml(html);
                    string normalizedText = NormalizeText(plainText);
                    File.WriteAllText(Path.Combine(_outputFolder, $"{safeName}.txt"), normalizedText);

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
                            // Agregar arista al grafo
                            grafo.addArista(url, fullUrl);
                            
                            await CrawlAsync(fullUrl, depth + 1, patron);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en {url}: {ex.Message}");
            }
        }

        private string ExtractTextFromHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return WebUtility.HtmlDecode(doc.DocumentNode.InnerText);
        }

        private string NormalizeText(string text)
        {
            string lower = text.ToLowerInvariant();
            string clean = Regex.Replace(lower, @"[^\p{L}\p{Nd}]+", " ");
            string noStop = string.Join(" ", clean
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !_stopWords.Contains(w)));
            return noStop;
        }

        private string ToSafeFilename(string url)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url));
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