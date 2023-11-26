using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(Terrain))]
public class TerrainGenerator : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;
    private float[,] heightMap;
    private int resolution;
    private float[,,] alphaData;

    private const int SAND = 0;
    private const int DIRT = 1;
    private const int GRASS = 2;
    private const int ROCK = 3;

    [Header("Noise Settings")]
    public float scale = 500;
    public Vector3 size = new Vector3(50, 30, 50);
    public int octaves = 4;

    [Header("Tree Settings")]
    public GameObject treePrefab;
    public int maxTrees = 100;
    public float treeDensity = 0.1f;

    private const float SAND_THRESHOLD = 0.1f;
    private const float DIRT_THRESHOLD = 0.2f;
    private const float GRASS_THRESHOLD = 0.7f;

    void Start()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        resolution = terrainData.heightmapResolution;

        terrainData.size = size;

        GenerateHeightMap();
        AddTexture();
        SpawnTreesOnGrass();
    }

    void GenerateHeightMap()
    {
        heightMap = new float[resolution, resolution];

        var seed = UnityEngine.Random.Range(0, 1000);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                for (int o = 0; o < octaves; o++)
                {
                    var px = (x + seed) / scale * Mathf.Pow(2, o);
                    var py = (y + seed) / scale * Mathf.Pow(2, o);
                    var sign = o % 2 == 0 ? 1 : -1;

                    var noiseValue = (noise.snoise(new float2(px, py)) + 1) / 2 / Mathf.Pow(2, o);

                    heightMap[x, y] += noiseValue * sign;
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    void AddTexture()
    {
        alphaData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapHeight);

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                alphaData[x, y, SAND] = 0;
                alphaData[x, y, DIRT] = 0;
                alphaData[x, y, GRASS] = 0;
                alphaData[x, y, ROCK] = 0;

                if (heightMap[x, y] < SAND_THRESHOLD) alphaData[x, y, SAND] = 1;
                else if (heightMap[x, y] < DIRT_THRESHOLD) alphaData[x, y, DIRT] = 1;
                else if (heightMap[x, y] < GRASS_THRESHOLD) alphaData[x, y, GRASS] = 1;
                else alphaData[x, y, ROCK] = 1;
            }
        }
        terrainData.SetAlphamaps(0, 0, alphaData);
    }

    void SpawnTreesOnGrass()
    {
        float[,,] alphaData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapHeight);

        List<Vector3> grassLocations = new List<Vector3>();

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                if (alphaData[x, y, GRASS] > 0)
                {
                    float normalizedX = x / (float)terrainData.alphamapWidth;
                    float normalizedY = y / (float)terrainData.alphamapHeight;

                    Vector3 worldPos = new Vector3(normalizedX * size.x, 0, normalizedY * size.z) + transform.position;

                    float terrainHeight = terrainData.GetHeight(Mathf.RoundToInt(normalizedX * (resolution - 1)), Mathf.RoundToInt(normalizedY * (resolution - 1)));
                    worldPos.y = terrainHeight + transform.position.y;

                    grassLocations.Add(worldPos);
                }
            }
        }

        for (int i = grassLocations.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Vector3 temp = grassLocations[i];
            grassLocations[i] = grassLocations[j];
            grassLocations[j] = temp;
        }

        int treesToSpawn = Mathf.Min(maxTrees, Mathf.FloorToInt(grassLocations.Count * treeDensity));

        for (int i = 0; i < treesToSpawn; i++)
        {
            Vector3 treePos = grassLocations[i];
            Instantiate(treePrefab, treePos, Quaternion.identity);
        }
    }

}
