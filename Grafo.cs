using System.Collections.Generic;
using System.Linq;

namespace WebCrawler
{
    public class Grafo
    {
        private Dictionary<string, List<string>> adyacencias;
        
        public Grafo()
        {
            adyacencias = new Dictionary<string, List<string>>();
        }
        
        public void addNodo(string url)
        {
            if (!adyacencias.ContainsKey(url))
            {
                adyacencias[url] = new List<string>();
            }
        }
        
        public void addArista(string origen, string destino)
        {
            addNodo(origen);
            addNodo(destino);
            
            if (!adyacencias[origen].Contains(destino))
            {
                adyacencias[origen].Add(destino);
            }
        }
        
        public List<string> getNodos()
        {
            return adyacencias.Keys.ToList();
        }
        
        public List<string> getEnlacesSalientes(string url)
        {
            if (adyacencias.ContainsKey(url))
            {
                return adyacencias[url];
            }
            return new List<string>();
        }
        
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
        
        public int CantidadNodos()
        {
            return adyacencias.Count;
        }
        
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
