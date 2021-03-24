using System;
using System.Collections.Generic;

public class Input
{
    public SateliteReceptor[] satelites { get; set; }
}

public class SateliteReceptor
{
    public string nombre { get; set; }
    public Posicion posicion { get; set; }
}

public class Posicion
{
    public float x { get; set; }
    public float y { get; set; }
}

public class Output
{
    public Position position { get ; set; }
    public string message { get; set; }

    public Output (Position _position, string _message)
    {
        this.position = _position;
        this.message = _message;
    }
}


public class Position
{
    public float x { get; set; }
    public float y { get; set; }
}

public class Comunicacion
{
    public Satellite[] satellites { get; set; }

    //public static implicit operator Comunicacion(List<Satellite> v)
    //{
    //    Comunicacion mensaje = new();
    //    int i = 0;
    //    foreach (Satellite s in v)
    //    {
    //        mensaje.satellites[i].name = s.name;
    //        mensaje.satellites[i].distance = s.distance;
    //        mensaje.satellites[i].message = s.message;
    //    }
    //    return mensaje;
    //}
}

public class Satellite
{
    public string name { get; set; }
    public float distance { get; set; }
    public string[] message { get; set; }
}
