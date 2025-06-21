using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using Zenject;

public class MarkerController : MonoBehaviour
{
    //[SerializeField] private Marker questMarker;

    //[SerializeField] private CustomPool<Marker> questMarkers;

    //private Dictionary<string, Marker> markersDict = new Dictionary<string, Marker>();

    //[SerializeField] private Camera playerCamera;


    //private Camera currentCamera;

    //[Inject]
    //private void Construct(
    //     CompanionCamera companionCamera, PlayerCamera playerCamera)
    //{
    //    this.playerCamera = playerCamera.GetCamera();
    //}

    //void Start()
    //{

    //    GlobalEventManager.OnCharacterSwitch.AddListener(CameraChange);
    //}
    //void Awake()
    //{
    //    currentCamera = playerCamera;
    //    questMarkers = new CustomPool<Marker>(questMarker, 5, transform);
    //}

    //public void GetMarker(string stepName, Transform target)
    //{
    //    if (markersDict.ContainsKey(stepName)) return;
    //    Marker marker = questMarkers.GetFromPool();
    //    marker.SetTarget(target);
    //    marker.SetCamera(currentCamera);
    //    markersDict.Add(stepName, marker);
    //}

    //public void ReturnMarker(string stepName)
    //{
    //    Debug.Log("ReturnMarker: " + stepName);
    //    questMarkers.ReleaseToPool(markersDict[stepName]);
    //    markersDict.Remove(stepName);
    //}

    //public void CameraChange(bool turn)
    //{
    //    currentCamera = playerCamera;
    //    foreach (var activeMarker in questMarkers.GetAllActives())
    //    {
    //        activeMarker.SetCamera(currentCamera);
    //    }
    //}

}
