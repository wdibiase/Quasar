using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Quasar.Controllers;

namespace Quasar
{
    public class SateliteServices
    {
        public List<SateliteReceptor> GetSatelites()
        {
            // Una forma para guardar las coordenadas de los satélites existentes es en un archivo json.
            // También podría ser accediendo a una base de datos, consultadas a otro webservice, etc.
            string jsonString = File.ReadAllText("datosSatelites.json");
            Input jsonSatelite = JsonSerializer.Deserialize<Input>(jsonString);

            List<SateliteReceptor> SatelitesEnServicio = new();
            for (int i = 0; i < jsonSatelite.satelites.Length; i += 1)
            {
                SateliteReceptor aux = new(jsonSatelite.satelites[i].nombre, (jsonSatelite.satelites[i].posicion.x, jsonSatelite.satelites[i].posicion.y));
                SatelitesEnServicio.Add(aux);
            }
            return SatelitesEnServicio;
        }

        public Comunicacion ReadMessage()
        {
            string jsonString = File.ReadAllText("mensaje.json");
            Comunicacion mensaje = JsonSerializer.Deserialize<Comunicacion>(jsonString);
            return mensaje;
        }

        public void SaveMessage(Comunicacion comunicacion)
        {
            string json = JsonConvert.SerializeObject(comunicacion);
            System.IO.File.WriteAllText("mensaje.json", json);
        }

        public void SaveMessage(Satellite comunicacion)
        {
            string json = JsonConvert.SerializeObject(comunicacion);
            System.IO.File.WriteAllText("mensaje" + comunicacion.name + ".json", json);
        }

        public (float x, float y) GetLocation(params float[] Distance)
        {
            // La trilateración es un método matemático para determinar las posiciones relativas de
            // objetos usando la geometría de los triángulos y de las esferas. 
            // Las ecuaciones se formulan teniendo en cuenta que una de las circunferencias se encuentra en el origen (0, 0).
            // La segunda tiene centro en el eje x, supongamos (d, 0), y la restante en (i, j), un offset desde las anteriores.
            // Para más datos ver: https://es.wikipedia.org/wiki/Trilateraci%C3%B3n

            // Para comenzar necesito saber adónde están los satéites operativos:
            List<SateliteReceptor> Satelites = GetSatelites();
            float X = 0, Y = 0, OffsetX = 0, OffsetY = 0;

            if (Distance.Length == Satelites.Count && Satelites.Count > 2) //Para la trilateración deben ser al menos 3 puntos.
            {
                //Si fuesen más de 3 satélites habría que además crear un algotirmo para que tome los 3 más cercanos.
                if (Satelites.Count > 3)
                {
                    // Tomar sólo las 3 menores distancias
                }

                // No es una situación realista, pero sabiendo el offset original se puede volver a las coordenadas originales.
                OffsetX = Satelites[0].posicion.x;
                OffsetY = Satelites[0].posicion.y;

                for (int i = 0; i < Distance.Length; i += 1)
                {
                    // Trasladar los satélites a la posición ficticia. El primer satélite terminará en la posición (0,0)
                    Satelites[i].posicion = (Satelites[i].posicion.x - OffsetX, Satelites[i].posicion.y - OffsetY);
                }

                X = (float)((Math.Pow(Distance[0], 2) - Math.Pow(Distance[1], 2) + Math.Pow(Satelites[1].posicion.x, 2)) / (2 * Satelites[1].posicion.x));
                float t1 = (float)((Math.Pow(Distance[0], 2) - Math.Pow(Distance[2], 2) + Math.Pow(Satelites[2].posicion.x, 2) + Math.Pow(Satelites[2].posicion.y, 2)) / (2 * Satelites[2].posicion.y));
                float t2 = (float)(Satelites[2].posicion.x / Satelites[2].posicion.y) * X;
                Y = t1 - t2;
            }
            return (X + OffsetX, Y + OffsetY); //Se retorna la posición volviendo al sistema de coordenadas sin offset.
        }

        public string[] GuardarMensajes(string[] mensaje1, string[] mensaje2, string[] mensaje3)
        {
            string[] salida = mensaje1;
            //Se supone que todos los mensajes tienen la misma cantidad de bloques
            if (mensaje1.Length == mensaje2.Length && mensaje2.Length == mensaje3.Length)
            {
                for (int i=0; i<mensaje2.Length; i++) //Arranco directamente en el mensaje 2
                {
                    if (mensaje2[i] != "")
                    {
                        salida[i] = mensaje2[i];
                    }
                }
                for (int i = 0; i < mensaje3.Length; i++)
                {
                    if (mensaje3[i] != "")
                    {
                        salida[i] = mensaje3[i];
                    }
                }
            }
            return salida;
        }

        public string GetMessage(params string[] message)
        {
            string mensaje = "";
            for (int i = 0; i < message.Length; i++)
            {
                mensaje = mensaje + " " + message[i];
            }
            return mensaje.Trim();
        }

    }
}
