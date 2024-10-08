﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace ATTMoreFarmPlots;

[BepInPlugin("com.michaelrooplall.mods.attmorefarmplots", "More Farm Plots", "0.9.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
        
    private void Awake() {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"[MORE FARM PLOTS] Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        var harmony = new Harmony("com.michaelrooplall.mods.attmorefarmplots.patch");

        harmony.PatchAll(typeof(PatchGardenManager_Awake));

    }

    public class PatchGardenManager_Awake {

        [HarmonyDebug]
        [HarmonyPatch(typeof(GardenManager), "Awake")]
        [HarmonyPrefix]
        static void Prefix(GardenManager __instance) {
            if (GardenManager.Instance != null) return;
            buildSecondFarmPlot(__instance);
        }

        [HarmonyDebug]
        [HarmonyPatch(typeof(GardenManager), "Awake")]
        [HarmonyPostfix]
        static void Postfix(GardenManager __instance) {

            if (NetworkManager.Singleton.IsHost) {

                MethodInfo LoadGameMethod = __instance.gardenBeds[0].GetType().GetMethod("LoadGame", BindingFlags.NonPublic | BindingFlags.Instance);
                
                for (int i = 15; i < __instance.gardenBeds.Length; i++) {

                    Debug.Log($"[MORE FARM PLOTS] Calling LoadGame() for additional GardenBed {i} [Does Save Exist? {__instance.gardenBeds[i].savedGardenBed != null}]");

                    LoadGameMethod.Invoke(__instance.gardenBeds[i], []);

                }
                
            }

        }
    }
    

    private static void buildSecondFarmPlot(GardenManager gardenManager) {

        Debug.Log("\n\n[MORE FARM PLOTS] Building Second Farm Plot");

        if (NetworkManager.Singleton.IsHost) {

            GameObject gardenPlotBase = GameObject.Find("Garden");
            GameObject gardenPlotPrefab = GameObject.Find("Garden/Garden Bed Piece");

            if (gardenPlotBase == null) {
                Debug.LogError("[MORE FARM PLOTS] Failed to find gardenPlotBase at Garden");
                return;
            }

            if (gardenPlotPrefab == null){
                Debug.LogError("[MORE FARM PLOTS] Failed to find gardenPlotPrefab at Garden/Garden Bed Piece");
                return;
            }


            float offsetDif = 1.5f;

            float baseX = -36.50f;
            float baseY = gardenPlotPrefab.transform.position.y;
            float baseZ = 62.0f;

            if (gardenManager.gardenBeds == null) {
                Debug.LogError("[MORE FARM PLOTS] GardenManager gardenBeds failed to load on time");
                return;
            }

            GardenBed[] gardenBeds = gardenManager.gardenBeds;

            if (gardenBeds == null) {
                Debug.LogError("[MORE FARM PLOTS] Failed to get gardenBeds field from gardenPlotBase gardenManager");
                return;
            }

            int ctr = gardenBeds.Length;

            int numRows = 3;
            int numCols = 5;
            
            Debug.Log("[MORE FARM PLOTS] Creating new list for more garden beds");

            GardenBed[] newGardenBeds = new GardenBed[ctr + (numRows * numCols)];

            for (int i = 0; i < ctr; i++) {
                newGardenBeds[i] = gardenBeds[i];
            }

            Vector3 firstPaintPosition = new Vector3(0, 0, 0);
            Vector3 lastPaintPosition = new Vector3(0, 0, 0);

            if (NetworkManager.Singleton == null){
                Debug.LogError("[MORE FARM PLOTS] NetworkManager is not ready");
                return;
            }

            GameObject gameObjectWithNetworkObjecToHijack = GameObject.Find("Game");
            GameObject gardenPlotTwo = gameObjectWithNetworkObjecToHijack; // new GameObject("Garden_MoreFarmPlots");
            // gardenPlotTwo.transform.SetParent(gameObjectWithNetworkObjecToHijack.transform);

            // Debug.Log("[MORE FARM PLOTS] Adding Network Object");
            // NetworkObject baseNetworkObject = gardenPlotTwo.AddComponent<NetworkObject>();

            NetworkObject baseNetworkObject = gardenPlotTwo.GetComponent<NetworkObject>();

            Debug.Log($"[MORE FARM PLOTS] Found {ctr} Garden Beds");

            gardenPlotPrefab.SetActive(false);

            for (int x = 0; x < numRows; x++) {
                for (int z = 0; z < numCols; z++) {

                    Debug.Log($"[MORE FARM PLOTS] Creating Additional Garden Plot ({x},{z}) [{ctr}]");
                    GameObject newGardenPlot = Instantiate(gardenPlotPrefab);
                    newGardenPlot.SetActive(false);

                    Debug.Log("Removing duplicate NetworkObject");
                    NetworkObject oldNetworkObject = newGardenPlot.GetComponent<NetworkObject>();
                    if (oldNetworkObject != null) {
                        DestroyImmediate(oldNetworkObject);
                    }

                    newGardenPlot.name = $"Garden Bed Piece_{ctr}";
                    newGardenPlot.transform.SetParent(gardenPlotTwo.transform);
                    newGardenPlot.transform.position = new Vector3(baseX + (offsetDif * x), baseY, baseZ + (offsetDif * z));

                    GardenBed newGardenBed = newGardenPlot.GetComponent<GardenBed>();

                    if (newGardenBed == null) {
                        Debug.LogError($"[MORE FARM PLOTS] Failed to find GardenBed component on newGardenPlot [{ctr}]");
                    }

                    newGardenPlot.SetActive(true);

                    newGardenBeds[ctr] = newGardenBed;
                    ctr++;

                    if (x == 0 && z == 0) {
                        firstPaintPosition = newGardenPlot.transform.position;
                    } else if (x == numRows - 1 && z == numCols - 1) {
                        lastPaintPosition = newGardenPlot.transform.position;
                    }

                }
            }
            
            gardenPlotPrefab.SetActive(true);

            if (Master.Instance.HasConnectingClients() && AppSettingsManager.Instance.appSettings.system.useSpawnQueue) {
                Game.Instance.SpawnEnqueue(baseNetworkObject);
            } else {
                baseNetworkObject.Spawn();
            }

            Debug.Log("[MORE FARM PLOTS] Adding new gardenBeds to gardenManager");

            // gardenManager.gardenBeds = newGardenBeds;

            Debug.Log("[MORE FARM PLOTS] Finished creating additional garden plots");

            ModifyTerrain(gardenPlotPrefab.transform.position, firstPaintPosition, lastPaintPosition);

        }

        
        // The DuplicateFence() function is disabled because the meshes are statically baked.
        // Any fence clones can't be moved. It just results in invisible hitboxes. Unfortunate.
        // DuplicateFence();

        // We'll just place a lamp on the ground instead.
        ModifyFarmFences();

    }


    private static void ModifyFarmFences() {

        // Clone lamp

        GameObject sourceLantern = GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/WallLamp/Props_Lantern");

        Debug.Log("[MORE FARM PLOTS] Modify Farm Fence Area");

        Vector3 lampPosition = new Vector3(-38, 1.78f, 69);

        GameObject cloneLantern = Instantiate(sourceLantern);
        cloneLantern.name = "MoreFarmPlotsLantern";
        cloneLantern.transform.position = lampPosition;
        cloneLantern.transform.parent = sourceLantern.transform.parent.parent;

        // GameObject lampLightObject = GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/WallLamp/MoreFarmPlotsLantern/Point Light_1");
        // lampLightObject.SetActive(true);

        // Light lampLightComponent = lampLightObject.GetComponent<Light>();

        // lampLightObject.transform.localPosition = new Vector3(0, 3.90f, 0);
        // Remove 2 fences to make it look better

        GameObject[] fences = [
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Fence_05"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Fence_06"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Fence_07"),
        ];

        for (int i = 0; i < fences.Length; i++) {
            Destroy(fences[i]);
        }

    }
    public static void DuplicateFence() {

        GameObject[] fenceOrigins = [
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Fence_05"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Fence_06"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Fence_07"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Fence_08"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Fence_09"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Props_TreeLog_4"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Props_TreeLog_5"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/Props_TreeLog_6"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/WallLamp"),
            GameObject.Find("Terrain/Terrain Props/Not walkable/Near Tavern/Fences/WallLamp_1")
        ];

        Vector3 pos1 = fenceOrigins[6].transform.position;
        Vector3 pos2 = fenceOrigins[7].transform.position;
        Vector3 offset = new Vector3(pos1.x - pos2.x, 0, pos1.z - pos2.z);

        for (int i = 0; i < fenceOrigins.Length; i++) {

            /* Clone all children and modify their MeshRenderer bounds m_center */

            GameObject fenceClone = Instantiate(fenceOrigins[i]);
            fenceClone.name = $"{fenceOrigins[i].name}_MoreFarmPlot";
            fenceClone.transform.position = fenceOrigins[i].transform.position + offset;

            fenceClone.transform.parent = fenceOrigins[i].transform.parent.transform;

        }

    }

    public static void ModifyTerrain(Vector3 prefabPosition, Vector3 firstCorner, Vector3 secondCorner) {

        GameObject terrainGameObject = GameObject.Find("Terrain/Terrain");
        Terrain terrain = terrainGameObject.GetComponent<Terrain>();

        Debug.Log("[MORE FARM PLOTS] Modifying Terrain under new farm plots");

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;

        int textureIndex = 7;

        // Convert world positions to terrain coordinates (normalized 0-1 range)
        Vector3 terrainStartPos = WorldToTerrainCoords(firstCorner, terrain.transform.position + new Vector3(2, 0, 2), terrainSize);
        Vector3 terrainEndPos = WorldToTerrainCoords(secondCorner, terrain.transform.position + new Vector3(-1, 0, -1), terrainSize);

        // Convert normalized coordinates to heightmap positions
        int startX = Mathf.RoundToInt(terrainStartPos.x * terrainData.detailWidth);
        int startY = Mathf.RoundToInt(terrainStartPos.z * terrainData.detailHeight);
        int endX = Mathf.RoundToInt(terrainEndPos.x * terrainData.detailWidth);
        int endY = Mathf.RoundToInt(terrainEndPos.z * terrainData.detailHeight);

        // Clear grass (details) in the area
        ClearGrass(terrainData, startX, startY, endX, endY);
        ClearAllGrass(terrainData, startX, startY, endX, endY);

        // Paint the terrain texture (splat map) in the area
        PaintTexture(terrainData, terrainStartPos, terrainEndPos, textureIndex);
    }

    static Vector3 WorldToTerrainCoords(Vector3 worldPos, Vector3 terrainPos, Vector3 terrainSize)
    {
        return new Vector3(
            (worldPos.x - terrainPos.x) / terrainSize.x,
            0,
            (worldPos.z - terrainPos.z) / terrainSize.z
        );
    }

    static void ClearGrass(TerrainData terrainData, int startX, int startY, int endX, int endY)
    {
        int[,] detailLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, 0);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                detailLayer[y, x] = 0; // Set grass amount to 0 (removing grass)
            }
        }

        terrainData.SetDetailLayer(0, 0, 0, detailLayer);
    }

    static void PaintTexture(TerrainData terrainData, Vector3 start, Vector3 end, int textureIndex)
    {
        int splatMapWidth = terrainData.alphamapWidth;
        int splatMapHeight = terrainData.alphamapHeight;

        int startX = Mathf.RoundToInt(start.x * splatMapWidth);
        int startY = Mathf.RoundToInt(start.z * splatMapHeight);
        int endX = Mathf.RoundToInt(end.x * splatMapWidth);
        int endY = Mathf.RoundToInt(end.z * splatMapHeight);

        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, splatMapWidth, splatMapHeight);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    alphaMap[y, x, i] = (i == textureIndex) ? 1.0f : 0.0f; // Apply selected texture
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMap);

        Debug.Log("Finished painting terrain under garden plot");

    }

    static void ClearAllGrass(TerrainData terrainData, int startX, int startY, int endX, int endY)
    {
        int detailLayers = terrainData.detailPrototypes.Length; // Get all detail layers (grass types)

        for (int layer = 0; layer < detailLayers; layer++) // Loop through all grass/detail layers
        {
            int[,] detailLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, layer);

            // Clear grass (set to 0) within the defined rectangle
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    detailLayer[y, x] = 0; // Clear this grass detail
                }
            }

            // Apply the cleared detail layer back to the terrain
            terrainData.SetDetailLayer(0, 0, layer, detailLayer);
        }
    }

}