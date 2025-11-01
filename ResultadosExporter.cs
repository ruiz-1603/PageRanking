using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebCrawler
{
    // Clase encargada de exportar los resultados a archivos
    public class ResultadosExporter
    {
        private string carpetaSalida;
        
        public ResultadosExporter(string carpeta)
        {
            this.carpetaSalida = carpeta;
        }
        
        // Exportar matriz de adyacencia
        public void ExportarMatrizAdyacencia(Grafo grafo)
        {
            string archivo = Path.Combine(carpetaSalida, "matriz_adyacencia.txt");
            List<string> urls = grafo.getNodos();
            
            using (StreamWriter sw = new StreamWriter(archivo))
            {
                sw.WriteLine("Matriz de Adyacencia del Grafo");
                sw.WriteLine("================================");
                sw.WriteLine($"Total de nodos: {urls.Count}");
                sw.WriteLine();
                
                // Índice de URLs
                sw.WriteLine("Índice de URLs:");
                for (int i = 0; i < urls.Count; i++)
                {
                    sw.WriteLine($"[{i}] {urls[i]}");
                }
                sw.WriteLine();
                
                // Matriz en formato binario
                sw.WriteLine("Matriz (1 = hay enlace, 0 = no hay enlace):");
                sw.WriteLine();
                
                for (int i = 0; i < urls.Count; i++)
                {
                    for (int j = 0; j < urls.Count; j++)
                    {
                        if (grafo.existeArista(urls[i], urls[j]))
                        {
                            sw.Write("1 ");
                        }
                        else
                        {
                            sw.Write("0 ");
                        }
                    }
                    sw.WriteLine();
                }
                
                // Lista de adyacencia
                sw.WriteLine();
                sw.WriteLine("Lista de adyacencia:");
                foreach (string url in urls)
                {
                    List<string> enlaces = grafo.getEnlacesSalientes(url);
                    sw.WriteLine($"\n{url}");
                    sw.WriteLine($"  Enlaces salientes: {enlaces.Count}");
                    foreach (string destino in enlaces)
                    {
                        sw.WriteLine($"    -> {destino}");
                    }
                }
            }
            
            Console.WriteLine($"\nMatriz de adyacencia guardada en: {archivo}");
        }
        
        // Exportar resultados de PageRank
        public void ExportarResultadosPageRank(Dictionary<string, double> pagerank)
        {
            string archivo = Path.Combine(carpetaSalida, "pagerank_resultados.txt");
            
            // Ordenar por PageRank descendente
            var ordenado = pagerank.OrderByDescending(x => x.Value).ToList();
            
            using (StreamWriter sw = new StreamWriter(archivo))
            {
                sw.WriteLine("Resultados de PageRank");
                sw.WriteLine("======================");
                sw.WriteLine($"Total de páginas: {pagerank.Count}");
                sw.WriteLine($"Factor de amortiguación (d): 0.85");
                sw.WriteLine();
                
                sw.WriteLine("Ranking de páginas por importancia:");
                sw.WriteLine();
                
                int ranking = 1;
                foreach (var item in ordenado)
                {
                    sw.WriteLine($"#{ranking}");
                    sw.WriteLine($"URL: {item.Key}");
                    sw.WriteLine($"PageRank: {item.Value:F8}");
                    sw.WriteLine();
                    ranking++;
                }
                
                // Estadísticas
                sw.WriteLine("\n=== Estadísticas ===");
                sw.WriteLine($"PageRank máximo: {ordenado[0].Value:F8}");
                sw.WriteLine($"PageRank mínimo: {ordenado[ordenado.Count - 1].Value:F8}");
                double promedio = pagerank.Values.Average();
                sw.WriteLine($"PageRank promedio: {promedio:F8}");
            }
            
            Console.WriteLine($"Resultados de PageRank guardados en: {archivo}");
            
            // Mostrar top 10 en consola
            Console.WriteLine("\n=== Top 10 páginas más importantes ===");
            for (int i = 0; i < Math.Min(10, ordenado.Count); i++)
            {
                Console.WriteLine($"#{i + 1}: {ordenado[i].Value:F6} - {ordenado[i].Key}");
            }
        }
    }
}