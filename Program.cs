using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebCrawler
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            
    
            var crawler = new WebCrawler();
            
      
            var urls = new List<string>
            {
                "https://www.una.ac.cr/",
            };
            
            // Carpeta donde se guardarán los resultados
            string rutaSalida = "resultados_crawler";
            
            // Mostrar configuración
            Console.WriteLine($"  - Carpeta de salida: {rutaSalida}");
            Console.WriteLine($"  - Factor de amortiguación: 0.85");
            Console.WriteLine($"  - Umbral de convergencia: 0.0001\n");
            
            Console.WriteLine("URLs de inicio:");
            foreach (var url in urls)
            {
                Console.WriteLine($"  - {url}");
            }
            Console.WriteLine("\n" + new string('-', 50));
            Console.WriteLine("Iniciando crawling...\n");
            
            // Ejecutar el crawler
            await crawler.StartCrawlAsync(rutaSalida, urls);
            
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("    PROCESO COMPLETADO EXITOSAMENTE");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"\nArchivos generados en: ./{rutaSalida}/");
            Console.WriteLine("  1. matriz_adyacencia.txt");
            Console.WriteLine("  2. pagerank_resultados.txt");
            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
