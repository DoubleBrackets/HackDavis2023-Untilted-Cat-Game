using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [System.Serializable]
    public struct Location
    {
        public Transform target;
        public InteractionConfigSO interactionConfigSo;
    }   
    
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] public List<Location> locations;
    [SerializeField] public int startIndex;

    public Location CurrentLocation => locations[currLocIndex];
    private int currLocIndex = -1;

    public int CurrLocIndex => currLocIndex;

    public void Initialize()
    {
        currLocIndex = startIndex;
        CatController.Instance.MoveToLocation(locations[startIndex].target.transform.position);
    }

    public async UniTask PerformInteraction(string newLocationName)
    {
        Debug.Log($"Trying to move to {newLocationName}");
        newLocationName = newLocationName.Replace(" ", "");
        int target = 0;
        for (int i = 0; i < locations.Count; i++)
        {
            string locName = locations[i].interactionConfigSo.name.ToLower();
            if (newLocationName.ToLower().Contains(locName))
                target = i;
        }

        if (target != currLocIndex)
        {
            currLocIndex = target;
            await CatController.Instance.MoveToLocation(locations[target].target.transform.position);
        }
    }
}
