using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ServiceStack.Host;
using System;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Quasar.Controllers
{
    //[ApiController]
    public class SateliteController : Controller
    {
        private void SetCookie(string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMinutes(60);

            Response.Cookies.Append(key, value, option);
        }

        private string GetCookie(string key)
        {
            return Request.Cookies[key];
        }

        private Output Decodificar(Comunicacion datos, bool save)
        {
            SateliteServices aux = new();
            //Guardar los datos para recuperarlos en el GET
            if (save) {
                //aux.SaveMessage(datos);
                string json = JsonConvert.SerializeObject(datos);
                SetCookie("QuasarSatelites", json, null);
            }

            (float x, float y) posicion = (0, 0);
            string[] mensaje = { "" };

            if (datos.satellites.Length > 2) // Sólo obtendré la posición si tengon al menos 3 distancias.
            {
                posicion = aux.GetLocation(datos.satellites[0].distance, datos.satellites[1].distance, datos.satellites[2].distance);
                mensaje = aux.GuardarMensajes(datos.satellites[0].message, datos.satellites[1].message, datos.satellites[2].message);
            }

            // La propiedad tipo "Position" no puede ser asignada directamente en el objeto "Output" haciendo:
            // respuesta.posicion.x = 999
            // porque arroja un error NullReferenceException
            // Entonces se asigna primero a un objeto tipo "Position" y luego se asigna a la propiedad de ese tipo en el objeto "Output".
            Position xy = new();
            xy.x = posicion.x;
            xy.y = posicion.y;
            Output respuesta = new(xy, aux.GetMessage(mensaje));

            return respuesta;
        }

        private Output Decodificar(List<Satellite> datos)
        {
            SateliteServices aux = new();

            (float x, float y) posicion = (0, 0);
            string[] mensaje = { "" };

            if (datos.Count > 2) // Sólo obtendré la posición si tengon al menos 3 distancias.
            {
                posicion = aux.GetLocation(datos[0].distance, datos[1].distance, datos[2].distance);
                mensaje = aux.GuardarMensajes(datos[0].message, datos[1].message, datos[2].message);
            }

            // La propiedad tipo "Position" no puede ser asignada directamente en el objeto "Output" haciendo:
            // respuesta.posicion.x = 999
            // porque arroja un error NullReferenceException
            // Entonces se asigna primero a un objeto tipo "Position" y luego se asigna a la propiedad de ese tipo en el objeto "Output".
            Position xy = new();
            xy.x = posicion.x;
            xy.y = posicion.y;
            Output respuesta = new(xy, aux.GetMessage(mensaje));

            return respuesta;
        }

        [Route("topsecret")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Post([FromBody] Comunicacion datos)
        {
            Output respuesta = Decodificar(datos, true);
            if (respuesta.message == "" || (respuesta.position.x == 0 && respuesta.position.y == 0))
            {
                return NotFound("RESPONSE CODE: 404");
            } else
            {
                return CreatedAtAction(nameof(Get), respuesta);
            }
        }

        [ApiController]
        public class ErrorController : ControllerBase
        {
            [Route("/error")]
            public IActionResult Error() => Problem();
        }

        [Route("topsecret_split/{name}")]
        [HttpPost]
        public ActionResult Post(Satellite sat)
        {
            string json = JsonConvert.SerializeObject(sat);
            //json = json.Replace("\\", "");
            json = json.Replace("\"", "'");
            SetCookie(sat.name, json, null);

            //TODO?
            return Ok(200);
            //return NotFound("RESPONSE CODE: 404");
        }

        [Route("topsecret_split")]
        [HttpGet]
        public ActionResult Get()
        {
            ActionResult respuesta = null;

            //La lista de satélites me da los nombres de los que deberían existir.
            SateliteServices Satserv = new();
            List<SateliteReceptor> Satelites = Satserv.GetSatelites();

            //Como no tengo información del mensaje enviado, debo leer lo que dejó la acción POST
            //Aquí veo cuántas recepciones de mensajes tuve. Deberían coincidir con la lista de satélites anterior.

            List<Satellite> Sats = new();
            for (int i = 0; i < Satelites.Count; i++)
            {
                string aux = GetCookie(Satelites[i].nombre); //Lee cada cookie

                if (aux != null) // Si existe la cookie o no expiró
                {
                    JObject sats = JObject.Parse(aux);

                    Satellite Sat = new();
                    Sat.name = sats["name"].ToString();  //sats["satellites"][i]["name"].ToString();
                    Sat.distance = (float)sats["distance"]; //(float)sats["satellites"][i]["distance"];

                    string[] messages = new string[sats["message"].Count()]; //string[sats["satellites"][i]["message"].Count()];

                    int j = 0;
                    foreach (string msg in sats["message"]) // sats["satellites"][i]["message"])
                    {
                        //TODO: CAMBIAR ACA. DEBO AGREGAR STRING, ACÁ QUEDA UN STRING[1] y debe quedar un string[n]
                        messages[j] = msg.ToString();
                        j++;
                    }
                    Sat.message = messages;
                    Sats.Add(Sat);
                }
            }
            respuesta = Ok(Decodificar(Sats));

            if (respuesta == null)
            {
                respuesta = NotFound("RESPONSE CODE: 404");
            }
            return respuesta;
        }

    }
}
