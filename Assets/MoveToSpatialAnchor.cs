using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToSpatialAnchor : MonoBehaviour
{
    // Start is called before the first frame update
    public void PositionSet(Vector3 postion, Quaternion rotation)
    {
        // only change X and Z position (is fixed to floor)
        transform.position = new Vector3(postion.x, transform.position.y, postion.z);
        // only change Y rotation (can only turn on that axis when set to floor)
        transform.rotation = Quaternion.Euler(transform.rotation.x, rotation.y, transform.rotation.z);
    }
}
