// ===========================================================================================================================
// ============================= IKJoint =====================================================================================
// ===========================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class IKJoint
{
    // la position modifiée par l'algo : en fait la somme des positions des sous-branches. 
    // _weight comptera le nombre de sous-branches ayant touchées cette articulation.
    private Vector3 _position;

    // un lien vers le Transform de l'arbre d'Unity
    private Transform _transform;

    // un poids qui indique combien de fois ce point a été bougé par l'algo.
    private float _weight = 0.0f;


    private IKJoint _mergedWith = null;

    private Quaternion _initialRotation;
    private Quaternion _rotation;

    private Vector3 _initialDirectionToChild;



    public string name
    {
        get
        {
            return _transform.name;
        }
    }

    public void MergeWith(IKJoint master)
    {
        _mergedWith = master;
    }

    public Vector3 position
    {
        get
        {
            if (_mergedWith != null) return _mergedWith.position;
            if (_weight == 0.0f) return _position;
            else return _position / _weight;
        }
    }


    public Vector3 positionTransform
    {
        get
        {
            return _transform.position;
        }
    }

    public Transform transform
    {
        get
        {
            return _transform;
        }
    }

    public Vector3 positionOrigParent
    {
        get
        {
            return _transform.parent.position;
        }
    }



    public IKJoint(Transform t)
    {
        // TODO : initialise _position, _weight
        _transform = t;
        _position = t.position;
        _weight = 1.0f;
        _initialRotation = t.rotation;
        _rotation = t.rotation;
    }

    public void SetPosition(Vector3 p)
    {
        if (_mergedWith != null)
        {
            _mergedWith.SetPosition(p);
            return;
        }
        _position = p;
        _weight = 1.0f;

    }

    public void AddPosition(Vector3 p)
    {
        // TODO : ajoute une position à 'position' et incrémente '_weight'
        if (_mergedWith != null)
        {
            _mergedWith.AddPosition(p);
            return;
        }
        _position += p;
        _weight += 1.0f;

    }

    public void InitializeChildDirection(IKJoint child)
    {
        _initialDirectionToChild = (child.positionTransform - positionTransform).normalized;
    }

    public void UpdateRotationToChild(IKJoint child)
    {
        if (_mergedWith != null)
        {
            _mergedWith.UpdateRotationToChild(child);
            return;
        }

        Vector3 currentDirection = (child.position - position).normalized;
        if (currentDirection.sqrMagnitude < 0.0001f || _initialDirectionToChild.sqrMagnitude < 0.0001f)
            return;

        // Calcule la rotation nécessaire pour aligner la direction initiale vers la direction actuelle
        Quaternion deltaRotation = Quaternion.FromToRotation(_initialDirectionToChild, currentDirection);

        // Applique cette rotation à la rotation de repos (en world space)
        _rotation = deltaRotation * _initialRotation;
    }



    public void ToTransform()
    {
        // TODO : applique la _position moyenne au transform, et remet le poids à 0

        if (_mergedWith != null)
        {
            _transform.position = _mergedWith.position;
            _transform.rotation = _mergedWith._rotation;
            return;
        }


        _transform.position = position;

        // Réinitialise pour la prochaine itération
        _position = _transform.position;
        _rotation = _transform.rotation;
        _weight = 1.0f;


    }

    // Même fonction mais avec une interpolation douce pour que les mouvements soient plus jolis vers la nouvelle position
    public void ToTransform(float smooth = 5f)
    {
        if (_mergedWith != null) return;

        // Interpolation douce vers la nouvelle position moyenne (ça fait plus joli je trouve)
        _transform.position = Vector3.Lerp(_transform.position, position, Time.deltaTime * smooth);
        _transform.rotation = Quaternion.Slerp(_transform.rotation, _rotation, Time.deltaTime * smooth);


        // Réinitialisation pour la prochaine itération
        _position = _transform.position;
        _weight = 1.0f;
    }


    public void Solve(IKJoint anchor, float l)
    {
        if (_mergedWith != null)
        {
            _mergedWith.Solve(anchor, l);
            return;
        }

        // direction du joint vers l’ancre
        Vector3 dir = (position - anchor.position).normalized;

        // position exacte à distance l de l’ancre
        _position = anchor.position + dir * l;

        _weight = 1.0f;

    }
}
