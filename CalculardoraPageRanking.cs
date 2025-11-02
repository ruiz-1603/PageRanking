using System;
using System.Collections.Generic;
using System.Linq;

namespace WebCrawler
{
    public class CalculardoraPageRanking
    {
        private Grafo grafo;
        private double factorAmortiguacion;
        private double umbralConvergencia;
        private int maxIteraciones;
        
        public CalculardoraPageRanking(Grafo grafo, double d = 0.85, double umbral = 0.0001, int maxIter = 100)
        {
            this.grafo = grafo;
            this.factorAmortiguacion = d;
            this.umbralConvergencia = umbral;
            this.maxIteraciones = maxIter;
        }
        public Dictionary<string, double> Calcular()
        {
            List<string> urls = grafo.getNodos();
            int n = urls.Count;
            
            if (n == 0)
            {
                Console.WriteLine("No hay páginas para calcular PageRank");
                return new Dictionary<string, double>();
            }
            
            Console.WriteLine($"\nCalculando PageRank para {n} páginas...");
            
            Dictionary<string, double> pagerank = new Dictionary<string, double>();
            foreach (string url in urls)
            {
                pagerank[url] = 1.0 / n;
            }
            
            int iteracion = 0;
            bool convergio = false;
            
            while (iteracion < maxIteraciones && !convergio)
            {
                Dictionary<string, double> nuevoPagerank = CalcularIteracion(pagerank, urls, n);
                
                double diferencia = CalcularDiferencia(pagerank, nuevoPagerank, n);
                
                Console.WriteLine($"Iteración {iteracion + 1}: diferencia promedio = {diferencia:F6}");
                
                if (diferencia < umbralConvergencia)
                {
                    convergio = true;
                    Console.WriteLine($"Convergencia alcanzada en {iteracion + 1} iteraciones");
                }
                pagerank = nuevoPagerank;
                iteracion++;
            }
            
            if (!convergio)
            {
                Console.WriteLine($"Se alcanzó el límite de {maxIteraciones} iteraciones sin convergencia completa");
            }
            return pagerank;
        }
        
        private Dictionary<string, double> CalcularIteracion(Dictionary<string, double> pagerankActual, List<string> urls, int n)
        {
            Dictionary<string, double> nuevoPagerank = new Dictionary<string, double>();
            
            foreach (string url in urls)
            {
                double suma = 0.0;
                
                // Obtener todas las páginas que enlazan a esta
                List<string> entrantes = grafo.getEnlacesEntrantes(url);
                
                foreach (string urlEntrante in entrantes)
                {
                    int cantidadEnlaces = grafo.getEnlacesSalientes(urlEntrante).Count;
                    if (cantidadEnlaces > 0)
                    {
                        suma += pagerankActual[urlEntrante] / cantidadEnlaces;
                    }
                }
                
                //// la Formula
                nuevoPagerank[url] = (1 - factorAmortiguacion) / n + factorAmortiguacion * suma;
            }
            
            return nuevoPagerank;
        }
        
        private double CalcularDiferencia(Dictionary<string, double> anterior, Dictionary<string, double> nuevo, int n)
        {
            double diferencia = 0.0;
            
            foreach (string url in anterior.Keys)
            {
                diferencia += Math.Abs(nuevo[url] - anterior[url]);
            }
            
            return diferencia / n;
        }
    }
}