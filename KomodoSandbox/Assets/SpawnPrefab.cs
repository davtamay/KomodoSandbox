//i want a monobehaviour that spawns a prefab everytime a tmp button is pressed, i dont want the prefabs to overlap, i want them to be one unit next to each to the latest one
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SpawnPrefab : MonoBehaviour
{
    public GameObject prefab;

    public GameObject button;
    public float spawnDistance = 0.1f;
    public float originalPos = 0;
    public TextMeshProUGUI cubesInScene;
    public int cubes = 0;
    public void Spawn()
    {
        Vector3 spawnPos = new Vector3(originalPos + spawnDistance,1,0)  ;
        spawnDistance += 0.01f;
        GameObject newObject = Instantiate(prefab, spawnPos, Quaternion.identity);
        cubes +=1;
        cubesInScene.text = "Cubes/1m in Scene: " + cubes;
        // newObject.transform.LookAt(spawnLocation.transform);
        // newObject.transform.Rotate(0, 180, 0);
        // newObject.GetComponent<Rigidbody>().AddForce(spawnLocation.transform.forward * 500);
        // newObject.GetComponent<Rigidbody>().AddForce(spawnLocation.transform.up * 500);
        // newObject.GetComponent<Rigidbody>().AddTorque(spawnLocation.transform.up * 500);
        // newObject.GetComponent<Rigidbody>().AddTorque(spawnLocation.transform.right * 500);
        button.GetComponent<Button>().interactable = false;
        StartCoroutine(ResetButton());
    }
    IEnumerator ResetButton()
    {
        yield return new WaitForSeconds(1f);
        button.GetComponent<Button>().interactable = true;
    }
}