using UnityEngine;

[CreateAssetMenu]
public class AudioClipRefsSO : ScriptableObject
{
    public AudioClip[] Chop;
    public AudioClip[] DeliveryFail;
    public AudioClip[] DeliverySuccess;
    public AudioClip[] Footstep;
    public AudioClip[] ObjectDrop;
    public AudioClip[] ObjectPickup;
    public AudioClip[] Trash;
    public AudioClip[] Warning;
}