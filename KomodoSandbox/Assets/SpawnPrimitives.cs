using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrimitives : MonoBehaviour
{

     public GameObject cube;
    public Material material;
    public float value;
   
    public int lowestLimit = 0;
    public int currentLimit = 0;

    public float currentPosition = 0;

     public float currentYPosition = 0;

    public Transform parent;

    public void OnValueChanged(float value)
    {
          int valueInt = (int)value;

           if ((int)value > lowestLimit){

            for(int i = lowestLimit; i < valueInt -1; i++){
                SpawnPrimitive();
            }

            lowestLimit =valueInt;


        }
        else
            return;


SpawnPrimitive();
//        material.color = new Color(Random.value, Random.value, Random.value);
       // cube.GetComponent<MeshRenderer>().material = material;

    }

    void SpawnPrimitive(){

        switch (Random.Range(0, 4))
        {
            case 0:
                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case 1:
                cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case 2:
                cube = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            case 3:
                cube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;

        }

        //cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<MeshRenderer>().material = material;
      //  cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube.transform.SetParent(parent);
        cube.transform.localPosition = Vector3.zero;


        if(currentPosition % 90 == 0){
            currentYPosition -= 1.5f;
            currentPosition = 0;
        }


        cube.transform.localPosition = new Vector3(currentPosition, currentYPosition, 0);
        currentPosition += 1.5f;

    }
    //     GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //     primitive.transform.position = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f));
    //     primitive.transform.localScale = new Vector3(Random.Range(1.0f, 5.0f), Random.Range(1.0f, 5.0f), Random.Range(1.0f, 5.0f));
    //     primitive.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 5.0f), Random.Range(0.0f, 5.0f), Random.Range(0.0f, 5.0f));
    // }
//create a script with a function that spawns one primitive, they all should follow a grid type of formation especially since I want to spawn 3000
}
