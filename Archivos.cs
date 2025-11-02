using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebCrawler
{
    public class Archivos
    {
        private string carpetaSalida;
        
        public Archivos(string carpeta)
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
        
                // Escribir matriz de adyacencia
                for (int i = 0; i < urls.Count; i++)
                {
                    for (int j = 0; j < urls.Count; j++)
                    {
                        sw.Write(grafo.existeArista(urls[i], urls[j]) ? "1 " : "0 ");
                    }
                    sw.WriteLine();
                }
            }

            Console.WriteLine($"\nMatriz de adyacencia guardada en: {archivo}");
        }
      
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

                sw.WriteLine("\n=== Estadísticas ===");
                sw.WriteLine($"PageRank máximo: {ordenado[0].Value:F8}");
                sw.WriteLine($"PageRank mínimo: {ordenado[ordenado.Count - 1].Value:F8}");
            }
            
            Console.WriteLine($"Resultados de PageRank guardados en: {archivo}");

            Console.WriteLine("\n=== Top 10 páginas más importantes ===");
            for (int i = 0; i < Math.Min(10, ordenado.Count); i++)
            {
                Console.WriteLine($"#{i + 1}: {ordenado[i].Value:F6} - {ordenado[i].Key}");
            }
        }
    }
}