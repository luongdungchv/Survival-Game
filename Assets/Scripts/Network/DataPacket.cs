using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;

public class DataPacket<T>
{
    public string cmd;
    public T arg1;
    public DataPacket(string msg)
    {
        string[] split = msg.Split(' ');
        cmd = split[0];

    }
}
