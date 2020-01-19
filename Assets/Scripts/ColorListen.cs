using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ColorListen : MonoBehaviour
{
   public Bulb bulb;
    float tempTime = 0;

    SpriteRenderer spriteRenderer;

    float red = 0;
    float green = 0;
    float blue = 0;
    float warmWhite = 0;
    float coldWhite = 0;

    Color color;
    // Start is called before the first frame update
    void Start()
    {
       
        bulb = GetComponent<Bulb>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {



          //bulb.SetColorLevel((byte)(red2 * 255), (byte)(green2 * 255), (byte)(blue2 * 255));


        tempTime += Time.deltaTime;
        if (tempTime > .1)
       {

            
                tempTime = 0;


                Debug.Log($"Update Color Mac Address: {bulb.MacAddress}");


                        bulb.GetState();

               
            

                Bulb.BulbColor bulbcolors = bulb.GetBulbColor();

                red = (float)bulbcolors.Red / 255;
                blue = (float)bulbcolors.Blue / 255;
                green = (float)bulbcolors.Green / 255;
                warmWhite = (float)bulbcolors.WarmWhite / 255;
                coldWhite = (float)bulbcolors.ColdWhite / 255;

                if (warmWhite != 0 || coldWhite != 0)
                {
                    red = warmWhite + (coldWhite / 1.2f);
                    green = warmWhite + coldWhite;
                    blue = (warmWhite / 1.6f + (coldWhite));
                }





                Color mnewColor = new Color(red, green, blue, 1);
               spriteRenderer.color = mnewColor;

    

        }

    }

    }


