using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeWithNewMaterialInstantiation : MonoBehaviour
{

    public GameObject cube;
    public Material material;
    public float value;
   
    public int lowestLimit = 0;
    public int currentLimit = 0;

    public float currentPosition = 0;

    public Transform parent;

    public List<string> shaderTypes = new List<string>();
    
    public void OnValueChanged(float value)
    {
        int valueInt = (int)value;

        if ((int)value > lowestLimit){

            for(int i = lowestLimit; i < valueInt -1; i++){
                InstantiatePrimitiveWithNewMaterial();
            }

            lowestLimit =valueInt;


        }
        else
            return;


        InstantiatePrimitiveWithNewMaterial();
        

    }

    public void InstantiatePrimitiveWithNewMaterial(){

material = new Material(Shader.Find(shaderTypes[Random.Range(0, shaderTypes.Count)])); 
// material = new Material(Shader.Find("Universal Render Pipeline/Unlit")); 
// switch (Random.Range(0, shaderTypes.Count +1)){
// case 0:
//  material = new Material(Shader.Find("Universal Render Pipeline/Unlit")); 
//  break;
//  case 1:
//  cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//  break;
//  case 2:
//  cube = GameObject.CreatePrimitive(PrimitiveType.Capsule);
//  break;
//  case 3:
//  cube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
//  break;

// }
        
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube.transform.SetParent(parent);
        cube.transform.localPosition = Vector3.zero;

        currentPosition += 1f;
        cube.transform.localPosition = new Vector3(currentPosition, 0, 0);
        material.color = new Color(Random.value, Random.value, Random.value);
        cube.GetComponent<MeshRenderer>().material = material;

    }
}

