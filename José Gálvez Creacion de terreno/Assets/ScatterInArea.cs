using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Coloca copias de un prefab al azar dentro de una zona (caja amarilla en la escena).
// Uso: agrega este script a un GameObject vacío, asigna el prefab, ajusta la zona
// y en el menú del componente (⋮) elige "Generar".
public class ScatterInArea : MonoBehaviour
{
    [Header("Qué colocar")]
    public GameObject prefab;
    public int cantidad = 20;

    [Header("Zona (ancho X, alto Y, largo Z)")]
    public Vector3 tamanoZona = new Vector3(50f, 10f, 50f);

    [Header("Variación")]
    public float escalaMinima = 1f;
    public float escalaMaxima = 2f;
    public bool rotacionAleatoria = true;
    public float distanciaMinima = 3f;   // separación mínima entre objetos

    [ContextMenu("Generar")]
    void Generar()
    {
        if (prefab == null) { Debug.LogWarning("Asigna un prefab primero."); return; }

        Terrain terreno = Terrain.activeTerrain;
        List<Vector3> puestos = new List<Vector3>();
        int intentos = 0;

        while (puestos.Count < cantidad && intentos < cantidad * 20)
        {
            intentos++;

            Vector3 pos = transform.position + new Vector3(
                Random.Range(-tamanoZona.x / 2f, tamanoZona.x / 2f),
                0f,
                Random.Range(-tamanoZona.z / 2f, tamanoZona.z / 2f));

            // Respetar separación mínima
            bool muyCerca = false;
            foreach (Vector3 p in puestos)
            {
                if (Vector2.Distance(new Vector2(p.x, p.z), new Vector2(pos.x, pos.z)) < distanciaMinima)
                { muyCerca = true; break; }
            }
            if (muyCerca) continue;

            // Apoyar sobre el terreno
            if (terreno != null)
                pos.y = terreno.SampleHeight(pos) + terreno.transform.position.y;

            GameObject obj;
#if UNITY_EDITOR
            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(obj, "Generar objetos");
#else
            obj = Instantiate(prefab);
#endif
            obj.transform.position = pos;
            if (rotacionAleatoria)
                obj.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            obj.transform.localScale = prefab.transform.localScale * Random.Range(escalaMinima, escalaMaxima);
            obj.transform.SetParent(transform, true);

            puestos.Add(pos);
        }

        Debug.Log($"Generados {puestos.Count} objetos.");
    }

    [ContextMenu("Borrar generados")]
    void BorrarGenerados()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);
#else
            DestroyImmediate(transform.GetChild(i).gameObject);
#endif
        }
    }

    // Dibuja la zona en la escena cuando el objeto está seleccionado
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, tamanoZona);
    }
}