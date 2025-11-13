// ==============================================
// ================= IK =========================
// ==============================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IK : MonoBehaviour
{
    [Header("Références principales")]
    [Tooltip("Transform racine de l'arbre de joints.")]
    public GameObject rootNode = null;

    [Tooltip("Transform (probablement une feuille) qui doit atteindre la cible.")]
    public Transform srcNode = null;

    [Tooltip("Transform cible que la feuille doit atteindre.")]
    public Transform targetNode = null;

    [Header("Configuration des chaînes IK")]
    [Tooltip("Si vrai, recrée toutes les chaînes dans Update.")]
    public bool createChains = true;

    [Tooltip("Liste de toutes les chaînes cinématiques.")]
    public List<IKChain> chains = new List<IKChain>();

    [Tooltip("Nombre d'itérations de l'algorithme IK par update.")]
    public int nb_ite = 10;

    [Tooltip("Si vrai, chaque feuille a sa propre cible (Target_<leaf name>).")]
    public bool unique_target = false;

    [Header("Contrôle et débogage")]
    [Tooltip("Touche clavier pour effectuer une étape unique de l'IK.")]
    public KeyCode ikStepKey = KeyCode.I;

    [Tooltip("Si vrai, l'IK s'exécutera en continu.")]
    public bool nonStopIK = false;

    [Tooltip("Facteur de lissage pour appliquer les transformations IK aux joints.")]
    public float smoothFactor = 20f;

    [Tooltip("Si vrai, affiche les lignes de debug et les informations dans la console.")]
    public bool debugLines = false;


    void Start()
    {
        if (createChains)
        {
            if (debugLines)
                Debug.Log("On crée une nouvelle CHAIN");
            createChains = false;    // la chaîne est créée une seule fois, au début
            chains.Clear();

            // On va trouver le premier carrefour
            Transform carrefour = FindFirstCarrefour(rootNode.transform);

            if (carrefour == null)
            {
                if (debugLines)
                    Debug.LogWarning("Aucun carrefour trouvé dans l'arbre. On va créer une chaîne simple.");
                IKChain simpleChain = new IKChain(rootNode.transform, srcNode, rootNode.transform, targetNode);
                chains.Add(simpleChain);
            }

            // Créer la chaîne principale (rootNode → carrefour)
            if (carrefour != null && carrefour != rootNode.transform)
            {
                IKChain mainChain = new IKChain(rootNode.transform, carrefour, rootNode.transform, targetNode);
                if (debugLines)
                    Debug.Log($"Chaîne principale: {rootNode.name} → {carrefour.name}");
                chains.Add(mainChain);
            }

            // Création des chaînes pour chaque branche à partir du carrefour
            if (carrefour != null)
            {
                CreateBranchChains(carrefour);
            }

            if (debugLines)
                Debug.Log("Total chains created: " + chains.Count);
            // Fusionner les joints partagés entre toutes les chaînes
            if (chains.Count > 1)
            {
                if (debugLines)
                    Debug.Log("=== Fusion des joints partagés ===");

                for (int i = 0; i < chains.Count; i++)
                {
                    for (int j = i + 1; j < chains.Count; j++)
                    {
                        // Comparer tous les joints de la chaîne i avec ceux de la chaîne j
                        foreach (IKJoint ji in chains[i].GetJoints())
                        {
                            foreach (IKJoint jj in chains[j].GetJoints())
                            {
                                if (ji.transform == jj.transform)
                                {
                                    // Fusionne jj vers ji (ji devient le maître)
                                    jj.MergeWith(ji);
                                    if (debugLines)
                                        Debug.Log($"✓ Joint {ji.name} fusionné entre chaîne {i} et {j}");
                                }
                            }
                        }
                    }
                }
            }

        }
    }

    void LateUpdate()
    {
        if (createChains)
            Start();

        if (Input.GetKeyDown(ikStepKey) && !nonStopIK)
        {
            Debug.Log("IK One Step");
            IKOneStep(true);
        }
        if (nonStopIK)
        {
            IKOneStep(true);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Chains count=" + chains.Count);
            foreach (IKChain ch in chains)
                ch.Check();
        }
    }


    void IKOneStep(bool down)
    {
        int j;

        for (j = 0; j < nb_ite; ++j)
        {
            foreach (IKChain ch in chains)
            {
                ch.Backward();
            }
            foreach (IKChain ch in chains)
            {
                ch.Forward();
            }
        }
        foreach (IKChain ch in chains)
        {
            ch.ToTransform(smoothFactor);
        }
    }

    Transform FindFirstCarrefour(Transform node)
    {
        if (node == srcNode)
            return null;
        
        if (node.childCount > 1)
            return node;

        foreach (Transform child in node)
        {
            Transform result = FindFirstCarrefour(child);
            if (result != null)
                return result;
        }

        return null;
    }

    Transform GetLastLeaf(Transform t)
    {
        while (t.childCount > 0)
        {
            // Si plusieurs enfants, prendre le premier
            t = t.GetChild(0);
        }
        return t;
    }

    void CreateBranchChains(Transform carrefour)
    {
        foreach (Transform child in carrefour)
        {
            // Vérifier s'il y a un sous-carrefour dans cette branche
            Transform subCarrefour = FindFirstCarrefour(child);

            if (subCarrefour != null && subCarrefour != child)
            {
                // Il y a un sous-carrefour : créer la chaîne jusqu'au sous-carrefour
                IKChain bridgeChain = new IKChain(carrefour, subCarrefour, carrefour, targetNode);
                chains.Add(bridgeChain);
                if (debugLines)
                    Debug.Log($"Chaîne pont: {carrefour.name} → {subCarrefour.name}");

                // Si cette branche contient elle-même un carrefour, continuer récursivement
                CreateBranchChains(subCarrefour);
            }
            else
            {
                // Pas de sous-carrefour : c'est une branche simple jusqu'à une feuille
                Transform leaf = GetLastLeaf(child);
                Transform target = null;

                if (!unique_target)
                    target = targetNode;
                else
                {
                    // Recherche d'une cible nommée "Target_<leaf name>"
                    target = null;
                    GameObject targetObj = GameObject.Find("Target_" + leaf.name);
                    if (targetObj != null)
                        target = targetObj.transform;
                    else
                    {
                        if (debugLines)
                        {
                            Debug.LogWarning($"Aucune cible trouvée pour la feuille {leaf.name} (cherchée: Target_{leaf.name})");
                            Debug.LogWarning("On va prendre targetNode par défaut ou rien.");
                        }
                        target = targetNode;
                    }
                }

                IKChain chain = new IKChain(carrefour, leaf, null, target);
                chains.Add(chain);
                if (debugLines)
                    Debug.Log($"Chaîne branchée: {carrefour.name} → {leaf.name}");
            }
        }
    }

}


