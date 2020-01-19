using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

public class ButtonHandler : MonoBehaviour
{
 
    public void ScanStuff()
    {
  
        Discovery d = new Discovery();
        d.Scan(2000, 4);



    }
}
