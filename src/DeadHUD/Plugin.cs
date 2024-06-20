using BepInEx;
using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.Collections;

namespace DeadHUD
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    class Plugin : BaseUnityPlugin
    {
        public const string PluginId = "qwbarch.DeadHUD";
        public const string PluginName = "DeadHUD";
        public const string PluginVersion = "1.0.0";

        void Awake()
        {
            new Harmony(PluginId).PatchAll(typeof(ModifyHUD));
        }
    }

    class ModifyHUD
    {
        private const float IconSize = 1.2f;
        private const float IconOffset = .6f;

        private static float OffsetX = 0f;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ResetPlayersLoadedValueClientRpc))]
        static void ResetState()
        {
            OffsetX = 0f;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.UpdateBoxesSpectateUI))]
        static bool PatchUpdateBoxesSpectateUI(HUDManager __instance)
        {
            PlayerControllerB playerScript;

            // X position of the initial spectate icon.
            float IconX(RectTransform transform)
            {
                var x = transform.anchoredPosition.x * 1.4f + OffsetX;
                OffsetX += transform.rect.width * IconOffset;
                return x;
            }

            var containerHeight = __instance.SpectateBoxesContainer.GetComponent<RectTransform>().rect.height;
            float IconY(RectTransform transform)
            {
                return -containerHeight + transform.rect.height * 0.3f;
            }

            static void DisableHudElements(GameObject gameObject)
            {
                void disable(string target)
                {
                    gameObject.transform.Find(target).gameObject.SetActive(false);
                }
                disable("PlayerName");
                disable("DeadOrAlive");
                disable("SpeakerIcon");
            }

            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                playerScript = StartOfRound.Instance.allPlayerScripts[i];
                if (!playerScript.isPlayerDead)
                {
                    if (playerScript.isPlayerControlled || !__instance.spectatingPlayerBoxes.Values.Contains(playerScript))
                    {
                        continue;
                    }
                    Debug.Log("Removing player spectate box since they disconnected");
                    Animator key = __instance.spectatingPlayerBoxes.FirstOrDefault((KeyValuePair<Animator, PlayerControllerB> x) => x.Value == playerScript).Key;
                    if (key.gameObject.activeSelf)
                    {
                        for (int j = 0; j < __instance.spectatingPlayerBoxes.Count; j++)
                        {
                            RectTransform component = __instance.spectatingPlayerBoxes.ElementAt(j).Key.gameObject.GetComponent<RectTransform>();
                            if (component.anchoredPosition.y <= -70f * __instance.boxesAdded + 1f)
                            {
                                component.anchoredPosition = new Vector2(component.anchoredPosition.x, component.anchoredPosition.y + 70f);
                            }
                        }
                        __instance.yOffsetAmount += 70f;
                    }
                    __instance.spectatingPlayerBoxes.Remove(key);
                    UnityEngine.Object.Destroy(key.gameObject);
                }
                else if (__instance.spectatingPlayerBoxes.Values.Contains(playerScript))
                {
                    GameObject gameObject = __instance.spectatingPlayerBoxes.FirstOrDefault((KeyValuePair<Animator, PlayerControllerB> x) => x.Value == playerScript).Key.gameObject;
                    if (!gameObject.activeSelf)
                    {
                        DisableHudElements(gameObject);
                        RectTransform component2 = gameObject.GetComponent<RectTransform>();
                        component2.sizeDelta *= IconSize;
                        component2.anchoredPosition = new Vector2(IconX(component2), IconY(component2));
                        __instance.boxesAdded++;
                        gameObject.SetActive(value: true);
                    }
                }
                else
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(__instance.spectatingPlayerBoxPrefab, __instance.SpectateBoxesContainer, worldPositionStays: false);
                    DisableHudElements(gameObject);
                    gameObject.SetActive(value: true);
                    RectTransform component3 = gameObject.GetComponent<RectTransform>();
                    component3.sizeDelta *= IconSize;
                    component3.anchoredPosition = new Vector2(IconX(component3), IconY(component3));
                    __instance.boxesAdded++;
                    __instance.spectatingPlayerBoxes.Add(gameObject.GetComponent<Animator>(), playerScript);
                    if (!GameNetworkManager.Instance.disableSteam)
                    {
                        HUDManager.FillImageWithSteamProfile(gameObject.GetComponent<RawImage>(), playerScript.playerSteamId);
                    }
                }
            }
            return false;
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
        //static void UpdateDeadPlayerIcons()
        //{
        //    var deltaTime = Time.deltaTime;
        //}
    }
}