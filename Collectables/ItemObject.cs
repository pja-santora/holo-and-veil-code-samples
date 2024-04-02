using UnityEngine;
using System.Collections.Generic;

public class ItemObject : MonoBehaviour
{
    public readonly Dictionary<Type, System.Action<string>> ItemPickups = new Dictionary<Type, System.Action<string>> {
        { Type.useable, PickupUseable },
        { Type.key, PickupKey },
        { Type.ship_log, PickupShipLog },
        { Type.artifact, PickupArtifact },
    };

    public enum Type { useable, key, ship_log, artifact }
    public Type _type;

    public string _name;
    public AudioSource _audio;

    private bool Unique = true;

    public void Set(string name, bool unique = true)
    {
        _name = name;
        Unique = unique;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals(PlayerPlatformerController.player_collider))
        {
            ItemPickups[_type](_name);
            GetItem();
        }
    }

    private static void PickupUseable(string name)
    {
        Player.AddItem(ItemDescription.ItemCatalog[name]);
    }

    private static void PickupKey(string name)
    {
        Player.AddKeyItem(ItemDescription.KeyItemCatalog[name]);
    }

    private static void PickupShipLog(string name)
    {
        Player.AddShipLog(ItemDescription.ShipLogCatalog[name]);
    }

    private static void PickupArtifact(string name)
    {
        Player.AddArtifact(ItemDescription.ArtifactCatalog[name]);
    }

    private void GetItem()
    {
        // set the item get screen parameters
        if (_type == Type.useable)
        {
            if (Unique) ItemTracker.UseableOverworldReferences[SceneController.current_scene + " " + _name] = true;

            if (ItemTracker.Firsts[_name])
            {
                ItemTracker.Firsts[_name] = false;

                // instantiate the item get screen
                GameObject item_get_prefab = (GameObject)Instantiate(Resources.Load("Overworld/HUD/item_get"),
                    GameObject.Find("_HUD").transform);
                item_get_prefab.GetComponent<ItemGetScreen>().Set(ItemDescription.ItemCatalog[_name]);
            }
            else
            {
                GameObject small_item_get_prefab = (GameObject)Instantiate(Resources.Load("Overworld/HUD/small_item_get"),
                    GameObject.Find("_HUD").transform);
                small_item_get_prefab.GetComponent<SmallItemGetScreen>().Set(ItemDescription.ItemCatalog[_name]);
            }
        }
        else if (_type == Type.key)
        {
            // instantiate the item get screen
            GameObject item_get_prefab = (GameObject)Instantiate(Resources.Load("Overworld/HUD/item_get"),
                GameObject.Find("_HUD").transform);

            item_get_prefab.GetComponent<ItemGetScreen>().Set(ItemDescription.KeyItemCatalog[_name]);
        }
        else if (_type == Type.ship_log)
        {
            // instantiate the item get screen
            GameObject item_get_prefab = (GameObject)Instantiate(Resources.Load("Overworld/HUD/ship_log_get"),
                GameObject.Find("_HUD").transform);

            item_get_prefab.GetComponent<ShipLogGetScreen>().Set(ItemDescription.ShipLogCatalog[_name]);
        }
        else if (_type == Type.artifact)
        {
            if (Unique) ItemTracker.ArtifactOverworldReferences[SceneController.current_scene + " " + _name] = true;

            if (ItemTracker.Firsts[_name])
            {
                ItemTracker.Firsts[_name] = false;

                // instantiate the item get screen
                GameObject item_get_prefab = (GameObject)Instantiate(Resources.Load("Overworld/HUD/item_get"),
                    GameObject.Find("_HUD").transform);
                item_get_prefab.GetComponent<ItemGetScreen>().Set(ItemDescription.ArtifactCatalog[_name]);
            }
            else
            {
                GameObject small_item_get_prefab = (GameObject)Instantiate(Resources.Load("Overworld/HUD/small_item_get"),
                    GameObject.Find("_HUD").transform);
                small_item_get_prefab.GetComponent<SmallItemGetScreen>().Set(ItemDescription.ArtifactCatalog[_name]);
            }
        }

        Destroy(gameObject);
    }
}
