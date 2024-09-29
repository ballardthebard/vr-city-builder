using UnityEngine;

public class ElementSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private GameObject[] elements;

    private GameObject lastElement;

    public void SpawnElement(int index)
    {
        if (spawnPosition.childCount > 0)
        {
            Destroy(lastElement);
        }

        lastElement = Instantiate(elements[index], spawnPosition.position, Quaternion.identity, spawnPosition);
    }
}