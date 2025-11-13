// ===========================================================================================================================
// =========================================== IKChain =======================================================================
// ===========================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class IKChain
{
    // Quand la chaine comporte une cible pour la racine. 
    // Ce sera le cas que pour la chaine comportant le root de l'arbre.
    private IKJoint rootTarget = null;

    // Quand la chaine à une cible à atteindre, 
    // ce ne sera pas forcément le cas pour toutes les chaines.
    private IKJoint endTarget = null;

    // Toutes articulations (IKJoint) triées de la racine vers la feuille. N articulations.
    private List<IKJoint> joints = new List<IKJoint>();

    // Contraintes pour chaque articulation : la longueur (à modifier pour 
    // ajouter des contraintes sur les angles). N-1 contraintes.
    private List<float> constraints = new List<float>();


    // Un cylndre entre chaque articulation (Joint). N-1 cylindres.
    //private List<GameObject> cylinders = new List<GameObject>();    



    // Créer la chaine d'IK en partant du noeud endNode et en remontant jusqu'au noeud plus haut, ou jusqu'à la racine
    public IKChain(Transform _rootNode, Transform _endNode, Transform _rootTarget, Transform _endTarget)
    {
        // Debug.Log("=== IKChain::createChain: ===");
        // TODO : construire la chaine allant de _endNode vers _rootTarget en remontant dans l'arbre (ou l'inverse en descente). 
        // Chaque Transform dans Unity a accès à son parent 'tr.parent'

        // Cibles et origine
        if (_rootTarget != null)
            rootTarget = new IKJoint(_rootTarget);

        if (_endTarget != null)
            endTarget = new IKJoint(_endTarget);


        // Construction de la liste des joints
        Transform current = _endNode;
        while (current != null)
        {
            joints.Insert(0, new IKJoint(current));
            // Debug.Log($"  Ajout joint: {current.name}");
            if (current == _rootNode)
                break;
            current = current.parent;
        }
        // Calcul des contraintes
        for (int i = 0; i < joints.Count - 1; i++)
        {
            float length = Vector3.Distance(joints[i].positionTransform, joints[i + 1].positionTransform);
            constraints.Add(length);
            joints[i].InitializeChildDirection(joints[i + 1]);
        }

    }


    public void Merge(IKJoint j)
    {
        // TODO-2 : fusionne les noeuds carrefour quand il y a plusieurs chaines cinématiques
        // Dans le cas d'une unique chaine, ne rien faire pour l'instant.

        foreach (IKJoint myJoint in joints)
        {
            if (myJoint.transform == j.transform)
            {
                // Fait pointer j vers myJoint (myJoint devient le maître)
                j.MergeWith(myJoint);
                // Debug.Log($"Joint fusionné: {j.name}");
                return;
            }
        }

    }


    public IKJoint First()
    {
        return joints[0];
    }
    public IKJoint Last()
    {
        return joints[joints.Count - 1];
    }

    public void Backward()
    {
        // TODO : une passe remontée de FABRIK. Placer le noeud N-1 sur la cible, 
        // puis on remonte du noeud N-2 au noeud 0 de la liste 
        // en résolvant les contrainte avec la fonction Solve de IKJoint.

        if (endTarget == null) { return; }

        // placer le dernier joint sur la cible
        joints[joints.Count - 1].SetPosition(endTarget.positionTransform);

        // remonter dans la chaîne
        for (int i = joints.Count - 2; i >= 0; i--)
        {
            joints[i].Solve(joints[i + 1], constraints[i]);
        }

    }

    public void Forward()
    {
        // TODO : une passe descendante de FABRIK. Placer le noeud 0 sur son origine puis on descend.
        // Codez et deboguez déjà Backward avant d'écrire celle-ci.

        // On place le premier noeud sur sa cible ou son origine
        if (rootTarget != null)
            joints[0].SetPosition(rootTarget.positionTransform);
        else
            joints[0].SetPosition(joints[0].position); // garde sa position

        // descendre dans la chaîne
        for (int i = 1; i < joints.Count; i++)
        {
            joints[i].Solve(joints[i - 1], constraints[i - 1]);
        }
    }

    public void ToTransform(float smooth = 5f)
    {
        // TODO : pour tous les noeuds de la liste appliquer la position au transform : voir ToTransform de IKJoint

        for (int i = 0; i < joints.Count - 1; i++)
        {
            joints[i].UpdateRotationToChild(joints[i + 1]);
        }

        foreach (IKJoint j in joints)
            j.ToTransform(smooth);
    }

    public void Check()
    {
        // TODO : des Debug.Log pour afficher le contenu de la chaine (ne sert que pour le debug)
        // Debug.Log("IKChain joints = " + joints.Count);
        for (int i = 0; i < joints.Count; i++)
        {
            // Debug.Log($"  {i}: {joints[i].name} at {joints[i].position}");
        }
    }

    public List<IKJoint> GetJoints()
    {
        return joints;
    }

}

