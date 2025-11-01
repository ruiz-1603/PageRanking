using System.Collections.Generic;
using System.Linq;

namespace WebCrawler
{
    // Clase que representa el grafo dirigido de páginas web
    public class Grafo
    {
        // Diccionario donde cada URL tiene su lista de enlaces salientes
        private Dictionary<string, List<string>> adyacencias;
        
        public Grafo()
        {
            adyacencias = new Dictionary<string, List<string>>();
        }
        
        // Agregar un nodo al grafo
        public void addNodo(string url)
        {
            if (!adyacencias.ContainsKey(url))
            {
                adyacencias[url] = new List<string>();
            }
        }
        
        // Agregar una arista dirigida de origen a destino
        public void addArista(string origen, string destino)
        {
            // Asegurar que ambos nodos existen
            addNodo(origen);
            addNodo(destino);
            
            // Agregar la arista si no existe
            if (!adyacencias[origen].Contains(destino))
            {
                adyacencias[origen].Add(destino);
            }
        }
        
        // Obtener todas las URLs del grafo
        public List<string> getNodos()
        {
            return adyacencias.Keys.ToList();
        }
        
        // Obtener los enlaces salientes de una URL
        public List<string> getEnlacesSalientes(string url)
        {
            if (adyacencias.ContainsKey(url))
            {
                return adyacencias[url];
            }
            return new List<string>();
        }
        
        // Obtener URLs que enlazan a una URL específica (enlaces entrantes)
        public List<string> getEnlacesEntrantes(string url)
        {
            List<string> entrantes = new List<string>();
            
            foreach (var nodo in adyacencias.Keys)
            {
                if (adyacencias[nodo].Contains(url))
                {
                    entrantes.Add(nodo);
                }
            }
            
            return entrantes;
        }
        
        // Obtener cantidad total de nodos
        public int CantidadNodos()
        {
            return adyacencias.Count;
        }
        
        // Verificar si existe una arista
        public bool existeArista(string origen, string destino)
        {
            if (adyacencias.ContainsKey(origen))
            {
                return adyacencias[origen].Contains(destino);
            }
            return false;
        }
    }
}
