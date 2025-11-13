using UnityEngine;

public class WallBuilder : MonoBehaviour
{
    [Header("Modèle de brique et configuration")]
    [Tooltip("Prefab de la brique à utiliser pour construire le mur.")]
    public GameObject brickPrefab; // Le cube ou prefab de brique
    [Tooltip("Nombre de lignes du mur.")]
    public int rows = 5;           // Nombre de lignes (vertical)
    [Tooltip("Nombre de colonnes du mur.")]
    public int columns = 10;       // Nombre de colonnes (horizontal)
    [Tooltip("Espace entre chaque briques.")]
    public float spacing = 0.1f;   // Espace entre les briques
    [Tooltip("Si vrai, les briques sont disposées en quinconce, sinon juste alignées.")]
    public bool quinconce = true; // Si true, les lignes sont en quinconce

    void Start()
    {
        BuildWall();
    }

    void BuildWall()
    {
        if (brickPrefab == null)
        {
            Debug.LogWarning("Prefab de brique manquant !");
            return;
        }

        // Taille de la brique
        Vector3 brickSize = brickPrefab.GetComponent<Renderer>().bounds.size;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                float offsetX;
                if (quinconce)
                    offsetX = (y % 2 == 0) ? 0 : brickSize.x / 2;
                else
                    offsetX = 0;

                Vector3 pos = new Vector3(x * (brickSize.x + spacing) + offsetX, y * (brickSize.y + spacing), 0);
                Instantiate(brickPrefab, transform.position + pos, Quaternion.identity, transform);
            }
        }
    }
}
