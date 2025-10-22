using UnityEngine; 
using System.Collections.Generic; 

[System.Serializable] 
public class PlatformType 
{ 
	public GameObject prefab; 
	public float weight; 
}

public class LevelGenerator : MonoBehaviour
{
    [Header("Core platforms")]
    public List<PlatformType> platformTypes;

    [Header("Platform tricks")]

    [Header("Final stage")]
    public GameObject finalPlatformPrefab;
    private bool generationStopped = false;

    public GameObject breakPlatformPrefab;
    [Range(0f, 1f)] public float breakPlatformChance = 0.2f;

    [Header("Player and Generation")]
    public Transform player;
    private float highestY; // самая верхняя сгенерированная точка 

    public int numberOfPlatforms = 200;
    public float levelWidth = 3f;
    public float minY = .2f;
    public float maxY = 1.5f;

    // выбор из core платформ по весам 
    GameObject ChooseCorePlatform()
    {
        float totalWeight = 0f;
        foreach (var type in platformTypes)
            totalWeight += type.weight;

        float roll = Random.value * totalWeight;
        foreach (var type in platformTypes)
        {
            if (roll < type.weight)
                return type.prefab;
            roll -= type.weight;
        }

        return platformTypes[0].prefab; // fallback 
    }

    void Start()
    {
        highestY = 0f;
        GeneratePlatforms(numberOfPlatforms); // первая пачка 
    }

    void Update()
    {
        if (generationStopped) return;

        // если игрок поднялся выше половины сгенерированного уровня — добавляем ещё 
        if (player.position.y + 10f > highestY)
        {
            GeneratePlatforms(50);
        }
    }

    void GeneratePlatforms(int count)
    {
        Vector3 spawnPosition = new Vector3();

        for (int i = 0; i < count; i++)
        {
            spawnPosition.y = highestY + Random.Range(minY, maxY);
            spawnPosition.x = Random.Range(-levelWidth, levelWidth);

            SpawnPlatform(spawnPosition);

            highestY = spawnPosition.y;
        }
    }

    void SpawnPlatform(Vector3 position)
    {
        // генерируем core платформу 
        GameObject corePrefab = ChooseCorePlatform();
        Instantiate(corePrefab, position, Quaternion.identity);

        // с шансом добавляем ловушку 
        if (breakPlatformPrefab != null && Random.value < breakPlatformChance)
        {
            Instantiate(breakPlatformPrefab, position, Quaternion.identity);
        }
    }

    public void StopGenerationAndSpawnFinal()
    {
        generationStopped = true;

        // Считаем высоту, на которой спавним финальную платформу 
        float cameraTopY = Camera.main.transform.position.y + Camera.main.orthographicSize; // верх камеры 
        float finalY = cameraTopY + 3f; // чуть выше камеры 
        Vector3 finalPos = new Vector3(0, finalY, 0);

        // Спавн финальной платформы 
        GameObject finalPlat = Instantiate(finalPlatformPrefab, finalPos, Quaternion.identity);

        // Удаляем все платформы, которые выше финальной 
        foreach (var platform in GameObject.FindGameObjectsWithTag("Platform"))
        {
            if (platform.transform.position.y > finalY)
                Destroy(platform);
        }
    }
} 