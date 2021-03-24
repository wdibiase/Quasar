namespace Quasar
{
    public class SateliteReceptor
    {
        public string nombre { get; set; }
        public (float x, float y) posicion { get; set; }
        public string[] MensajeRecibido { get; set; }
            
        //Constructor
        public SateliteReceptor(string name, (float x, float y) posicion)
        {
            this.nombre = name;
            this.posicion = (posicion.x, posicion.y);
        }
    }

    public class Posicion
    {
        public float x { get; set; }
        public float y { get; set; }
    }
}
